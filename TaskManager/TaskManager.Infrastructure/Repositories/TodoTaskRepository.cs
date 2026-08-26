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

    public async Task<TodoTask?> GetByIdAsync(Guid id)
    {
        return await _context.TodoTasks.FindAsync(id);
    }

    public async Task<IEnumerable<TodoTask>> GetAllAsync()
    {
        return await _context.TodoTasks.AsNoTracking().ToListAsync();
    }

    public async Task AddAsync(TodoTask task)
    {
        await _context.TodoTasks.AddAsync(task);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TodoTask task)
    {
        _context.TodoTasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TodoTask task)
    {
        _context.TodoTasks.Remove(task);
        await _context.SaveChangesAsync();
    }
}
