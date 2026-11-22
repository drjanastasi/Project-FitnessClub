using FitnessClub.Api.Exceptions;
using FitnessClub.Api.Mapping;
using FitnessClub.Api.Models;
using FitnessClub.Api.Repositories;
namespace FitnessClub.Api.Services;
public interface IWorkoutScheduleValidator
{
    Task ValidateAsync(Workout workout);
}
public class WorkoutScheduleValidator : IWorkoutScheduleValidator
{
    private const int MinutesInDay = 24 * 60;
    private readonly IWorkoutRepository _workouts;
    private readonly ITrainerRepository _trainers;
    private readonly IClubRepository _clubs;
    public WorkoutScheduleValidator(IWorkoutRepository workouts, ITrainerRepository trainers, IClubRepository clubs)
    {
        _workouts = workouts;
        _trainers = trainers;
        _clubs = clubs;
    }
    public async Task ValidateAsync(Workout workout)
    {
        if (_trainers.GetById(workout.TrainerId) is null)
            throw new BadRequestException($"Тренера з id={workout.TrainerId} не існує.");
        if (!await _clubs.ExistsAsync(workout.ClubId))
            throw new BadRequestException($"Клубу з id={workout.ClubId} не існує.");
        var start = ToMinutes(workout.StartTime);
        var end = start + workout.DurationMinutes;
        if (end > MinutesInDay)
            throw new BadRequestException("Тренування має закінчитися до 24:00 того ж дня.");
        var sameDay = await _workouts.GetTrainerWorkoutsOnDayAsync(
            workout.TrainerId, workout.DayOfWeek, exceptWorkoutId: workout.Id == 0 ? null : workout.Id);
        var overlapping = sameDay.FirstOrDefault(other =>
        {
            var otherStart = ToMinutes(other.StartTime);
            var otherEnd = otherStart + other.DurationMinutes;
            return start < otherEnd && otherStart < end;
        });
        if (overlapping is not null)
            throw new ConflictException(
                $"Тренер уже веде тренування «{overlapping.Name}» у цей день " +
                $"{overlapping.StartTime:HH\\:mm}–{overlapping.EndTime():HH\\:mm}. Час тренувань не може перетинатися.");
    }
    private static int ToMinutes(TimeOnly time) => time.Hour * 60 + time.Minute;
}
