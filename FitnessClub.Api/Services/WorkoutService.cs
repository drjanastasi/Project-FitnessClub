using FitnessClub.Api.DTOs.Workouts;
using FitnessClub.Api.Exceptions;
using FitnessClub.Api.Mapping;
using FitnessClub.Api.Repositories;
namespace FitnessClub.Api.Services;
public interface IWorkoutService
{
    Task<IReadOnlyList<WorkoutDto>> GetAllAsync(WorkoutQueryParameters query);
    Task<IReadOnlyList<ScheduleDayDto>> GetScheduleAsync(ScheduleQueryParameters query);
    Task<WorkoutDetailsDto> GetByIdAsync(int id);
    Task<WorkoutDto> CreateAsync(CreateWorkoutDto dto);
    Task<WorkoutDto> UpdateAsync(int id, UpdateWorkoutDto dto);
    Task DeleteAsync(int id);
}
public class WorkoutService : IWorkoutService
{
    private readonly IWorkoutRepository _workouts;
    private readonly ITrainerRepository _trainers;
    private readonly IWorkoutScheduleValidator _scheduleValidator;
    public WorkoutService(
        IWorkoutRepository workouts,
        ITrainerRepository trainers,
        IWorkoutScheduleValidator scheduleValidator)
    {
        _workouts = workouts;
        _trainers = trainers;
        _scheduleValidator = scheduleValidator;
    }
    public async Task<IReadOnlyList<WorkoutDto>> GetAllAsync(WorkoutQueryParameters query)
    {
        var workouts = await _workouts.GetAllAsync(query);
        return workouts.Select(w => w.ToDto()).ToList();
    }
    public async Task<IReadOnlyList<ScheduleDayDto>> GetScheduleAsync(ScheduleQueryParameters query)
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
        return week
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
    }
    public async Task<WorkoutDetailsDto> GetByIdAsync(int id)
    {
        var workout = await _workouts.GetWithMembershipsAsync(id)
            ?? throw NotFound(id);

        return workout.ToDetailsDto();
    }
    public async Task<WorkoutDto> CreateAsync(CreateWorkoutDto dto)
    {
        var workout = dto.ToEntity();
        await _scheduleValidator.ValidateAsync(workout);

        await _workouts.AddAsync(workout);
        return workout.ToDto();
    }
    public async Task<WorkoutDto> UpdateAsync(int id, UpdateWorkoutDto dto)
    {
        var workout = await _workouts.GetByIdAsync(id)
            ?? throw NotFound(id);

        var candidate = dto.ToEntity();
        candidate.Id = id;
        await _scheduleValidator.ValidateAsync(candidate);

        dto.ApplyTo(workout);
        await _workouts.SaveChangesAsync();

        return workout.ToDto();
    }
    public async Task DeleteAsync(int id)
    {
        var workout = await _workouts.GetByIdAsync(id)
            ?? throw NotFound(id);

        await _workouts.DeleteAsync(workout);
    }
    private static NotFoundException NotFound(int id) =>
        new($"Тренування з id={id} не знайдено.");
}
