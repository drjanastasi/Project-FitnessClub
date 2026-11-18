namespace FitnessClub.Api.Models;

/// <summary>
/// Групове тренування. Містить і інформацію про розклад
/// (день тижня + час початку), тому окремого ресурсу "Розклад" немає.
/// Приклад: Йога — понеділок 18:00 — 60 хв — Beginner — до 15 людей.
/// Таблиця: Workouts.
/// </summary>
public class Workout
{
    /// <summary>Первинний ключ.</summary>
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Ідентифікатор тренера. Коли в проєкті з'явиться сутність Trainer,
    /// це поле стане зовнішнім ключем (див. WorkoutConfiguration).
    /// </summary>
    public int TrainerId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public int DurationMinutes { get; set; }

    public DifficultyLevel Level { get; set; }

    public int MaxParticipants { get; set; }

    /// <summary>Абонементи, до яких входить це тренування.</summary>
    public ICollection<MembershipWorkout> MembershipWorkouts { get; set; } = new List<MembershipWorkout>();
}

public enum DifficultyLevel
{
    Beginner,
    Intermediate,
    Advanced
}
