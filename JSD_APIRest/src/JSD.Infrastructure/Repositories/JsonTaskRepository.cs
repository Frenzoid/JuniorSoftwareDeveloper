using System.Text.Json;
using Microsoft.Extensions.Options;

using JSD.Domain.Models;
using JSD.Common.Interfaces;
using JSD.Common.Settings;

namespace JSD.Infrastructure.Repositories;

// Repository for the tasks, using a JSON file as the storage and a semaphore to handle concurrency
// NOTE: Would a queue instead of a semaphore be more appropriate for this exercise?
public class JsonTaskRepository(IOptions<AppSettings> opts) : ITaskRepository
{
  private readonly string _path = opts.Value.TasksFilePath;
  private readonly SemaphoreSlim _mutex = new(1, 1);

  // ---- Private methods ----
  // Read all tasks from the JSON file and return them as a list of TaskItem objects
  private async Task<List<TaskItem>> ReadAllAsync()
  {
    // Check if the file exists, if not return an empty list
    if (!File.Exists(_path)) return new();

    // Read the file and Deserialize the JSON file or return an empty list
    await using var s = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
    return await JsonSerializer.DeserializeAsync<List<TaskItem>>(s) ?? new List<TaskItem>();
  }

  // Write all tasks to the JSON file, overwriting the existing file
  private async Task WriteAllAsync(List<TaskItem> list)
  {
    Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
    await using var s = File.Open(_path, FileMode.Create, FileAccess.Write, FileShare.None);
    await JsonSerializer.SerializeAsync(s, list, new JsonSerializerOptions { WriteIndented = true });
  }


  // ---- Public methods ----
  public async Task AddAsync(TaskItem t)
  {
    // Wait for the semaphore to be available before proceeding
    await _mutex.WaitAsync();
    try
    {
      // Weread all, add new and overwrite the file with the new task
      List<TaskItem> allTasks = await ReadAllAsync();
      allTasks.Add(t);
      await WriteAllAsync(allTasks);
    }
    finally { _mutex.Release(); }
  }

  public async Task<List<TaskItem>> GetByTenantAsync(string tenant)
  {
    List<TaskItem> allTasks = await ReadAllAsync();
    // NOTE: Cool thing i found out, so we can use the range operator to get a behavior similar to the spread operator in JS
    //   maybe its a bit messy, but its a one liner, and it looks cool ( we can always use .toList() if we want )
    return [.. allTasks.Where(x => x.TenantId == tenant)];
  }

  public async Task<object> UpdateAsync(string tenant, string id, string desc)
  {
    await _mutex.WaitAsync();
    try
    {
      List<TaskItem> allTasks = await ReadAllAsync();
      TaskItem? task = allTasks.FirstOrDefault(x => x.Id == id);

      // Check if the task exists
      if (task is null)
        return "notFound";

      // Check if the task belongs to the tenant
      if (task.TenantId != tenant)
        return "notOwned";

      // Get the index of the task in the list
      int idx = allTasks.IndexOf(task);

      // Update the task description with the new value
      TaskItem updatedTask = allTasks[idx] with { Description = desc };
      allTasks[idx] = updatedTask;
      await WriteAllAsync(allTasks);

      // return updated task item
      return allTasks[idx];
    }
    finally { _mutex.Release(); }
  }
}