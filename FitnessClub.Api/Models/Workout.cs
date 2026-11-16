namespace FitnessClub.Api.Models;

/// <summary>
/// Групове тренування. Містить і інформацію про розклад
/// (день тижня + час початку), тому окремого ресурсу "Розклад" немає.
/// Приклад: Йога — понеділок 18:00 — 60 хв — Beginner — до 15 людей.
/// </summary>
public class Workout
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Ідентифікатор тренера. На етапі 2 стане зовнішнім ключем
    /// на ресурс Trainers (частина іншого учасника команди).
    /// </summary>
    public int TrainerId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public int DurationMinutes { get; set; }

    public DifficultyLevel Level { get; set; }

    public int MaxParticipants { get; set; }
}

public enum DifficultyLevel
{
    Beginner,
    Intermediate,
    Advanced
}
