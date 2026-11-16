using FitnessClub.Api.DTOs.Workouts;
using FitnessClub.Api.Mapping;
using FitnessClub.Api.Models;
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
    private readonly IWorkoutRepository _repository;

    public WorkoutsController(IWorkoutRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Отримати розклад тренувань з фільтрацією.</summary>
    /// <param name="day">День тижня, наприклад Monday.</param>
    /// <param name="level">Рівень складності: Beginner, Intermediate, Advanced.</param>
    /// <param name="trainerId">Лише тренування цього тренера.</param>
    /// <param name="search">Пошук за назвою або описом.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkoutDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<IEnumerable<WorkoutDto>> GetAll(
        [FromQuery] DayOfWeek? day,
        [FromQuery] DifficultyLevel? level,
        [FromQuery] int? trainerId,
        [FromQuery] string? search)
    {
        var query = _repository.GetAll().AsEnumerable();

        if (day.HasValue)
            query = query.Where(w => w.DayOfWeek == day.Value);
        if (level.HasValue)
            query = query.Where(w => w.Level == level.Value);
        if (trainerId.HasValue)
            query = query.Where(w => w.TrainerId == trainerId.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(w =>
                w.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (w.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));

        // Сортуємо як розклад: понеділок → неділя, потім за часом.
        var result = query
            .OrderBy(w => ((int)w.DayOfWeek + 6) % 7)
            .ThenBy(w => w.StartTime)
            .Select(w => w.ToDto());

        return Ok(result);
    }

    /// <summary>Отримати тренування за id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WorkoutDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<WorkoutDto> GetById(int id)
    {
        var workout = _repository.GetById(id);
        if (workout is null)
            return Problem(statusCode: 404, title: $"Тренування з id={id} не знайдено.");

        return Ok(workout.ToDto());
    }

    /// <summary>Створити нове тренування.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(WorkoutDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<WorkoutDto> Create([FromBody] CreateWorkoutDto dto)
    {
        var created = _repository.Add(dto.ToModel());
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
    }

    /// <summary>Повністю оновити тренування.</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(WorkoutDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<WorkoutDto> Update(int id, [FromBody] UpdateWorkoutDto dto)
    {
        if (_repository.GetById(id) is null)
            return Problem(statusCode: 404, title: $"Тренування з id={id} не знайдено.");

        var updated = dto.ToModel();
        updated.Id = id;
        _repository.Update(updated);

        return Ok(updated.ToDto());
    }

    /// <summary>Видалити тренування.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        if (!_repository.Delete(id))
            return Problem(statusCode: 404, title: $"Тренування з id={id} не знайдено.");

        return NoContent();
    }
}
