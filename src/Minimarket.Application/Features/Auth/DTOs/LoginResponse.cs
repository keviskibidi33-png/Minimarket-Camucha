namespace Minimarket.Application.Features.Auth.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public bool ProfileCompleted { get; set; } = false;
}

