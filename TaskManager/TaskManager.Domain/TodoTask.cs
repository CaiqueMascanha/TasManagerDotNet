using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Domain;

/// <summary>
/// Representa a Entidade de Domínio que rege as regras de uma Tarefa.
/// </summary>
/// 
public sealed class TodoTask
{
    // Identificador único universal da tarefa
    public Guid Id { get; private set; }

    public string Title { get; private set; }
    public string Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Construtor principal que garante a consistência do estado na criação
    public TodoTask(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("O título da tarefa não pode ser nulo ou vazio");
        }

        Id = Guid.NewGuid();
        Title = title;
        Description = description ?? string.Empty;
        IsCompleted = false;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Construtor privado exigido pelo ORM (Entity Framework Core) para materialização dos dados.
    /// Evita que desenvolvedores instanciem a entidade sem as validações de negócio do construtor principal.
    /// </summary>
    private TodoTask() 
    {
        Title = null!;
        Description = null!;
    }

    public void Complete()
    {
        if (IsCompleted)
            throw new InvalidOperationException("Esta tarefa já está concluída.");

        IsCompleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("O título não pode ser nulo ou vazio.", nameof(title));

        Title = title;
        Description = description ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;

    }

}
