using Microsoft.EntityFrameworkCore;
using TaskManager.Domain;
using TaskManager.Infrastructure.Context;
using TaskManager.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrar os controladores no container de DI
builder.Services.AddControllers();

// 2. Configurar o Swagger/OpenAPI para documentar e testar os Endpoints
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Configurar a conexão com o banco SQL Server do Docker
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 4. Injeção de Dependência: Acoplando a interface à implementação concreta do repositório
builder.Services.AddScoped<ITodoTaskRepository, TodoTaskRepository>();

var app = builder.Build();

// Configurando os Middlewares (Pipeline de Requisições HTTP)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Inicialização Facilitada: Aplica as migrations automaticamente ao subir a API
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.Run();