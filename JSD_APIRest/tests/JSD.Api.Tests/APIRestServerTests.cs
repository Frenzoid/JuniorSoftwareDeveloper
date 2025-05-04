using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

using JSD.Api.Dtos;
using JSD.Domain.Models;
using JSD.Common.Settings;
namespace JSD.Api.Tests;

using JSD.Infrastructure.Repositories;
using JSD.Common.Interfaces;
using Xunit;

// Integration tests for the API endpoints
public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly HttpClient _client;
  private readonly string _tempFile;

  // NOTE: So apparently each test is ran in a new instance of the test class, to avoid conflits would be better to use a temp file for each test.
  public ApiIntegrationTests(WebApplicationFactory<Program> factory)
  {
    // spin up a unique temp file per test-class-instance
    _tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
    if (File.Exists(_tempFile))
      File.Delete(_tempFile);

    // prepare Options<AppSettings> pointing at that temp file
    var inMemorySettings = new AppSettings { TasksFilePath = _tempFile };

    // build a factory that replaces ITaskRepository
    var customFactory = factory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureServices(services =>
      {
        // remove the existing JsonTaskRepository registration
        var descriptor = services.Single(d => d.ServiceType == typeof(ITaskRepository));
        services.Remove(descriptor);

        // register our test focused JsonTaskRepository(opts) singleton with the temp file
        // NOTE: This is one instance (singelton) per test, not per class or suite
        services.AddSingleton<ITaskRepository>(sp =>
          new JsonTaskRepository(
            Options.Create(inMemorySettings)
          )
        );
      });
    });

    // create a client from the our factory
    _client = customFactory.CreateClient();
  }

  private void AddTenant(string tenant)
  {
    _client.DefaultRequestHeaders.Remove("X-Tenant-ID");
    _client.DefaultRequestHeaders.Add("X-Tenant-ID", tenant);
  }

  [Fact]
  public async Task GetTasks_WithValidTenant_ReturnsEmptyList()
  {
    AddTenant("Frenzoid");
    var resp = await _client.GetAsync("/tasks");
    Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    var list = await resp.Content.ReadFromJsonAsync<List<TaskItem>>();
    Assert.NotNull(list);
    Assert.Empty(list);
  }

  [Fact]
  public async Task GetTasks_WithoutTenantHeader_ReturnsBadRequest()
  {
    _client.DefaultRequestHeaders.Remove("X-Tenant-ID");
    var resp = await _client.GetAsync("/tasks");
    Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
  }

  [Fact]
  public async Task GetTasks_WithBlockedTenant_ReturnsForbidden()
  {
    AddTenant("NotAllowed");
    var resp = await _client.GetAsync("/tasks");
    Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
  }

  [Fact]
  public async Task CreateTask_WithValidTenant_ReturnsCreated()
  {
    AddTenant("Frenzoid");
    var dto = new TaskDto { Description = "Test create" };
    var post = await _client.PostAsJsonAsync("/tasks", dto);
    Assert.Equal(HttpStatusCode.Created, post.StatusCode);
    var created = await post.Content.ReadFromJsonAsync<TaskItem>();
    Assert.NotNull(created);
    Assert.Equal(dto.Description, created.Description);
  }

  [Fact]
  public async Task CreateTask_WithoutBody_ReturnsBadRequest()
  {
    AddTenant("Frenzoid");
    var post = await _client.PostAsJsonAsync<TaskDto>("/tasks", null);
    Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
  }

  [Fact]
  public async Task UpdateTask_WithValidData_ReturnsOk()
  {
    AddTenant("Frenzoid");
    var dto = new TaskDto { Description = "Updatable" };
    var post = await _client.PostAsJsonAsync("/tasks", dto);
    var created = await post.Content.ReadFromJsonAsync<TaskItem>();
    Assert.NotNull(created);

    var put = await _client.PutAsJsonAsync($"/tasks/{created.Id}", new TaskDto { Description = "Updated" });
    Assert.Equal(HttpStatusCode.OK, put.StatusCode);
    var updated = await put.Content.ReadFromJsonAsync<TaskItem>();
    Assert.NotNull(updated);
    Assert.Equal("Updated", updated.Description);
  }

  [Fact]
  public async Task UpdateTask_NotFound_ReturnsNotFound()
  {
    AddTenant("Frenzoid");
    var put = await _client.PutAsJsonAsync("/tasks/nonexistent-id", new TaskDto { Description = "X" });
    Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
  }

  [Fact]
  public async Task UpdateTask_NotOwned_ReturnsForbidden()
  {
    AddTenant("Frenzoid");
    var dto1 = new TaskDto { Description = "Owner1" };
    var post1 = await _client.PostAsJsonAsync("/tasks", dto1);
    var created = await post1.Content.ReadFromJsonAsync<TaskItem>();
    // print entire response for debugging
    Console.WriteLine(await post1.Content.ReadAsStringAsync());
    Assert.NotNull(created);

    // Use different tenant to attempt update
    AddTenant("OtherTenant");
    var put = await _client.PutAsJsonAsync($"/tasks/{created.Id}", new TaskDto { Description = "Hack" });
    Assert.Equal(HttpStatusCode.Forbidden, put.StatusCode);
  }

  [Fact]
  public async Task GetTasks_ReturnsOnlyTenantTasks()
  {
    // Create with TenantA
    AddTenant("TenantA");
    var dtoA = new TaskDto { Description = "A" };
    var postA = await _client.PostAsJsonAsync("/tasks", dtoA);
    var createdA = await postA.Content.ReadFromJsonAsync<TaskItem>();

    // Create with TenantB
    AddTenant("TenantB");
    var dtoB = new TaskDto { Description = "B" };
    var postB = await _client.PostAsJsonAsync("/tasks", dtoB);
    var createdB = await postB.Content.ReadFromJsonAsync<TaskItem>();

    // Verify TenantA sees only A
    AddTenant("TenantA");
    var listA = await _client.GetFromJsonAsync<List<TaskItem>>("/tasks");
    Assert.Single(listA);
    Assert.Equal("A", listA![0].Description);

    // Verify TenantB sees only B
    AddTenant("TenantB");
    var listB = await _client.GetFromJsonAsync<List<TaskItem>>("/tasks");
    Assert.Single(listB);
    Assert.Equal("B", listB![0].Description);
  }
}
