using FitnessClub.Api.DTOs.Memberships;
using FitnessClub.Api.Exceptions;
using FitnessClub.Api.Mapping;
using FitnessClub.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FitnessClub.Api.Controllers;

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

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MembershipDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<MembershipDto>>> GetAll([FromQuery] MembershipQueryParameters query)
    {
        var memberships = await _memberships.GetAllAsync(query);
        return Ok(memberships.Select(m => m.ToDto()));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MembershipDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MembershipDetailsDto>> GetById(int id)
    {
        var membership = await _memberships.GetWithWorkoutsAsync(id)
            ?? throw MembershipNotFound(id);

        return Ok(membership.ToDetailsDto());
    }
    
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MembershipDto>> Create([FromBody] CreateMembershipDto dto)
    {
        // Невалідні DTO автоматично отримують 400 завдяки [ApiController].
        if (await _memberships.NameExistsAsync(dto.Name))
            throw DuplicateName(dto.Name);

        var membership = dto.ToEntity();
        await _memberships.AddAsync(membership);

        return CreatedAtAction(nameof(GetById), new { id = membership.Id }, membership.ToDto());
    }

    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MembershipDto>> Update(int id, [FromBody] UpdateMembershipDto dto)
    {
        var membership = await _memberships.GetByIdAsync(id)
            ?? throw MembershipNotFound(id);

        if (await _memberships.NameExistsAsync(dto.Name, exceptId: id))
            throw DuplicateName(dto.Name);

        dto.ApplyTo(membership);
        await _memberships.SaveChangesAsync();

        return Ok(membership.ToDto());
    }

    [HttpPatch("{id:int}/status")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MembershipDto>> UpdateStatus(int id, [FromBody] UpdateMembershipStatusDto dto)
    {
        var membership = await _memberships.GetByIdAsync(id)
            ?? throw MembershipNotFound(id);

        membership.IsActive = dto.IsActive!.Value;
        await _memberships.SaveChangesAsync();

        return Ok(membership.ToDto());
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var membership = await _memberships.GetByIdAsync(id)
            ?? throw MembershipNotFound(id);

        await _memberships.DeleteAsync(membership);
        return NoContent();
    }

    [HttpPost("{id:int}/workouts/{workoutId:int}")]
    [ProducesResponseType(typeof(MembershipDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MembershipDetailsDto>> AddWorkout(int id, int workoutId)
    {
        if (!await _memberships.ExistsAsync(id))
            throw MembershipNotFound(id);

        if (!await _workouts.ExistsAsync(workoutId))
            throw new NotFoundException($"Тренування з id={workoutId} не знайдено.");

        if (await _memberships.HasWorkoutAsync(id, workoutId))
            throw new ConflictException("Це тренування вже входить в абонемент.");

        await _memberships.AddWorkoutAsync(id, workoutId);

        var updated = await _memberships.GetWithWorkoutsAsync(id);
        return Ok(updated!.ToDetailsDto());
    }

    [HttpDelete("{id:int}/workouts/{workoutId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveWorkout(int id, int workoutId)
    {
        if (!await _memberships.RemoveWorkoutAsync(id, workoutId))
            throw new NotFoundException("Таке тренування не входить в цей абонемент.");

        return NoContent();
    }

    private static NotFoundException MembershipNotFound(int id) =>
        new($"Абонемент з id={id} не знайдено.");

    private static ConflictException DuplicateName(string name) =>
        new($"Абонемент з назвою \"{name.Trim()}\" вже існує.");
}
