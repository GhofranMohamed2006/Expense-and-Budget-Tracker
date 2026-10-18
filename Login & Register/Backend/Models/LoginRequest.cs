namespace PennywiseAuth.Models;

/// <summary>
/// Incoming payload for user login.
/// </summary>
public class LoginRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}
