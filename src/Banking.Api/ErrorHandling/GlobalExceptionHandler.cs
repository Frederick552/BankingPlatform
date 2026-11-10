using Banking.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.ErrorHandling;

/// <summary>
/// Único lugar donde las excepciones se traducen a respuestas HTTP (RFC 7807 - problem+json).
/// Los controladores no necesitan try/catch.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = Map(exception);

        if (status >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Error no controlado en {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        else
            logger.LogWarning("Error de negocio {ExceptionType} en {Method} {Path}: {Message}",
                exception.GetType().Name, httpContext.Request.Method, httpContext.Request.Path, exception.Message);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(
            problem, options: null, contentType: "application/problem+json", cancellationToken);

        return true;
    }

    // Los mensajes de las excepciones de negocio son seguros para el cliente;
    // para cualquier otra excepción nunca se expone su contenido.
    private static (int Status, string Title, string Detail) Map(Exception exception) => exception switch
    {
        NotFoundException e =>
            (StatusCodes.Status404NotFound, "Recurso no encontrado", e.Message),
        InsufficientFundsException e =>
            (StatusCodes.Status400BadRequest, "Fondos insuficientes", e.Message),
        DomainValidationException e =>
            (StatusCodes.Status400BadRequest, "Solicitud inválida", e.Message),
        ConcurrencyConflictException e =>
            (StatusCodes.Status409Conflict, "Conflicto de concurrencia", e.Message),
        BadHttpRequestException =>
            (StatusCodes.Status400BadRequest, "Solicitud mal formada", "El cuerpo de la solicitud no es válido."),
        _ =>
            (StatusCodes.Status500InternalServerError, "Error interno del servidor",
                "Ocurrió un error inesperado. Si persiste, contacte a soporte indicando el traceId.")
    };
}
