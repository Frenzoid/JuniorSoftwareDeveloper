namespace JSD.Domain.Models;

// This record represents a task item, it contains properties for the task's ID, description, creation date, and tenant ID.
public record TaskItem(
    string Id,
    string Description,
    DateTime CreationDate,
    string TenantId
);
