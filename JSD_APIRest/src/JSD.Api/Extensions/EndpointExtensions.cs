using JSD.Api.Dtos;
using JSD.Domain.Models;
using JSD.Infrastructure.Services;
using JSD.Common.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace JSD.Api.Extensions;

// Routers for the API endpoints and controllers
public static class EndpointExtensions
{
  public static void MapTaskEndpoints(this WebApplication app)
  {
    app.MapPost("/tasks", CreateTask);
    app.MapGet("/tasks", GetTasks);
    app.MapPut("/tasks/{id}", UpdateTask);
  }

  private static async Task<IResult> CreateTask(
      // I think I can use [FromBody] to tell the framework to use the body of the request as the DTO
      [FromBody] TaskDto? dto,
      ITaskRepository repo,
      IDocuWareService dw,
      HttpContext ctx)
  {

    // Check if the DTO is null or empty, return 400 Bad Request if it is
    // NOTE: I found out about this causality after running the tests, so we cover empty strings and nulls here
    if (dto is null || string.IsNullOrWhiteSpace(dto.Description))
      return Results.BadRequest("Description is required in a JSON format.");

    // Get the tenant ID from the request context ( we already verified it exists on the middleware )
    var tenant = ctx.Items["TenantId"] as string;

    // Create a new task item and add it to the repository
    var task = new TaskItem(
        Guid.NewGuid().ToString(),
        dto.Description,
        DateTime.UtcNow,
        tenant!
    );

    // Add the task to the repository ( JSON file )
    await repo.AddAsync(task);

    // Add the task to DocuWare
    await dw.AddRecordAsync(task);

    return Results.Created($"/tasks/{task.Id}", task);
  }

  private static async Task<IResult> GetTasks(
      ITaskRepository repo,
      HttpContext ctx)
  {
    var tenant = ctx.Items["TenantId"] as string;

    // Get all tasks for the tenant
    var tasks = await repo.GetByTenantAsync(tenant!);
    return Results.Ok(tasks);
  }

  private static async Task<IResult> UpdateTask(
      string id,
      [FromBody] TaskDto? dto,
      ITaskRepository repo,
      HttpContext ctx)
  {
    // Check if the DTO is null or empty, return 400 Bad Request if it is
    if (dto is null || string.IsNullOrWhiteSpace(dto.Description))
      return Results.BadRequest("Description is required in a JSON format.");

    var tenant = ctx.Items["TenantId"] as string;

    // Check if the task exists and update it ( returns the updated task or false )
    var updated = await repo.UpdateAsync(tenant!, id, dto.Description);

    // Task was not found, return 404
    if (updated is string _s1 && _s1 == "notFound")
      return Results.NotFound($"Task with ID {id} not found.");

    // Task was not owned by the tenant, return 403
    // NOTE: I recall from my BSc that security practices tell that we actually should not even tell the user if the task was not found or not owned by the tenant
    //   but in this case, given that its an exercise, we can be a bit more verbose i suppose
    if (updated is string _s2 && _s2 == "notOwned")
      return Results.Problem(
          detail: $"Task with ID {id} not owned by tenant {tenant}.",
          statusCode: StatusCodes.Status403Forbidden);

    // Task was found and updated, return the updated task
    if (updated is TaskItem task)
      return Results.Ok(task);

    // Santiy Check: This should not happen, but just in case
    return Results.BadRequest("Failed to update task ( how did you get here? ).");
  }
}
