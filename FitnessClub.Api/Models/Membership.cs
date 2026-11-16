namespace FitnessClub.Api.Models;

/// <summary>
/// Абонемент фітнес-клубу (наприклад, "Безлімітний на місяць").
/// </summary>
public class Membership
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Ціна в гривнях.</summary>
    public decimal Price { get; set; }

    /// <summary>Тривалість дії абонемента в днях.</summary>
    public int DurationDays { get; set; }

    /// <summary>Чи можна зараз придбати цей абонемент.</summary>
    public bool IsActive { get; set; }
}
