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
    Task<TodoTask?> GetByIdAsync(Guid id);
    Task<IEnumerable<TodoTask>> GetAllAsync();
    Task AddAsync(TodoTask task);
    Task UpdateAsync(TodoTask task);
    Task DeleteAsync(TodoTask task);
}
