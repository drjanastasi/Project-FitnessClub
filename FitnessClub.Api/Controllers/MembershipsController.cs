using FitnessClub.Api.DTOs.Memberships;
using FitnessClub.Api.Mapping;
using FitnessClub.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FitnessClub.Api.Controllers;

/// <summary>
/// Абонементи фітнес-клубу та тренування, що до них входять.
/// </summary>
[ApiController]
[Route("api/memberships")]
[Produces("application/json")]
public class MembershipsController : ControllerBase
{
    private readonly IMembershipRepository _memberships;
    private readonly IWorkoutRepository _workouts;

    public MembershipsController(IMembershipRepository memberships, IWorkoutRepository workouts)
    {
        _memberships = memberships;
        _workouts = workouts;
    }

    /// <summary>Отримати список абонементів з фільтрацією, пошуком і сортуванням.</summary>
    /// <remarks>Приклад: GET /api/memberships?isActive=true&amp;maxPrice=1000&amp;sortBy=price&amp;desc=true</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MembershipDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<MembershipDto>>> GetAll([FromQuery] MembershipQueryParameters query)
    {
        var memberships = await _memberships.GetAllAsync(query);
        return Ok(memberships.Select(m => m.ToDto()));
    }

    /// <summary>Отримати абонемент за id разом зі списком тренувань, що до нього входять.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MembershipDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MembershipDetailsDto>> GetById(int id)
    {
        var membership = await _memberships.GetWithWorkoutsAsync(id);
        if (membership is null)
            return MembershipNotFound(id);

        return Ok(membership.ToDetailsDto());
    }

    /// <summary>Створити новий абонемент.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MembershipDto>> Create([FromBody] CreateMembershipDto dto)
    {
        // Невалідні DTO автоматично отримують 400 завдяки [ApiController].
        if (await _memberships.NameExistsAsync(dto.Name))
            return Problem(statusCode: 409, title: $"Абонемент з назвою \"{dto.Name}\" вже існує.");

        var membership = dto.ToEntity();
        await _memberships.AddAsync(membership);

        return CreatedAtAction(nameof(GetById), new { id = membership.Id }, membership.ToDto());
    }

    /// <summary>Повністю оновити абонемент.</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MembershipDto>> Update(int id, [FromBody] UpdateMembershipDto dto)
    {
        var membership = await _memberships.GetByIdAsync(id);
        if (membership is null)
            return MembershipNotFound(id);

        if (await _memberships.NameExistsAsync(dto.Name, exceptId: id))
            return Problem(statusCode: 409, title: $"Абонемент з назвою \"{dto.Name}\" вже існує.");

        dto.ApplyTo(membership);
        await _memberships.SaveChangesAsync();

        return Ok(membership.ToDto());
    }

    /// <summary>Активувати або деактивувати абонемент (часткове оновлення).</summary>
    [HttpPatch("{id:int}/status")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MembershipDto>> UpdateStatus(int id, [FromBody] UpdateMembershipStatusDto dto)
    {
        var membership = await _memberships.GetByIdAsync(id);
        if (membership is null)
            return MembershipNotFound(id);

        membership.IsActive = dto.IsActive!.Value;
        await _memberships.SaveChangesAsync();

        return Ok(membership.ToDto());
    }

    /// <summary>Видалити абонемент (його зв'язки з тренуваннями видаляються каскадно).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var membership = await _memberships.GetByIdAsync(id);
        if (membership is null)
            return MembershipNotFound(id);

        await _memberships.DeleteAsync(membership);
        return NoContent();
    }

    // ---------- Зв'язок абонемент ↔ тренування ----------

    /// <summary>Додати тренування в абонемент.</summary>
    [HttpPost("{id:int}/workouts/{workoutId:int}")]
    [ProducesResponseType(typeof(MembershipDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MembershipDetailsDto>> AddWorkout(int id, int workoutId)
    {
        if (!await _memberships.ExistsAsync(id))
            return MembershipNotFound(id);

        if (!await _workouts.ExistsAsync(workoutId))
            return Problem(statusCode: 404, title: $"Тренування з id={workoutId} не знайдено.");

        if (await _memberships.HasWorkoutAsync(id, workoutId))
            return Problem(statusCode: 409, title: "Це тренування вже входить в абонемент.");

        await _memberships.AddWorkoutAsync(id, workoutId);

        var updated = await _memberships.GetWithWorkoutsAsync(id);
        return Ok(updated!.ToDetailsDto());
    }

    /// <summary>Прибрати тренування з абонемента.</summary>
    [HttpDelete("{id:int}/workouts/{workoutId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveWorkout(int id, int workoutId)
    {
        if (!await _memberships.RemoveWorkoutAsync(id, workoutId))
            return Problem(statusCode: 404, title: "Таке тренування не входить в цей абонемент.");

        return NoContent();
    }

    private ObjectResult MembershipNotFound(int id) =>
        Problem(statusCode: 404, title: $"Абонемент з id={id} не знайдено.");
}
