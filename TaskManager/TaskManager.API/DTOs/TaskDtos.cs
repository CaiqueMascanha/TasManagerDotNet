namespace TaskManager.API.DTOs;

/// <summary>
/// DTOs de Entrada e Saída fortemente tipados e imutáveis.
/// </summary>
public sealed record CreateTaskRequest(string Title, string Description);

public sealed record UpdateTaskRequest(string Title, string Description);

public sealed record TaskResponse(
    Guid Id,
    string Title,
    string Description,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
  


