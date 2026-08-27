using Microsoft.AspNetCore.Mvc;
using TaskManager.API.DTOs;
using TaskManager.Domain;

namespace TaskManager.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class TodoTasksController : ControllerBase
{
    private readonly ITodoTaskRepository _repository;

    // Injeção de dependência via Construtor
    public TodoTasksController(ITodoTaskRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        try
        {
            var task = new TodoTask(request.Title, request.Description);
            await _repository.AddTaskAsync(task);

            var response = new TaskResponse(task.Id, task.Title, task.Description, task.IsCompleted, task.CreatedAt, task.UpdatedAt);

            // Retorna o status HTTP 201 (Created) apontando para o endpoint de busca por 
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, response);

        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var task = await _repository.GetTaskByIdAsync(id);
        if (task == null)
            return NotFound();

        var response = new TaskResponse(task.Id, task.Title, task.Description, task.IsCompleted, task.CreatedAt, task.UpdatedAt);

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await _repository.GetAllTaskAsync();

        var response = tasks.Select(t => new TaskResponse(t.Id, t.Title, t.Description, t.IsCompleted, t.CreatedAt, t.UpdatedAt));

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request)
    {
        try
        {
            var task = await _repository.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            task.UpdateDetails(request.Title, request.Description);
            await _repository.UpdateTaskAsync(task);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error =ex.Message });
        }
    }

    [HttpPut("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        try
        {
            var task = await _repository.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

            task.Complete();
            await _repository.UpdateTaskAsync(task);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
