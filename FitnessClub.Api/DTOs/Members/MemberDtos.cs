using System.ComponentModel.DataAnnotations;

namespace FitnessClub.Api.DTOs.Members;

public record MemberDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateOnly BirthDate,
    bool IsActive,
    int MembershipId);

public class CreateMemberDto
{
    [Required(ErrorMessage = "Ім'я обов'язкове.")]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Прізвище обов'язкове.")]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обов'язковий.")]
    [EmailAddress(ErrorMessage = "Вкажіть правильний Email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Телефон обов'язковий.")]
    [Phone(ErrorMessage = "Вкажіть правильний номер телефону.")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Дата народження обов'язкова.")]
    public DateOnly? BirthDate { get; set; }

    public bool IsActive { get; set; } = true;

    [Range(1, int.MaxValue, ErrorMessage = "Потрібно вибрати абонемент.")]
    public int MembershipId { get; set; }
}

public class UpdateMemberDto : CreateMemberDto
{
}
