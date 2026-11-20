using System.ComponentModel.DataAnnotations;
namespace FitnessClub.Api.DTOs.Auth;
public class RegisterDto
{
    [Required(ErrorMessage = "Email обов'язковий.")]
    [EmailAddress(ErrorMessage = "Некоректний формат email.")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Пароль обов'язковий.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль має містити від 6 до 100 символів.")]
    public string Password { get; set; } = string.Empty;
    [Required(ErrorMessage = "Ім'я обов'язкове.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Ім'я має містити від 2 до 50 символів.")]
    public string FirstName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Прізвище обов'язкове.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Прізвище має містити від 2 до 50 символів.")]
    public string LastName { get; set; } = string.Empty;
}
public class LoginDto
{
    [Required(ErrorMessage = "Email обов'язковий.")]
    [EmailAddress(ErrorMessage = "Некоректний формат email.")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Пароль обов'язковий.")]
    public string Password { get; set; } = string.Empty;
}
public record UserProfileDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles);
public record AuthResponseDto(
    string Token,
    string TokenType,
    DateTime ExpiresAt,
    UserProfileDto User);
public class ChangeRoleDto
{
    [Required(ErrorMessage = "Роль обов'язкова.")]
    [RegularExpression("^(User|Administrator)$", ErrorMessage = "Роль може бути лише User або Administrator.")]
    public string Role { get; set; } = string.Empty;
}
