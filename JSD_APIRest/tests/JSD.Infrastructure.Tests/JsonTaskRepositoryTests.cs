using Microsoft.Extensions.Options;
using JSD.Common.Settings;
using JSD.Domain.Models;
using JSD.Infrastructure.Repositories;

namespace JSD.Infrastructure.Tests
{

  // Tests for the JsonTaskRepository class
  public class JsonTaskRepositoryTests : IDisposable
  {
    private readonly string _tempFile;
    private readonly JsonTaskRepository _repo;

    public JsonTaskRepositoryTests()
    {
      _tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
      // ensure no leftover
      if (File.Exists(_tempFile)) File.Delete(_tempFile);

      var opts = Options.Create(new AppSettings { TasksFilePath = _tempFile });
      _repo = new JsonTaskRepository(opts);
    }

    // NOTE: Even tho were not using this method, apparently the test framework will call it to clean up after itself
    //    This was pretty hard to find out in the docs
    public void Dispose()
    {
      if (File.Exists(_tempFile)) File.Delete(_tempFile);
    }

    [Fact]
    public async Task GetByTenant_WhenNoTasks_ReturnsEmptyList()
    {
      var list = await _repo.GetByTenantAsync("AnyTenant");
      Assert.Empty(list);
    }

    [Fact]
    public async Task AddAndGetByTenant_ShouldReturnOnlyThatTenantTasks()
    {
      var t1 = new TaskItem("id1", "first", DateTime.UtcNow, "TenantA");
      var t2 = new TaskItem("id2", "second", DateTime.UtcNow, "TenantB");

      await _repo.AddAsync(t1);
      await _repo.AddAsync(t2);

      var listA = await _repo.GetByTenantAsync("TenantA");
      var listB = await _repo.GetByTenantAsync("TenantB");

      Assert.Single(listA);
      Assert.Equal("id1", listA.Single().Id);

      Assert.Single(listB);
      Assert.Equal("id2", listB.Single().Id);
    }

    [Fact]
    public async Task AddAsync_IsPersistentAcrossInstances()
    {
      var task = new TaskItem("persist", "p", DateTime.UtcNow, "T");
      await _repo.AddAsync(task);

      // new repo instance reading same file
      var repo2 = new JsonTaskRepository(
        Options.Create(new AppSettings { TasksFilePath = _tempFile })
      );
      var list = await repo2.GetByTenantAsync("T");
      Assert.Single(list);
      Assert.Equal("persist", list.Single().Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenExists_ShouldModifyAndReturnTaskItem()
    {
      var original = new TaskItem("x", "orig", DateTime.UtcNow, "T1");
      await _repo.AddAsync(original);

      var result = await _repo.UpdateAsync("T1", "x", "newdesc");
      Assert.IsType<TaskItem>(result);

      var updated = (TaskItem)result;
      Assert.Equal("newdesc", updated.Description);

      // round-trip check
      var list = await _repo.GetByTenantAsync("T1");
      Assert.Equal("newdesc", list.Single().Description);
    }

    [Fact]
    public async Task UpdateAsync_WhenIdNotFound_ShouldReturnNotFound()
    {
      var res = await _repo.UpdateAsync("T", "does-not-exist", "whatever");
      Assert.Equal("notFound", res);
    }

    [Fact]
    public async Task UpdateAsync_WhenWrongTenant_ShouldReturnNotOwned()
    {
      var task = new TaskItem("y", "orig", DateTime.UtcNow, "T1");
      await _repo.AddAsync(task);

      var res = await _repo.UpdateAsync("T2", "y", "new");
      Assert.Equal("notOwned", res);
    }
  }
}
