namespace FitnessClub.Api.Models;

/// <summary>
/// Абонемент фітнес-клубу (наприклад, "Безлімітний на місяць").
/// Таблиця: Memberships.
/// </summary>
public class Membership
{
    /// <summary>Первинний ключ.</summary>
    public int Id { get; set; }

    /// <summary>Обов'язкове, до 100 символів, унікальне.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Необов'язкове, до 500 символів.</summary>
    public string? Description { get; set; }

    /// <summary>Ціна в гривнях.</summary>
    public decimal Price { get; set; }

    /// <summary>Тривалість дії абонемента в днях.</summary>
    public int DurationDays { get; set; }

    /// <summary>Чи можна зараз придбати цей абонемент.</summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Групові тренування, які входять в абонемент (зв'язок багато-до-багатьох
    /// через проміжну таблицю MembershipWorkouts).
    /// </summary>
    public ICollection<MembershipWorkout> MembershipWorkouts { get; set; } = new List<MembershipWorkout>();
}
