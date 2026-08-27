using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TaskManager.Domain;
using TaskManager.Infrastructure.Context;

namespace TaskManager.Infrastructure.Repositories;

/// <summary>
/// Implementação concreta do repositório utilizando EF Core.
/// </summary>
public sealed class TodoTaskRepository : ITodoTaskRepository
{
    private readonly AppDbContext _context;

    public TodoTaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TodoTask?> GetTaskByIdAsync(Guid id)
    {
        return await _context.TodoTasks.FindAsync(id);
    }

    public async Task<IEnumerable<TodoTask>> GetAllTaskAsync()
    {
        // Instrução do EF Core que diz para o ORM não rastrear o estado dessas entidades na memória.
        // Essencial para otimizar a performance em consultas puras de leitura (GETs)
        return await _context.TodoTasks.AsNoTracking().ToListAsync();
    }

    public async Task AddTaskAsync(TodoTask task)
    {
        await _context.TodoTasks.AddAsync(task);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTaskAsync(TodoTask task)
    {
        _context.TodoTasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTaskAsync(TodoTask task)
    {
        _context.TodoTasks.Remove(task);
        await _context.SaveChangesAsync();
    }
}
