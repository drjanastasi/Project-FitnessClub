using System.ComponentModel.DataAnnotations;
using FitnessClub.Api.DTOs.Workouts;

namespace FitnessClub.Api.DTOs.Memberships;

/// <summary>Абонемент у списку.</summary>
public record MembershipDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int DurationDays,
    bool IsActive,
    int WorkoutsCount);

/// <summary>Один абонемент разом із тренуваннями, що в нього входять (пов'язані дані).</summary>
public record MembershipDetailsDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int DurationDays,
    bool IsActive,
    IReadOnlyList<WorkoutShortDto> Workouts);

/// <summary>Тіло запиту POST /api/memberships.</summary>
public class CreateMembershipDto
{
    [Required(ErrorMessage = "Назва абонемента обов'язкова.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Назва має містити від 2 до 100 символів.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Опис не може перевищувати 500 символів.")]
    public string? Description { get; set; }

    [Range(typeof(decimal), "1", "100000", ErrorMessage = "Ціна має бути від 1 до 100000 грн.")]
    public decimal Price { get; set; }

    [Range(1, 365, ErrorMessage = "Тривалість має бути від 1 до 365 днів.")]
    public int DurationDays { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>Тіло запиту PUT /api/memberships/{id} — повне оновлення.</summary>
public class UpdateMembershipDto : CreateMembershipDto
{
}

/// <summary>Тіло запиту PATCH /api/memberships/{id}/status — часткове оновлення.</summary>
public class UpdateMembershipStatusDto
{
    [Required(ErrorMessage = "Поле isActive обов'язкове.")]
    public bool? IsActive { get; set; }
}

/// <summary>
/// Query-параметри для GET /api/memberships: фільтрація, пошук, сортування.
/// </summary>
public class MembershipQueryParameters : IValidatableObject
{
    /// <summary>Лише активні (true) або неактивні (false).</summary>
    public bool? IsActive { get; set; }

    /// <summary>Мінімальна ціна.</summary>
    [Range(0, 100000, ErrorMessage = "minPrice має бути від 0 до 100000.")]
    public decimal? MinPrice { get; set; }

    /// <summary>Максимальна ціна.</summary>
    [Range(0, 100000, ErrorMessage = "maxPrice має бути від 0 до 100000.")]
    public decimal? MaxPrice { get; set; }

    /// <summary>Пошук за назвою або описом.</summary>
    [StringLength(100)]
    public string? Search { get; set; }

    /// <summary>Абонементи, що включають це тренування.</summary>
    public int? WorkoutId { get; set; }

    /// <summary>Поле сортування: name, price, duration.</summary>
    [RegularExpression("^(?i)(name|price|duration)$", ErrorMessage = "sortBy може бути лише: name, price, duration.")]
    public string? SortBy { get; set; }

    /// <summary>Сортувати за спаданням.</summary>
    public bool Desc { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinPrice > MaxPrice)
            yield return new ValidationResult(
                "minPrice не може бути більшим за maxPrice.",
                new[] { nameof(MinPrice) });
    }
}
