using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Minimarket.API.Models;
using Minimarket.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Minimarket.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. TraceId: {TraceId}", 
                Activity.Current?.Id ?? context.TraceIdentifier);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Obtener correlation ID del contexto (si existe)
        var correlationId = context.Items["CorrelationId"]?.ToString() 
            ?? Activity.Current?.Id 
            ?? context.TraceIdentifier;

        var response = exception switch
        {
            ValidationException validationEx => new ErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Validation failed",
                Errors = validationEx.Errors.SelectMany(e => e.Value).ToList(),
                TraceId = Activity.Current?.Id ?? context.TraceIdentifier,
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow
            },
            NotFoundException notFoundEx => new ErrorResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = notFoundEx.Message,
                TraceId = Activity.Current?.Id ?? context.TraceIdentifier,
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow
            },
            BusinessRuleViolationException businessEx => new ErrorResponse
            {
                StatusCode = StatusCodes.Status422UnprocessableEntity,
                Message = businessEx.Message,
                TraceId = Activity.Current?.Id ?? context.TraceIdentifier,
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow
            },
            UnauthorizedException => new ErrorResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized access",
                TraceId = Activity.Current?.Id ?? context.TraceIdentifier,
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow
            },
            _ => new ErrorResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = _environment.IsDevelopment() 
                    ? exception.Message 
                    : "An internal server error occurred. Please try again later.",
                TraceId = Activity.Current?.Id ?? context.TraceIdentifier,
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow
            }
        };

        // Logging con correlation ID
        _logger.LogError(exception, 
            "Exception handled. CorrelationId: {CorrelationId}, TraceId: {TraceId}, StatusCode: {StatusCode}", 
            correlationId, response.TraceId, response.StatusCode);

        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsJsonAsync(response);
    }
}

