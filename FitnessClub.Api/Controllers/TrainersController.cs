using FitnessClub.Api.DTOs.Trainers;
using FitnessClub.Api.Mapping;
using FitnessClub.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FitnessClub.Api.Controllers;

/// <summary>
/// Тренери фітнес-клубу.
/// </summary>
[ApiController]
[Route("api/trainers")]
[Produces("application/json")]
public class TrainersController : ControllerBase
{
    private readonly ITrainerRepository _repository;

    public TrainersController(ITrainerRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Отримати список тренерів з можливістю пошуку.</summary>
    /// <param name="search">Пошук за ім'ям, прізвищем або спеціалізацією.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TrainerDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<TrainerDto>> GetAll(
        [FromQuery] string? search)
    {
        var query = _repository.GetAll().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t =>
                t.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.Specialization.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(query.OrderBy(t => t.Id).Select(t => t.ToDto()));
    }

    /// <summary>Отримати тренера за id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TrainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<TrainerDto> GetById(int id)
    {
        var trainer = _repository.GetById(id);

        if (trainer is null)
            return Problem(statusCode: 404, title: $"Тренера з id={id} не знайдено.");

        return Ok(trainer.ToDto());
    }

    /// <summary>Створити нового тренера.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TrainerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<TrainerDto> Create([FromBody] CreateTrainerDto dto)
    {
        if (_repository.EmailExists(dto.Email))
            return Problem(statusCode: 409, title: "Тренер з таким Email вже існує.");

        var created = _repository.Add(dto.ToModel());

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created.ToDto());
    }

    /// <summary>Повністю оновити дані тренера.</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TrainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<TrainerDto> Update(int id, [FromBody] UpdateTrainerDto dto)
    {
        if (_repository.GetById(id) is null)
            return Problem(statusCode: 404, title: $"Тренера з id={id} не знайдено.");

        if (_repository.EmailExists(dto.Email, exceptId: id))
            return Problem(statusCode: 409, title: "Тренер з таким Email вже існує.");

        var updated = dto.ToModel();
        updated.Id = id;

        _repository.Update(updated);

        return Ok(updated.ToDto());
    }

    /// <summary>Видалити тренера.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        if (!_repository.Delete(id))
            return Problem(statusCode: 404, title: $"Тренера з id={id} не знайдено.");

        return NoContent();
    }
}
