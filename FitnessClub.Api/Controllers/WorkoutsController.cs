using FitnessClub.Api.Auth;
using FitnessClub.Api.DTOs.Workouts;
using FitnessClub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FitnessClub.Api.Controllers;
[ApiController]
[Route("api/workouts")]
[Produces("application/json")]
public class WorkoutsController : ControllerBase
{
    private readonly IWorkoutService _service;
    public WorkoutsController(IWorkoutService service)
    {
        _service = service;
    }
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<WorkoutDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<WorkoutDto>>> GetAll([FromQuery] WorkoutQueryParameters query)
    {
        return Ok(await _service.GetAllAsync(query));
    }
    [HttpGet("schedule")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ScheduleDayDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ScheduleDayDto>>> GetSchedule([FromQuery] ScheduleQueryParameters query)
    {
        return Ok(await _service.GetScheduleAsync(query));
    }
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(WorkoutDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkoutDetailsDto>> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }
    [HttpPost]
    [Authorize(Policy = Policies.AdminOnly)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(WorkoutDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkoutDto>> Create([FromBody] CreateWorkoutDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.AdminOnly)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(WorkoutDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkoutDto>> Update(int id, [FromBody] UpdateWorkoutDto dto)
    {
        return Ok(await _service.UpdateAsync(id, dto));
    }
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
