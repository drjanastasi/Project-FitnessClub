using System.ComponentModel.DataAnnotations;
using FitnessClub.Api.Models;
namespace FitnessClub.Api.DTOs.Workouts;
public record WorkoutDto(
    int Id,
    string Name,
    string? Description,
    int TrainerId,
    DayOfWeek DayOfWeek,
    string StartTime,
    int DurationMinutes,
    DifficultyLevel Level,
    int MaxParticipants);
public record WorkoutShortDto(
    int Id,
    string Name,
    DayOfWeek DayOfWeek,
    string StartTime,
    DifficultyLevel Level);
public record WorkoutMembershipDto(int Id, string Name, decimal Price);
public record WorkoutDetailsDto(
    int Id,
    string Name,
    string? Description,
    int TrainerId,
    DayOfWeek DayOfWeek,
    string StartTime,
    int DurationMinutes,
    DifficultyLevel Level,
    int MaxParticipants,
    IReadOnlyList<WorkoutMembershipDto> Memberships);
public class CreateWorkoutDto
{
    [Required(ErrorMessage = "Назва тренування обов'язкова.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Назва має містити від 2 до 100 символів.")]
    public string Name { get; set; } = string.Empty;
    [StringLength(500, ErrorMessage = "Опис не може перевищувати 500 символів.")]
    public string? Description { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Потрібно вказати коректний trainerId.")]
    public int TrainerId { get; set; }
    [Required(ErrorMessage = "День тижня обов'язковий.")]
    [EnumDataType(typeof(DayOfWeek))]
    public DayOfWeek? DayOfWeek { get; set; }
    [Required(ErrorMessage = "Час початку обов'язковий.")]
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Час має бути у форматі HH:mm, наприклад 18:00.")]
    public string StartTime { get; set; } = string.Empty;
    [Range(15, 240, ErrorMessage = "Тривалість має бути від 15 до 240 хвилин.")]
    public int DurationMinutes { get; set; }
    [Required(ErrorMessage = "Рівень складності обов'язковий.")]
    [EnumDataType(typeof(DifficultyLevel))]
    public DifficultyLevel? Level { get; set; }
    [Range(1, 100, ErrorMessage = "Кількість учасників має бути від 1 до 100.")]
    public int MaxParticipants { get; set; }
}
public class UpdateWorkoutDto : CreateWorkoutDto
{
}
public class WorkoutQueryParameters
{
    public DayOfWeek? Day { get; set; }
    public DifficultyLevel? Level { get; set; }
    [Range(1, int.MaxValue)]
    public int? TrainerId { get; set; }
    [Range(1, int.MaxValue)]
    public int? MembershipId { get; set; }
    [StringLength(100)]
    public string? Search { get; set; }
    [RegularExpression("^(?i)(schedule|name|duration)$", ErrorMessage = "sortBy може бути лише: schedule, name, duration.")]
    public string? SortBy { get; set; }
    public bool Desc { get; set; }
}
public record ScheduleItemDto(
    int Id,
    string Name,
    string StartTime,
    string EndTime,
    int DurationMinutes,
    DifficultyLevel Level,
    int TrainerId,
    string? TrainerName,
    int MaxParticipants);

public record ScheduleDayDto(
    DayOfWeek Day,
    int WorkoutsCount,
    IReadOnlyList<ScheduleItemDto> Workouts);

public class ScheduleQueryParameters
{
    public DifficultyLevel? Level { get; set; }
    [Range(1, int.MaxValue)]
    public int? TrainerId { get; set; }
    [Range(1, int.MaxValue)]
    public int? MembershipId { get; set; }
    public bool IncludeEmptyDays { get; set; } = true;
}
