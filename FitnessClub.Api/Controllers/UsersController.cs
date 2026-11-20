using FitnessClub.Api.Auth;
using FitnessClub.Api.DTOs.Auth;
using FitnessClub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FitnessClub.Api.Controllers;
[ApiController]
[Route("api/users")]
[Produces("application/json")]
[Authorize(Policy = Policies.AdminOnly)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public class UsersController : ControllerBase
{
    private readonly IAuthService _auth;
    public UsersController(IAuthService auth)
    {
        _auth = auth;
    }
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetAll()
    {
        return Ok(await _auth.GetAllUsersAsync());
    }
    [HttpPut("{id}/role")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> ChangeRole(string id, [FromBody] ChangeRoleDto dto)
    {
        return Ok(await _auth.ChangeRoleAsync(id, dto.Role));
    }
}
