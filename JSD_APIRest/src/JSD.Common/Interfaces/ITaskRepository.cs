using JSD.Domain.Models;

namespace JSD.Common.Interfaces;

// Repository interface for the JSON Repository handling tasks
public interface ITaskRepository
{
  Task AddAsync(TaskItem task);
  Task<List<TaskItem>> GetByTenantAsync(string tenant);
  Task<object> UpdateAsync(string tenant, string id, string description);
}
