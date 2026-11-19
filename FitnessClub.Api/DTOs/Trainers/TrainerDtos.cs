using System.ComponentModel.DataAnnotations;

namespace FitnessClub.Api.DTOs.Trainers;

/// <summary>
/// Дані тренера, які повертає API.
/// </summary>
public record TrainerDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Specialization,
    int Experience);

/// <summary>
/// Дані для створення тренера.
/// </summary>
public class CreateTrainerDto
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

    [Required(ErrorMessage = "Спеціалізація обов'язкова.")]
    [StringLength(100, MinimumLength = 2)]
    public string Specialization { get; set; } = string.Empty;

    [Range(0, 60, ErrorMessage = "Досвід роботи має бути від 0 до 60 років.")]
    public int Experience { get; set; }
}

/// <summary>
/// Дані для повного оновлення тренера.
/// </summary>
public class UpdateTrainerDto : CreateTrainerDto
{
}
