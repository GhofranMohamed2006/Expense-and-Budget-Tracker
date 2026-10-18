namespace PennywiseAuth.Models;

/// <summary>
/// Safe user profile information returned upon successful authentication.
/// Excludes all security credentials, salts, and password hashes.
/// </summary>
public class UserProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// Standardized JSON response payload returned by authentication endpoints.
/// </summary>
public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserProfileDto? User { get; set; }
    public string? RedirectUrl { get; set; }
    public List<string>? Errors { get; set; }

    public static AuthResponse Ok(string message, UserProfileDto? user = null, string? redirectUrl = null) =>
        new() { Success = true, Message = message, User = user, RedirectUrl = redirectUrl };

    public static AuthResponse Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}
