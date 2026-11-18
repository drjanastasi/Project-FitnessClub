namespace FitnessClub.Api.Models;

/// <summary>
/// Проміжна сутність для зв'язку багато-до-багатьох:
/// один абонемент включає багато тренувань,
/// одне тренування доступне в багатьох абонементах.
/// Таблиця: MembershipWorkouts, складений первинний ключ (MembershipId, WorkoutId).
/// </summary>
public class MembershipWorkout
{
    /// <summary>Зовнішній ключ на Memberships.</summary>
    public int MembershipId { get; set; }
    public Membership Membership { get; set; } = null!;

    /// <summary>Зовнішній ключ на Workouts.</summary>
    public int WorkoutId { get; set; }
    public Workout Workout { get; set; } = null!;
}
