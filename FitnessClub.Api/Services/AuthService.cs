using FitnessClub.Api.Auth;
using FitnessClub.Api.Data;
using FitnessClub.Api.DTOs.Auth;
using FitnessClub.Api.Exceptions;
using FitnessClub.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace FitnessClub.Api.Services;
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<UserProfileDto> GetProfileAsync(string userId);
    Task<IReadOnlyList<UserProfileDto>> GetAllUsersAsync();
    Task<UserProfileDto> ChangeRoleAsync(string userId, string role);
}
public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _users;
    private readonly ITokenService _tokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    public AuthService(
        UserManager<AppUser> users,
        ITokenService tokens,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _users = users;
        _tokens = tokens;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim();
        if (await _users.FindByEmailAsync(email) is not null)
            throw new ConflictException("Користувач з таким email уже зареєстрований.");
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var created = await _users.CreateAsync(user, dto.Password);
            if (!created.Succeeded)
                throw new BadRequestException(JoinErrors(created));
            var roleAdded = await _users.AddToRoleAsync(user, Roles.User);
            if (!roleAdded.Succeeded)
                throw new InvalidOperationException($"Не вдалося призначити роль: {JoinErrors(roleAdded)}");
            return true;
        });
        return await BuildAuthResponseAsync(user);
    }
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _users.FindByEmailAsync(dto.Email.Trim());
        if (user is null || !await _users.CheckPasswordAsync(user, dto.Password))
            throw new UnauthorizedException("Невірний email або пароль.");
        return await BuildAuthResponseAsync(user);
    }
    public async Task<UserProfileDto> GetProfileAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId)
            ?? throw new NotFoundException("Користувача не знайдено.");
        return await ToProfileAsync(user);
    }
    public async Task<IReadOnlyList<UserProfileDto>> GetAllUsersAsync()
    {
        var users = await _users.Users
            .OrderBy(u => u.Email)
            .ToListAsync();
        var result = new List<UserProfileDto>();
        foreach (var user in users)
            result.Add(await ToProfileAsync(user));
        return result;
    }
    public async Task<UserProfileDto> ChangeRoleAsync(string userId, string role)
    {
        if (!Roles.All.Contains(role))
            throw new BadRequestException("Роль може бути лише User або Administrator.");
        var user = await _users.FindByIdAsync(userId)
            ?? throw new NotFoundException("Користувача не знайдено.");
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            if (role == Roles.Administrator && !await _users.IsInRoleAsync(user, Roles.Administrator))
                await _users.AddToRoleAsync(user, Roles.Administrator);
            if (role == Roles.User && await _users.IsInRoleAsync(user, Roles.Administrator))
                await _users.RemoveFromRoleAsync(user, Roles.Administrator);
            if (!await _users.IsInRoleAsync(user, Roles.User))
                await _users.AddToRoleAsync(user, Roles.User);
            return true;
        });
        return await ToProfileAsync(user);
    }
    private async Task<AuthResponseDto> BuildAuthResponseAsync(AppUser user)
    {
        var profile = await ToProfileAsync(user);
        var (token, expiresAt) = _tokens.CreateToken(user, profile.Roles);
        return new AuthResponseDto(token, "Bearer", expiresAt, profile);
    }
    private async Task<UserProfileDto> ToProfileAsync(AppUser user)
    {
        var roles = await _users.GetRolesAsync(user);
        return new UserProfileDto(user.Id, user.Email ?? string.Empty, user.FirstName, user.LastName,
            roles.OrderBy(r => r).ToList());
    }
    private static string JoinErrors(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(e => e.Description));
}
