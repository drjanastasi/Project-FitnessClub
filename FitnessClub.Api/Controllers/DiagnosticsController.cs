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
