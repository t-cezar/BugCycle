using BugCycle.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BugCycle.Web.Middleware;


// Traduc exceptiile din stratul de business in coduri HTTP, etc

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetails,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetails = problemDetails;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = Classify(exception);

        LogAppropriately(context, exception, status);

        context.Response.StatusCode = status;

        if (context.Request.Path.StartsWithSegments("/api"))
        {
            return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = status == StatusCodes.Status500InternalServerError
                        ? "A aparut o eroare neasteptata."
                        : exception.Message,
                    Instance = context.Request.Path
                }
            });
        }

        if (context.Response.HasStarted)
        {
            return false;
        }

        var message = status == StatusCodes.Status500InternalServerError
            ? "A aparut o eroare neasteptata."
            : exception.Message;

        context.Response.Redirect(
            $"/Home/Problem?status={status}&message={Uri.EscapeDataString(message)}");

        return true;
    }

    private static (int Status, string Title) Classify(Exception exception) => exception switch
    {
        NotFoundException => (StatusCodes.Status404NotFound, "Resursa nu a fost gasita"),
        ForbiddenException => (StatusCodes.Status403Forbidden, "Acces interzis"),
        DomainException => (StatusCodes.Status400BadRequest, "Cerere invalida"),
        _ => (StatusCodes.Status500InternalServerError, "Eroare interna")
    };


    // Erorile asteptate (404, 403, 400) sunt parte din fluxul normal si se
    // logheaza ca informatie; doar 500 merita nivelul Error cu stack trace.
    private void LogAppropriately(HttpContext context, Exception exception, int status)
    {
        if (status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Eroare netratata la {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            _logger.LogInformation(
                "Cerere respinsa cu {Status} la {Method} {Path}: {Message}",
                status,
                context.Request.Method,
                context.Request.Path,
                exception.Message);
        }
    }
}
