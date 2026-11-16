using System.ComponentModel.DataAnnotations;

namespace FitnessClub.Api.DTOs.Memberships;

/// <summary>Те, що API повертає клієнту.</summary>
public record MembershipDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int DurationDays,
    bool IsActive);

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
