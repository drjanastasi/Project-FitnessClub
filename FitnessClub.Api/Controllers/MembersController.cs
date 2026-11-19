
using FitnessClub.Api.DTOs.Members;
using FitnessClub.Api.Mapping;
using FitnessClub.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FitnessClub.Api.Controllers;

/// <summary>
/// Клієнти фітнес-клубу.
/// </summary>
[ApiController]
[Route("api/members")]
[Produces("application/json")]
public class MembersController : ControllerBase
{
    private readonly IMemberRepository _repository;
    private readonly IMembershipRepository _membershipRepository;

    public MembersController(
        IMemberRepository repository,
        IMembershipRepository membershipRepository)
    {
        _repository = repository;
        _membershipRepository = membershipRepository;
    }

    /// <summary>Отримати список клієнтів з фільтрацією за статусом та пошуком.</summary>
    /// <param name="isActive">Фільтр за статусом активності клієнта.</param>
    /// <param name="search">Пошук клієнта за ім'ям, прізвищем або Email.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MemberDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<MemberDto>> GetAll(
        [FromQuery] bool? isActive,
        [FromQuery] string? search)
    {
        var query = _repository.GetAll().AsEnumerable();

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m =>
                m.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                m.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                m.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(query.OrderBy(m => m.Id).Select(m => m.ToDto()));
    }

    /// <summary>Отримати клієнта за id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<MemberDto> GetById(int id)
    {
        var member = _repository.GetById(id);

        if (member is null)
            return Problem(statusCode: 404, title: $"Клієнта з id={id} не знайдено.");

        return Ok(member.ToDto());
    }

    /// <summary>Створити нового клієнта.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<MemberDto> Create([FromBody] CreateMemberDto dto)
    {
        if (_repository.EmailExists(dto.Email))
            return Problem(statusCode: 409, title: "Клієнт з таким Email вже існує.");

        if (_membershipRepository.GetById(dto.MembershipId) is null)
            return Problem(statusCode: 400, title: "Вказаний абонемент не знайдено.");

        var created = _repository.Add(dto.ToModel());

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created.ToDto());
    }

    /// <summary>Повністю оновити дані клієнта.</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<MemberDto> Update(int id, [FromBody] UpdateMemberDto dto)
    {
        if (_repository.GetById(id) is null)
            return Problem(statusCode: 404, title: $"Клієнта з id={id} не знайдено.");

        if (_repository.EmailExists(dto.Email, exceptId: id))
            return Problem(statusCode: 409, title: "Клієнт з таким Email вже існує.");

        if (_membershipRepository.GetById(dto.MembershipId) is null)
            return Problem(statusCode: 400, title: "Вказаний абонемент не знайдено.");

        var updated = dto.ToModel();
        updated.Id = id;

        _repository.Update(updated);

        return Ok(updated.ToDto());
    }

    /// <summary>Видалити клієнта.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        if (!_repository.Delete(id))
            return Problem(statusCode: 404, title: $"Клієнта з id={id} не знайдено.");

        return NoContent();
    }
}
