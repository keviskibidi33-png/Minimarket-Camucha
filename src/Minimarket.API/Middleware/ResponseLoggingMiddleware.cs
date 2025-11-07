using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Minimarket.API.Middleware;

/// <summary>
/// Middleware para registrar información detallada de las respuestas HTTP
/// </summary>
public class ResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseLoggingMiddleware> _logger;

    public ResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<ResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Interceptar el stream de respuesta
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            // Leer el body de la respuesta
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);

            // Loggear información de la respuesta
            var correlationId = context.Items["CorrelationId"]?.ToString() 
                ?? Activity.Current?.Id 
                ?? context.TraceIdentifier;

            _logger.LogInformation(
                "Response - Path: {Path}, Method: {Method}, StatusCode: {StatusCode}, ContentType: {ContentType}, ContentLength: {ContentLength}, BodyLength: {BodyLength}, CorrelationId: {CorrelationId}",
                context.Request.Path,
                context.Request.Method,
                context.Response.StatusCode,
                context.Response.ContentType,
                context.Response.ContentLength,
                responseBodyText.Length,
                correlationId);

            // Si el body está vacío o es null, loggear advertencia
            if (string.IsNullOrEmpty(responseBodyText))
            {
                _logger.LogWarning(
                    "Response body is empty or null - Path: {Path}, StatusCode: {StatusCode}, ContentType: {ContentType}, CorrelationId: {CorrelationId}",
                    context.Request.Path,
                    context.Response.StatusCode,
                    context.Response.ContentType,
                    correlationId);
            }
            else if (responseBodyText.Length > 0 && responseBodyText.Length < 500)
            {
                // Loggear preview del body si es pequeño
                _logger.LogInformation(
                    "Response body preview: {Preview}, CorrelationId: {CorrelationId}",
                    responseBodyText,
                    correlationId);
            }

            // Copiar el body de vuelta al stream original
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ResponseLoggingMiddleware");
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}

