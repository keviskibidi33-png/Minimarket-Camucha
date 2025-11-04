using Serilog.Context;

namespace Minimarket.API.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Obtener o generar correlation ID
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault() 
            ?? Guid.NewGuid().ToString();

        // Agregar correlation ID al contexto
        context.Items["CorrelationId"] = correlationId;

        // Agregar correlation ID a los headers de respuesta
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        // Enriquecer logs con correlation ID usando Serilog LogContext
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogDebug("Request started with CorrelationId: {CorrelationId}", correlationId);
            
            await _next(context);
            
            _logger.LogDebug("Request completed with CorrelationId: {CorrelationId}, StatusCode: {StatusCode}", 
                correlationId, context.Response.StatusCode);
        }
    }
}

