using FitnessClub.Api.Auth;
using FitnessClub.Api.DTOs.Clubs;
using FitnessClub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessClub.Api.Controllers
{
    [ApiController]
    [Route("api/clubs")]
    [Produces("application/json")]
    public class ClubsController : ControllerBase
    {
        private readonly IClubService _service;

        public ClubsController(IClubService service)
        {
            _service = service;
        }

        /// <summary>Список клубів (філій) з пошуком і сортуванням.</summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ClubDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<ClubDto>>> GetAll([FromQuery] ClubQueryParameters query)
        {
            return Ok(await _service.GetAllAsync(query));
        }

        /// <summary>Один клуб за id.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ClubDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClubDto>> GetById(int id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        /// <summary>Створити клуб (лише адміністратор).</summary>
        [HttpPost]
        [Authorize(Policy = Policies.AdminOnly)]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ClubDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ClubDto>> Create([FromBody] CreateClubDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Повне оновлення клубу (лише адміністратор).</summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ClubDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ClubDto>> Update(int id, [FromBody] UpdateClubDto dto)
        {
            return Ok(await _service.UpdateAsync(id, dto));
        }

        /// <summary>Видалити клуб (лише адміністратор).</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
