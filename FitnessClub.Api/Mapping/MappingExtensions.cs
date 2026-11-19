using System.Globalization;
using FitnessClub.Api.DTOs.Members;
using FitnessClub.Api.DTOs.Memberships;
using FitnessClub.Api.DTOs.Workouts;
using FitnessClub.Api.DTOs.Trainers;
using FitnessClub.Api.Models;

namespace FitnessClub.Api.Mapping;

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

    // ---------- Members ----------

    public static MemberDto ToDto(this Member m) =>
        new(
            m.Id,
            m.FirstName,
            m.LastName,
            m.Email,
            m.Phone,
            m.BirthDate,
            m.IsActive,
            m.MembershipId);

    public static Member ToModel(this CreateMemberDto dto) => new()
    {
        FirstName = dto.FirstName.Trim(),
        LastName = dto.LastName.Trim(),
        Email = dto.Email.Trim(),
        Phone = dto.Phone.Trim(),
        BirthDate = dto.BirthDate!.Value,
        IsActive = dto.IsActive,
        MembershipId = dto.MembershipId
    };

    // ---------- Trainers ----------

    public static TrainerDto ToDto(this Trainer t) =>
        new(
            t.Id,
            t.FirstName,
            t.LastName,
            t.Email,
            t.Phone,
            t.Specialization,
            t.Experience);

    public static Trainer ToModel(this CreateTrainerDto dto) => new()
    {
        FirstName = dto.FirstName.Trim(),
        LastName = dto.LastName.Trim(),
        Email = dto.Email.Trim(),
        Phone = dto.Phone.Trim(),
        Specialization = dto.Specialization.Trim(),
        Experience = dto.Experience
    };
}
