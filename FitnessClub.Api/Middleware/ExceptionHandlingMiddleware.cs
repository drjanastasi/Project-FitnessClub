using FitnessClub.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FitnessClub.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            
            AppException appException => (appException.StatusCode, appException.Message),
            DbUpdateException => (StatusCodes.Status409Conflict,
                "Не вдалося зберегти зміни: дані конфліктують з уже існуючими записами."),
            BadHttpRequestException badRequest => (badRequest.StatusCode, "Некоректний запит."),
            _ => (StatusCodes.Status500InternalServerError,
                "Сталася внутрішня помилка сервера. Спробуйте пізніше.")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception,
                "Необроблений виняток під час {Method} {Path}. TraceId: {TraceId}",
                context.Request.Method, context.Request.Path, context.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(
                "Помилка запиту {Method} {Path}: {StatusCode} {Message}",
                context.Request.Method, context.Request.Path, statusCode, exception.Message);
        }
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;
        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }
}
