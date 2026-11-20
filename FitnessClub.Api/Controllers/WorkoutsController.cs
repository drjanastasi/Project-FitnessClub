using FitnessClub.Api.DTOs.Workouts;
using FitnessClub.Api.Exceptions;
using FitnessClub.Api.Mapping;
using FitnessClub.Api.Models;
using FitnessClub.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
namespace FitnessClub.Api.Controllers;

[ApiController]
[Route("api/workouts")]
[Produces("application/json")]
public class WorkoutsController : ControllerBase
{
    private const int MinutesInDay = 24 * 60;
    private readonly IWorkoutRepository _workouts;
    private readonly ITrainerRepository _trainers;
    public WorkoutsController(IWorkoutRepository workouts, ITrainerRepository trainers)
    {
        _workouts = workouts;
        _trainers = trainers;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkoutDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<WorkoutDto>>> GetAll([FromQuery] WorkoutQueryParameters query)
    {
        var workouts = await _workouts.GetAllAsync(query);
        return Ok(workouts.Select(w => w.ToDto()));
    }

    [HttpGet("schedule")]
    [ProducesResponseType(typeof(IEnumerable<ScheduleDayDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ScheduleDayDto>>> GetSchedule([FromQuery] ScheduleQueryParameters query)
    {
        var workouts = await _workouts.GetAllAsync(new WorkoutQueryParameters
        {
            Level = query.Level,
            TrainerId = query.TrainerId,
            MembershipId = query.MembershipId
        });

        var trainerNames = _trainers.GetAll()
            .ToDictionary(t => t.Id, t => $"{t.FirstName} {t.LastName}");
        var week = Enumerable.Range(1, 7).Select(i => (DayOfWeek)(i % 7));
        var schedule = week
            .Select(day =>
            {
                var items = workouts
                    .Where(w => w.DayOfWeek == day)
                    .OrderBy(w => w.StartTime)
                    .Select(w => w.ToScheduleItemDto(trainerNames.GetValueOrDefault(w.TrainerId)))
                    .ToList();
                return new ScheduleDayDto(day, items.Count, items);
            })
            .Where(d => query.IncludeEmptyDays || d.WorkoutsCount > 0)
            .ToList();

        return Ok(schedule);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WorkoutDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkoutDetailsDto>> GetById(int id)
    {
        var workout = await _workouts.GetWithMembershipsAsync(id)
            ?? throw WorkoutNotFound(id);

        return Ok(workout.ToDetailsDto());
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(WorkoutDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkoutDto>> Create([FromBody] CreateWorkoutDto dto)
    {
        var workout = dto.ToEntity();
        await EnsureScheduleIsValidAsync(workout);
        await _workouts.AddAsync(workout);
        return CreatedAtAction(nameof(GetById), new { id = workout.Id }, workout.ToDto());
    }

    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(WorkoutDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkoutDto>> Update(int id, [FromBody] UpdateWorkoutDto dto)
    {
        var workout = await _workouts.GetByIdAsync(id)
            ?? throw WorkoutNotFound(id);
        var candidate = dto.ToEntity();
        candidate.Id = id;
        await EnsureScheduleIsValidAsync(candidate);

        dto.ApplyTo(workout);
        await _workouts.SaveChangesAsync();

        return Ok(workout.ToDto());
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var workout = await _workouts.GetByIdAsync(id)
            ?? throw WorkoutNotFound(id);

        await _workouts.DeleteAsync(workout);
        return NoContent();
    }

    private async Task EnsureScheduleIsValidAsync(Workout workout)
    {
        if (_trainers.GetById(workout.TrainerId) is null)
            throw new BadRequestException($"Тренера з id={workout.TrainerId} не існує.");

        var start = workout.StartTime.Hour * 60 + workout.StartTime.Minute;
        var end = start + workout.DurationMinutes;
        if (end > MinutesInDay)
            throw new BadRequestException("Тренування має закінчитися до 24:00 того ж дня.");

        var sameDay = await _workouts.GetTrainerWorkoutsOnDayAsync(
            workout.TrainerId, workout.DayOfWeek, exceptWorkoutId: workout.Id == 0 ? null : workout.Id);
        var overlapping = sameDay.FirstOrDefault(other =>
        {
            var otherStart = other.StartTime.Hour * 60 + other.StartTime.Minute;
            var otherEnd = otherStart + other.DurationMinutes;
            return start < otherEnd && otherStart < end;
        });

        if (overlapping is not null)
            throw new ConflictException(
                $"Тренер уже веде тренування «{overlapping.Name}» у цей день " +
                $"{overlapping.StartTime:HH\\:mm}–{overlapping.EndTime():HH\\:mm}. Час тренувань не може перетинатися.");
    }
    private static NotFoundException WorkoutNotFound(int id) =>
        new($"Тренування з id={id} не знайдено.");
}
