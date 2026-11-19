using FitnessClub.Api.DTOs.Workouts;
using FitnessClub.Api.Mapping;
using FitnessClub.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FitnessClub.Api.Controllers;

/// <summary>
/// Групові тренування (разом з розкладом).
/// </summary>
[ApiController]
[Route("api/workouts")]
[Produces("application/json")]
public class WorkoutsController : ControllerBase
{
    private readonly IWorkoutRepository _workouts;

    public WorkoutsController(IWorkoutRepository workouts)
    {
        _workouts = workouts;
    }

    /// <summary>Отримати розклад тренувань з фільтрацією, пошуком і сортуванням.</summary>
    /// <remarks>Приклад: GET /api/workouts?day=Monday&amp;level=Beginner&amp;membershipId=3</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkoutDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<WorkoutDto>>> GetAll([FromQuery] WorkoutQueryParameters query)
    {
        var workouts = await _workouts.GetAllAsync(query);
        return Ok(workouts.Select(w => w.ToDto()));
    }

    /// <summary>Отримати тренування за id разом з абонементами, до яких воно входить.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WorkoutDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkoutDetailsDto>> GetById(int id)
    {
        var workout = await _workouts.GetWithMembershipsAsync(id);
        if (workout is null)
            return WorkoutNotFound(id);

        return Ok(workout.ToDetailsDto());
    }

    /// <summary>Створити нове тренування.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(WorkoutDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkoutDto>> Create([FromBody] CreateWorkoutDto dto)
    {
        var workout = dto.ToEntity();
        await _workouts.AddAsync(workout);

        return CreatedAtAction(nameof(GetById), new { id = workout.Id }, workout.ToDto());
    }

    /// <summary>Повністю оновити тренування.</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(WorkoutDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkoutDto>> Update(int id, [FromBody] UpdateWorkoutDto dto)
    {
        var workout = await _workouts.GetByIdAsync(id);
        if (workout is null)
            return WorkoutNotFound(id);

        dto.ApplyTo(workout);
        await _workouts.SaveChangesAsync();

        return Ok(workout.ToDto());
    }

    /// <summary>Видалити тренування (зв'язки з абонементами видаляються каскадно).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var workout = await _workouts.GetByIdAsync(id);
        if (workout is null)
            return WorkoutNotFound(id);

        await _workouts.DeleteAsync(workout);
        return NoContent();
    }

    private ObjectResult WorkoutNotFound(int id) =>
        Problem(statusCode: 404, title: $"Тренування з id={id} не знайдено.");
}
