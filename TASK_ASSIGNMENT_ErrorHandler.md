# TASK ASSIGNMENT - Error Handler - Logging and Exception Handling

**Fecha**: [Fecha Actual]  
**Agente**: @Error-Handler  
**Prioridad**: 🟠 ALTA  
**Deadline**: Esta semana (4 días hábiles)

---

## CONTEXTO Y OBJETIVO

Como Error Handler Specialist, eres responsable del sistema de manejo de errores y logging. Actualmente el sistema usa logging básico, pero necesita **logging estructurado con Serilog** y mejoras en el manejo de excepciones.

**Objetivo**: Implementar logging estructurado completo y mejorar el GlobalExceptionMiddleware para proporcionar mejor debugging y experiencia de usuario.

---

## RESPONSABILIDADES DE ERROR HANDLER

### 1. Logging Estructurado
- Configurar Serilog con sinks apropiados
- Implementar logging estructurado en toda la aplicación
- Agregar correlation IDs para tracing

### 2. Exception Handling
- Mejorar GlobalExceptionMiddleware
- Manejar excepciones específicas apropiadamente
- Proporcionar mensajes de error user-friendly

### 3. Monitoring y Observability
- Configurar sinks para logging (File, Console, Seq opcional)
- Agregar métricas y contexto a logs
- Facilitar debugging en producción

---

## TAREAS ASIGNADAS

### TAREA 1: Configurar Serilog (Día 1 - 4 horas)

**PRIORITY**: 🟠 ALTA  
**DELIVERABLE**: Serilog configurado y funcionando

#### Acceptance Criteria:
- [ ] Instalar paquete NuGet Serilog.AspNetCore
- [ ] Instalar sinks: Serilog.Sinks.Console, Serilog.Sinks.File
- [ ] Configurar Serilog en Program.cs
- [ ] Configurar enriquecimiento (Environment, Application, etc.)
- [ ] Configurar formato de logs estructurado (JSON)
- [ ] Configurar niveles de log por ambiente (Development vs Production)
- [ ] Verificar que logs se escriben correctamente
- [ ] Documentar configuración en README

#### Reference Files:
- `src/Minimarket.API/Program.cs`
- `src/Minimarket.API/appsettings.json`
- `src/Minimarket.API/appsettings.Development.json`

#### Implementation Details:
```csharp
// src/Minimarket.API/Program.cs
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/minimarket-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .CreateLogger();

builder.Host.UseSerilog();

// Resto del código...
```

```json
// appsettings.json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/minimarket-.txt",
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithEnvironmentName" ]
  }
}
```

---

### TAREA 2: Implementar Correlation IDs (Día 1-2 - 3 horas)

**PRIORITY**: 🟠 ALTA  
**DELIVERABLE**: Correlation IDs en todos los logs

#### Acceptance Criteria:
- [ ] Crear middleware para generar correlation ID
- [ ] Agregar correlation ID a HttpContext
- [ ] Enriquecer logs con correlation ID automáticamente
- [ ] Incluir correlation ID en respuestas de error
- [ ] Verificar que correlation ID se propaga en toda la request

#### Implementation Details:
```csharp
// src/Minimarket.API/Middleware/CorrelationIdMiddleware.cs
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault() 
                           ?? Guid.NewGuid().ToString();
        
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;
        
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
```

---

### TAREA 3: Mejorar GlobalExceptionHandlerMiddleware (Día 2-3 - 5 horas)

**PRIORITY**: 🟠 ALTA  
**DELIVERABLE**: Middleware mejorado con manejo específico de excepciones

#### Acceptance Criteria:
- [ ] Manejar `ValidationException` (FluentValidation) → 400 Bad Request
- [ ] Manejar `NotFoundException` → 404 Not Found
- [ ] Manejar `BusinessRuleViolationException` → 400 Bad Request
- [ ] Manejar `InsufficientStockException` → 400 Bad Request con mensaje específico
- [ ] Manejar `UnauthorizedException` → 401 Unauthorized
- [ ] Manejar excepciones no esperadas → 500 Internal Server Error
- [ ] Incluir correlation ID en todas las respuestas de error
- [ ] Logging estructurado con nivel apropiado
- [ ] Mensajes user-friendly en producción
- [ ] Detalles completos en desarrollo

#### Reference Files:
- `src/Minimarket.API/Middleware/GlobalExceptionHandlerMiddleware.cs`
- `src/Minimarket.Application/Common/Exceptions/`

#### Implementation Details:
```csharp
// src/Minimarket.API/Middleware/GlobalExceptionHandlerMiddleware.cs
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        IHostEnvironment environment,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _environment = environment;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "Unknown";
        
        context.Response.ContentType = "application/json";
        var response = new ErrorResponse();

        switch (exception)
        {
            case ValidationException validationEx:
                _logger.LogWarning(validationEx, 
                    "Validation error. CorrelationId: {CorrelationId}", correlationId);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Errores de validación",
                    Details = validationEx.Errors.Select(e => e.ErrorMessage).ToList(),
                    CorrelationId = correlationId
                };
                break;

            case NotFoundException notFoundEx:
                _logger.LogWarning(notFoundEx, 
                    "Resource not found. CorrelationId: {CorrelationId}", correlationId);
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response = new ErrorResponse
                {
                    StatusCode = 404,
                    Message = notFoundEx.Message,
                    CorrelationId = correlationId
                };
                break;

            case BusinessRuleViolationException businessEx:
                _logger.LogWarning(businessEx, 
                    "Business rule violation. CorrelationId: {CorrelationId}", correlationId);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new ErrorResponse
                {
                    StatusCode = 400,
                    Message = businessEx.Message,
                    CorrelationId = correlationId
                };
                break;

            case InsufficientStockException stockEx:
                _logger.LogWarning(stockEx, 
                    "Insufficient stock. CorrelationId: {CorrelationId}", correlationId);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new ErrorResponse
                {
                    StatusCode = 400,
                    Message = $"Stock insuficiente para {stockEx.ProductName}. Disponible: {stockEx.Available}, Solicitado: {stockEx.Requested}",
                    CorrelationId = correlationId
                };
                break;

            case UnauthorizedException:
                _logger.LogWarning("Unauthorized access. CorrelationId: {CorrelationId}", correlationId);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                response = new ErrorResponse
                {
                    StatusCode = 401,
                    Message = "No autorizado",
                    CorrelationId = correlationId
                };
                break;

            default:
                _logger.LogError(exception, 
                    "Unexpected error. CorrelationId: {CorrelationId}", correlationId);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response = new ErrorResponse
                {
                    StatusCode = 500,
                    Message = _environment.IsDevelopment() 
                        ? exception.Message 
                        : "Ocurrió un error inesperado",
                    Details = _environment.IsDevelopment() 
                        ? new List<string> { exception.StackTrace ?? string.Empty }
                        : null,
                    CorrelationId = correlationId
                };
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }
}
```

---

### TAREA 4: Mejorar Modelo de Error Response (Día 3 - 2 horas)

**PRIORITY**: 🟡 MEDIA  
**DELIVERABLE**: ErrorResponse mejorado con más contexto

#### Acceptance Criteria:
- [ ] Agregar campo CorrelationId a ErrorResponse
- [ ] Agregar campo Timestamp
- [ ] Agregar campo Path (endpoint que causó el error)
- [ ] Agregar campo Method (HTTP method)
- [ ] Mantener compatibilidad con frontend actual

#### Reference Files:
- `src/Minimarket.API/Models/ErrorResponse.cs`

#### Implementation:
```csharp
// src/Minimarket.API/Models/ErrorResponse.cs
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Details { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Path { get; set; }
    public string? Method { get; set; }
}
```

---

### TAREA 5: Agregar Logging Contextual en Handlers (Día 4 - 3 horas)

**PRIORITY**: 🟡 MEDIA  
**DELIVERABLE**: Logging mejorado en handlers críticos

#### Acceptance Criteria:
- [ ] Agregar logging estructurado en CreateSaleCommandHandler
- [ ] Agregar logging estructurado en CancelSaleCommandHandler
- [ ] Usar LogContext para enriquecer logs
- [ ] Agregar métricas relevantes (tiempo de ejecución opcional)
- [ ] Logs deben incluir correlation ID automáticamente

#### Reference Files:
- `src/Minimarket.Application/Features/Sales/Commands/CreateSaleCommandHandler.cs`
- `src/Minimarket.Application/Features/Sales/Commands/CancelSaleCommandHandler.cs`

#### Example:
```csharp
using Serilog.Context;

public async Task<Result<SaleDto>> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
{
    using (LogContext.PushProperty("UserId", request.UserId))
    using (LogContext.PushProperty("ProductCount", request.Sale.SaleDetails.Count))
    {
        _logger.LogInformation("Creating sale for user {UserId} with {ProductCount} products");
        
        // Resto del código...
        
        _logger.LogInformation("Sale created successfully. SaleId: {SaleId}, DocumentNumber: {DocumentNumber}", 
            sale.Id, sale.DocumentNumber);
    }
}
```

---

## ESTRUCTURA DE ARCHIVOS

```
src/Minimarket.API/
├── Middleware/
│   ├── GlobalExceptionHandlerMiddleware.cs (MEJORAR)
│   ├── CorrelationIdMiddleware.cs (CREAR)
│   └── RequestLoggingMiddleware.cs (OPCIONAL - futuro)
├── Models/
│   └── ErrorResponse.cs (MEJORAR)
└── Program.cs (CONFIGURAR Serilog)
```

---

## ESTÁNDARES DE LOGGING

### Niveles de Log
- **Error**: Excepciones y errores críticos
- **Warning**: Validaciones fallidas, reglas de negocio violadas
- **Information**: Operaciones importantes (crear venta, anular venta)
- **Debug**: Información detallada para debugging (solo en desarrollo)

### Formato de Logs
- **Development**: Formato legible con colores
- **Production**: JSON estructurado para parsing

### Propiedades de Log
- CorrelationId (siempre)
- UserId (cuando aplica)
- EntityId (cuando aplica)
- Action (operación realizada)

---

## MÉTRICAS Y OBJETIVOS

### Coverage de Logging
- **Handlers críticos**: 100% tienen logging
- **Middleware**: 100% tiene logging estructurado
- **Exceptions**: 100% son loggeadas

### Calidad de Logs
- **Estructurados**: JSON en producción
- **Contextuales**: Incluyen correlation ID
- **Accionables**: Facilitan debugging

---

## DEPENDENCIAS Y BLOQUEOS

### Dependencias
- ✅ GlobalExceptionHandlerMiddleware existe
- ✅ Excepciones personalizadas existen
- ⚠️ Necesita instalar paquetes Serilog

### Bloqueos Potenciales
- Si hay problemas con configuración de Serilog
- Si hay conflictos con logging existente

### Acción si Bloqueado
- Reportar inmediatamente a Tech Lead
- Documentar el bloqueo específico

---

## REPORTE DIARIO REQUERIDO

Al final de cada día, reportar:

```
## DAILY PROGRESS - Error Handler - [Fecha]

### Tareas Completadas Hoy:
- [Lista de tareas completadas]

### Logging Configurado:
- ✅ Serilog configurado / ⏳ Pendiente
- ✅ Correlation IDs implementados / ⏳ Pendiente
- ✅ Middleware mejorado / ⏳ Pendiente

### Blockers:
- [Lista de blockers si los hay]

### Plan Mañana:
- [Tareas específicas para mañana]
```

---

## ACCEPTANCE CRITERIA FINAL

El trabajo está **COMPLETO** cuando:

- [ ] ✅ Serilog configurado y funcionando
- [ ] ✅ Logs se escriben en archivo y consola
- [ ] ✅ Correlation IDs implementados y funcionando
- [ ] ✅ GlobalExceptionHandlerMiddleware maneja todas las excepciones
- [ ] ✅ Mensajes de error son user-friendly en producción
- [ ] ✅ Detalles completos disponibles en desarrollo
- [ ] ✅ Logging estructurado en handlers críticos
- [ ] ✅ ErrorResponse mejorado con más contexto
- [ ] ✅ Todos los tests pasan (si hay tests afectados)
- [ ] ✅ PR creado con todos los cambios
- [ ] ✅ Code review aprobado por Tech Lead

---

## RECURSOS Y REFERENCIAS

### Documentación
- [Serilog Documentation](https://serilog.net/)
- [Serilog ASP.NET Core](https://github.com/serilog/serilog-aspnetcore)
- [Structured Logging Best Practices](https://www.elastic.co/guide/en/elasticsearch/guide/current/logging.html)

### Archivos de Referencia
- `src/Minimarket.API/Middleware/GlobalExceptionHandlerMiddleware.cs`
- `src/Minimarket.Application/Common/Exceptions/`
- `TECHNICAL_AUDIT.md` - Sección de Logging

---

## PRIORIZACIÓN DE TAREAS

**Orden de Ejecución Recomendado**:
1. **Día 1**: Tarea 1 (Serilog) → Tarea 2 (Correlation IDs)
2. **Día 2**: Tarea 3 (GlobalExceptionMiddleware - inicio)
3. **Día 3**: Tarea 3 (GlobalExceptionMiddleware - completar) → Tarea 4 (ErrorResponse)
4. **Día 4**: Tarea 5 (Logging Contextual) → Testing y refinamiento

---

## NOTAS FINALES

**@Error-Handler**: 

Esta tarea es **ALTA PRIORIDAD** porque mejora significativamente la capacidad de debugging y monitoreo del sistema. El logging estructurado es esencial para producción.

**ENFÓCATE EN**:
- ✅ Logging estructurado (JSON en producción)
- ✅ Correlation IDs para tracing
- ✅ Mensajes de error user-friendly
- ✅ Configuración por ambiente

**ESTA TAREA ES COMPLEMENTARIA Y NO BLOQUEA OTRAS TAREAS.**

---

**ASIGNADO POR**: Tech Lead  
**FECHA**: [Fecha Actual]  
**DEADLINE**: [Fecha + 4 días hábiles]  
**STATUS**: 🟡 EN PROGRESO

