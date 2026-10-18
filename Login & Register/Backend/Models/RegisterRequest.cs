namespace PennywiseAuth.Models;

/// <summary>
/// Incoming payload for user registration.
/// </summary>
public class RegisterRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}
