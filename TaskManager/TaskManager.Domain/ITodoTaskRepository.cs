using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Domain;

/// <summary>
/// Contrato do Repositório para a Entidade TodoTask.
/// Seguindo o DIP, a interface reside no Domínio.
/// </summary>
public interface ITodoTaskRepository
{
    Task<TodoTask?> GetTaskByIdAsync(Guid id);
    Task<IEnumerable<TodoTask>> GetAllTaskAsync();
    Task AddTaskAsync(TodoTask task);
    Task UpdateTaskAsync(TodoTask task);
    Task DeleteTaskAsync(TodoTask task);
}
