using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using TaskManager.Domain;
using TaskManager.Infrastructure.Context;
using TaskManager.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);


// =========================================================
// 1. CONTROLLERS
// =========================================================

builder.Services.AddControllers();


// =========================================================
// 2. AUTENTICAÇÃO - MICROSOFT ENTRA ID
// =========================================================

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(
        builder.Configuration.GetSection("AzureAd")
    );


// =========================================================
// 3. AUTORIZAÇÃO
// =========================================================

builder.Services.AddAuthorization();


// =========================================================
// 4. SWAGGER / OPENAPI
// =========================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "TaskManager API",
        Version = "v1",
        Description = "API para CRUD de tasks"
    });


    // -----------------------------------------------------
    // Define o mecanismo de autenticação
    // -----------------------------------------------------

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Informe o token JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });


    // -----------------------------------------------------
    // Diz que os endpoints utilizam esse mecanismo
    // -----------------------------------------------------

    c.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "Bearer",
                document
            )] = []
        });
});


// =========================================================
// 5. BANCO DE DADOS
// =========================================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});


// =========================================================
// 6. DEPENDENCY INJECTION
// =========================================================

builder.Services.AddScoped<
    ITodoTaskRepository,
    TodoTaskRepository
>();


// =========================================================
// BUILD
// =========================================================

var app = builder.Build();


// =========================================================
// PIPELINE HTTP
// =========================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();


// =========================================================
// SEGURANÇA
// =========================================================

// Quem é você?
app.UseAuthentication();

// O que você pode fazer?
app.UseAuthorization();


// =========================================================
// CONTROLLERS
// =========================================================

app.MapControllers();


// =========================================================
// MIGRATIONS
// =========================================================

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider.GetRequiredService<AppDbContext>();

    dbContext.Database.Migrate();
}


app.Run();