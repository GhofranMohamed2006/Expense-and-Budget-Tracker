using System.ComponentModel.DataAnnotations;

namespace Pennywise.BLL.DTOs;

public class RegisterDto
{
    [Required, StringLength(100, MinimumLength = 2)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public record UserDto(int Id, string FullName, string Email, DateTime CreatedAt);
public record AuthResultDto(string Token, UserDto User);
