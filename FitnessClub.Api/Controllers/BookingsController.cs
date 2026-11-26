using FitnessClub.Api.Auth;
using FitnessClub.Api.DTOs.Bookings;
using FitnessClub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FitnessClub.Api.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    [Produces("application/json")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _service;

        public BookingsController(IBookingService service)
        {
            _service = service;
        }

        /// <summary>Список записів з фільтрацією та сортуванням (потрібна авторизація).</summary>
        [HttpGet]
        [Authorize(Policy = Policies.MemberOrAdmin)]
        [ProducesResponseType(typeof(IEnumerable<BookingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetAll([FromQuery] BookingQueryParameters query)
        {
            return Ok(await _service.GetAllAsync(query));
        }

        /// <summary>Кількість вільних місць на тренування в конкретну дату (доступно всім).</summary>
        [HttpGet("availability")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(WorkoutAvailabilityDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkoutAvailabilityDto>> GetAvailability(
            [FromQuery, BindRequired] int workoutId,
            [FromQuery, BindRequired] DateOnly date)
        {
            return Ok(await _service.GetAvailabilityAsync(workoutId, date));
        }

        /// <summary>Один запис за id (потрібна авторизація).</summary>
        [HttpGet("{id:int}")]
        [Authorize(Policy = Policies.MemberOrAdmin)]
        [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingDto>> GetById(int id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        /// <summary>Записати клієнта на тренування в конкретну дату.</summary>
        [HttpPost]
        [Authorize(Policy = Policies.MemberOrAdmin)]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Скасувати запис (статус стає Cancelled).</summary>
        [HttpPost("{id:int}/cancel")]
        [Authorize(Policy = Policies.MemberOrAdmin)]
        [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookingDto>> Cancel(int id)
        {
            return Ok(await _service.CancelAsync(id));
        }

        /// <summary>Видалити запис остаточно (лише адміністратор).</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
