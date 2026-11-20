using System.ComponentModel.DataAnnotations;
namespace FitnessClub.Api.Auth;
public class JwtSettings
{
    public const string SectionName = "Jwt";
    [Required]
    public string Issuer { get; set; } = string.Empty;
    [Required]
    public string Audience { get; set; } = string.Empty;
    [Required, MinLength(32)]
    public string Key { get; set; } = string.Empty;
    [Range(1, 1440)]
    public int ExpiresMinutes { get; set; } = 60;
}
public class AdminUserSettings
{
    public const string SectionName = "AdminUser";
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = "Адміністратор";
    public string LastName { get; set; } = string.Empty;
}
