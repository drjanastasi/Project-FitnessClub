using FitnessClub.Api.DTOs.Clubs;
using FitnessClub.Api.DTOs.Members;
using FitnessClub.Api.DTOs.Memberships;
using FitnessClub.Api.DTOs.Trainers;
using FitnessClub.Api.DTOs.Workouts;
using FitnessClub.Api.Models;
using FitnessClub.Api.DTOs.Bookings;
using System.Globalization;

namespace FitnessClub.Api.Mapping;

public static class MappingExtensions
{
    private static string FormatTime(TimeOnly time) =>
        time.ToString("HH:mm", CultureInfo.InvariantCulture);

    // ---------- Memberships ----------

    public static MembershipDto ToDto(this Membership m) =>
        new(m.Id, m.Name, m.Description, m.Price, m.DurationDays, m.IsActive,
            m.MembershipWorkouts.Count);
    public static MembershipDetailsDto ToDetailsDto(this Membership m) =>
        new(m.Id, m.Name, m.Description, m.Price, m.DurationDays, m.IsActive,
            m.MembershipWorkouts
                .Select(mw => mw.Workout)
                .OrderBy(w => ((int)w.DayOfWeek + 6) % 7)
                .ThenBy(w => w.StartTime)
                .Select(w => w.ToShortDto())
                .ToList());

    public static Membership ToEntity(this CreateMembershipDto dto)
    {
        var membership = new Membership();
        dto.ApplyTo(membership);
        return membership;
    }

    public static void ApplyTo(this CreateMembershipDto dto, Membership m)
    {
        m.Name = dto.Name.Trim();
        m.Description = dto.Description?.Trim();
        m.Price = dto.Price;
        m.DurationDays = dto.DurationDays;
        m.IsActive = dto.IsActive;
    }

    public static WorkoutDto ToDto(this Workout w) =>
        new(w.Id, w.Name, w.Description, w.TrainerId, w.DayOfWeek,
            FormatTime(w.StartTime), w.DurationMinutes, w.Level, w.MaxParticipants);
    public static TimeOnly EndTime(this Workout w) =>
        w.StartTime.AddMinutes(w.DurationMinutes);

    public static ScheduleItemDto ToScheduleItemDto(this Workout w, string? trainerName) =>
        new(w.Id, w.Name, FormatTime(w.StartTime), FormatTime(w.EndTime()), w.DurationMinutes,
            w.Level, w.TrainerId, trainerName, w.MaxParticipants);

    public static WorkoutShortDto ToShortDto(this Workout w) =>
        new(w.Id, w.Name, w.DayOfWeek, FormatTime(w.StartTime), w.Level);

    public static WorkoutDetailsDto ToDetailsDto(this Workout w) =>
        new(w.Id, w.Name, w.Description, w.TrainerId, w.DayOfWeek,
            FormatTime(w.StartTime), w.DurationMinutes, w.Level, w.MaxParticipants,
            w.MembershipWorkouts
                .Select(mw => mw.Membership)
                .OrderBy(m => m.Price)
                .Select(m => new WorkoutMembershipDto(m.Id, m.Name, m.Price))
                .ToList());
    public static Workout ToEntity(this CreateWorkoutDto dto)
    {
        var workout = new Workout();
        dto.ApplyTo(workout);
        return workout;
    }

    public static void ApplyTo(this CreateWorkoutDto dto, Workout w)
    {
        w.Name = dto.Name.Trim();
        w.Description = dto.Description?.Trim();
        w.TrainerId = dto.TrainerId;
        w.DayOfWeek = dto.DayOfWeek!.Value;
        w.StartTime = TimeOnly.ParseExact(dto.StartTime, "HH:mm", CultureInfo.InvariantCulture);
        w.DurationMinutes = dto.DurationMinutes;
        w.Level = dto.Level!.Value;
        w.MaxParticipants = dto.MaxParticipants;
    }

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

    // ---------- Clubs ----------

    public static ClubDto ToDto(this Club c) =>
        new(c.Id, c.Name, c.Description, c.Address, c.Phone, c.Email, c.WorkHours);

    public static Club ToEntity(this CreateClubDto dto)
    {
        var club = new Club();
        dto.ApplyTo(club);
        return club;
    }

    public static void ApplyTo(this CreateClubDto dto, Club c)
    {
        c.Name = dto.Name.Trim();
        c.Description = dto.Description?.Trim();
        c.Address = dto.Address.Trim();
        c.Phone = dto.Phone.Trim();
        c.Email = dto.Email.Trim();
        c.WorkHours = dto.WorkHours?.Trim();
    }

    // ---------- Bookings ----------

    /// <summary>
    /// Booking у DTO. Тренування (Workout) має бути завантажене через Include,
    /// інакше назва й розклад будуть недоступні.
    /// </summary>
    /// 

    public static BookingDto ToDto(this Booking b) =>
        new(b.Id, b.MemberId, b.WorkoutId, b.Workout.Name, b.Workout.DayOfWeek,
            b.Workout.StartTime, b.Date, b.Status, b.CreatedAt);

    /// <summary>Новий запис зі статусом Confirmed. Час створення передає сервіс.</summary>
    public static Booking ToEntity(this CreateBookingDto dto, DateTime createdAtUtc) =>
        new()
        {
            MemberId = dto.MemberId,
            WorkoutId = dto.WorkoutId,
            Date = dto.Date!.Value,
            Status = BookingStatus.Confirmed,
            CreatedAt = createdAtUtc
        };
}
