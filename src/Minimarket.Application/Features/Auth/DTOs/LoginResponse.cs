namespace Minimarket.Application.Features.Auth.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
}

