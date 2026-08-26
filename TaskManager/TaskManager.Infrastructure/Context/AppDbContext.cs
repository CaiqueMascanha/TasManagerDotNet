using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TaskManager.Domain;

namespace TaskManager.Infrastructure.Context;

/// <summary>
/// Contexto do EF Core responsável por mapear as entidades para o SQL Server.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public DbSet<TodoTask> TodoTasks { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configuração explícita (Fluent API) substituindo anotações de dados (Data Annotations).
    /// Mantém as entidades de Domínio limpas de acoplamento com o banco.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoTask>(builder =>
        {
            builder.ToTable("TodoTasks");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
            .IsRequired()

            .HasMaxLength(150);

            builder.Property(t => t.Description)
            .HasMaxLength(1000);

            builder.Property(t => t.IsCompleted)
            .IsRequired();

            builder.Property(t => t.CreatedAt)
            .IsRequired();

            builder.Property(t => t.UpdatedAt);

        });

        base.OnModelCreating(modelBuilder);
    }
}
