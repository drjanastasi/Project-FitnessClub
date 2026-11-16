using FitnessClub.Api.DTOs.Memberships;
using FitnessClub.Api.Mapping;
using FitnessClub.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FitnessClub.Api.Controllers;

/// <summary>
/// Абонементи фітнес-клубу.
/// </summary>
[ApiController]
[Route("api/memberships")]
[Produces("application/json")]
public class MembershipsController : ControllerBase
{
    private readonly IMembershipRepository _repository;

    public MembershipsController(IMembershipRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Отримати список абонементів з фільтрацією та сортуванням.</summary>
    /// <param name="isActive">Лише активні (true) або неактивні (false) абонементи.</param>
    /// <param name="minPrice">Мінімальна ціна.</param>
    /// <param name="maxPrice">Максимальна ціна.</param>
    /// <param name="search">Пошук за назвою або описом.</param>
    /// <param name="sortBy">Поле сортування: name, price, duration.</param>
    /// <param name="desc">Сортувати за спаданням.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MembershipDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<IEnumerable<MembershipDto>> GetAll(
        [FromQuery] bool? isActive,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] bool desc = false)
    {
        if (minPrice > maxPrice)
        {
            ModelState.AddModelError(nameof(minPrice), "minPrice не може бути більшим за maxPrice.");
            return ValidationProblem(ModelState);
        }

        var query = _repository.GetAll().AsEnumerable();

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);
        if (minPrice.HasValue)
            query = query.Where(m => m.Price >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(m => m.Price <= maxPrice.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m =>
                m.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (m.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));

        switch (sortBy?.ToLowerInvariant())
        {
            case null or "":
                query = query.OrderBy(m => m.Id);
                break;
            case "name":
                query = desc ? query.OrderByDescending(m => m.Name) : query.OrderBy(m => m.Name);
                break;
            case "price":
                query = desc ? query.OrderByDescending(m => m.Price) : query.OrderBy(m => m.Price);
                break;
            case "duration":
                query = desc ? query.OrderByDescending(m => m.DurationDays) : query.OrderBy(m => m.DurationDays);
                break;
            default:
                ModelState.AddModelError(nameof(sortBy), "sortBy може бути лише: name, price, duration.");
                return ValidationProblem(ModelState);
        }

        return Ok(query.Select(m => m.ToDto()));
    }

    /// <summary>Отримати абонемент за id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<MembershipDto> GetById(int id)
    {
        var membership = _repository.GetById(id);
        if (membership is null)
            return Problem(statusCode: 404, title: $"Абонемент з id={id} не знайдено.");

        return Ok(membership.ToDto());
    }

    /// <summary>Створити новий абонемент.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<MembershipDto> Create([FromBody] CreateMembershipDto dto)
    {
        // Невалідні DTO автоматично отримують 400 завдяки [ApiController].
        if (_repository.NameExists(dto.Name))
            return Problem(statusCode: 409, title: $"Абонемент з назвою \"{dto.Name}\" вже існує.");

        var created = _repository.Add(dto.ToModel());
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
    }

    /// <summary>Повністю оновити абонемент.</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<MembershipDto> Update(int id, [FromBody] UpdateMembershipDto dto)
    {
        if (_repository.GetById(id) is null)
            return Problem(statusCode: 404, title: $"Абонемент з id={id} не знайдено.");

        if (_repository.NameExists(dto.Name, exceptId: id))
            return Problem(statusCode: 409, title: $"Абонемент з назвою \"{dto.Name}\" вже існує.");

        var updated = dto.ToModel();
        updated.Id = id;
        _repository.Update(updated);

        return Ok(updated.ToDto());
    }

    /// <summary>Активувати або деактивувати абонемент (часткове оновлення).</summary>
    [HttpPatch("{id:int}/status")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<MembershipDto> UpdateStatus(int id, [FromBody] UpdateMembershipStatusDto dto)
    {
        var membership = _repository.GetById(id);
        if (membership is null)
            return Problem(statusCode: 404, title: $"Абонемент з id={id} не знайдено.");

        membership.IsActive = dto.IsActive!.Value;
        _repository.Update(membership);

        return Ok(membership.ToDto());
    }

    /// <summary>Видалити абонемент.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        if (!_repository.Delete(id))
            return Problem(statusCode: 404, title: $"Абонемент з id={id} не знайдено.");

        return NoContent();
    }
}
