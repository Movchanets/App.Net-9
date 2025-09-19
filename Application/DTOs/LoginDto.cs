namespace Application.DTOs;

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class TokenRequest
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}
