using FitnessClub.Api.Diagnostics;
using Microsoft.AspNetCore.Mvc;
namespace FitnessClub.Api.Controllers;
[ApiController]
[Route("api/diagnostics")]
[Produces("application/json")]
public class DiagnosticsController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    public DiagnosticsController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }
    [HttpGet("lifetimes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetLifetimes(
        [FromServices] ITransientProbe transient1,
        [FromServices] ITransientProbe transient2,
        [FromServices] IScopedProbe scoped1,
        [FromServices] IScopedProbe scoped2,
        [FromServices] ISingletonProbe singleton1,
        [FromServices] ISingletonProbe singleton2)
    {
        return Ok(new
        {
            transient = new { first = transient1.InstanceId, second = transient2.InstanceId, same = transient1.InstanceId == transient2.InstanceId },
            scoped = new { first = scoped1.InstanceId, second = scoped2.InstanceId, same = scoped1.InstanceId == scoped2.InstanceId },
            singleton = new { first = singleton1.InstanceId, second = singleton2.InstanceId, same = singleton1.InstanceId == singleton2.InstanceId }
        });
    }
    [HttpGet("error")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ThrowError()
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        throw new InvalidOperationException(
            "Тестовий виняток: рядок підключення Data Source=fitnessclub.db, внутрішня інформація, яку не можна показувати клієнту.");
    }
}
