using System.Globalization;
using FitnessClub.Api.DTOs.Memberships;
using FitnessClub.Api.DTOs.Workouts;
using FitnessClub.Api.Models;

namespace FitnessClub.Api.Mapping;

/// <summary>
/// Перетворення між моделями та DTO. Клієнт ніколи не працює з моделями напряму.
/// </summary>
public static class MappingExtensions
{
    // ---------- Memberships ----------

    public static MembershipDto ToDto(this Membership m) =>
        new(m.Id, m.Name, m.Description, m.Price, m.DurationDays, m.IsActive);

    public static Membership ToModel(this CreateMembershipDto dto) => new()
    {
        Name = dto.Name.Trim(),
        Description = dto.Description?.Trim(),
        Price = dto.Price,
        DurationDays = dto.DurationDays,
        IsActive = dto.IsActive
    };

    // ---------- Workouts ----------

    public static WorkoutDto ToDto(this Workout w) =>
        new(w.Id, w.Name, w.Description, w.TrainerId, w.DayOfWeek,
            w.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture),
            w.DurationMinutes, w.Level, w.MaxParticipants);

    public static Workout ToModel(this CreateWorkoutDto dto) => new()
    {
        Name = dto.Name.Trim(),
        Description = dto.Description?.Trim(),
        TrainerId = dto.TrainerId,
        DayOfWeek = dto.DayOfWeek!.Value,
        StartTime = TimeOnly.ParseExact(dto.StartTime, "HH:mm", CultureInfo.InvariantCulture),
        DurationMinutes = dto.DurationMinutes,
        Level = dto.Level!.Value,
        MaxParticipants = dto.MaxParticipants
    };
}
