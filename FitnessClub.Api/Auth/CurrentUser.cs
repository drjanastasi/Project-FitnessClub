using System.Security.Claims;

namespace FitnessClub.Api.Auth
{
    /// <summary>Поточний користувач запиту (дані беруться з JWT).</summary>
    public interface ICurrentUser
    {
        string? Email { get; }
        bool IsAdministrator { get; }
    }

    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _accessor;

        public CurrentUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

        public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);

        public bool IsAdministrator => Principal?.IsInRole(Roles.Administrator) ?? false;
    }
}
