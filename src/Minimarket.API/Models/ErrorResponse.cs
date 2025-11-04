namespace Minimarket.API.Models;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public string TraceId { get; set; } = System.Diagnostics.Activity.Current?.Id ?? "";
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

