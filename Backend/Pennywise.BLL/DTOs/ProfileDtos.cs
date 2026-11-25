using System.ComponentModel.DataAnnotations;

namespace Pennywise.BLL.DTOs;

public record ProfileDto(string FullName, string Email, DateTime CreatedAt);

public class UpdateProfileDto
{
    [Required, StringLength(100, MinimumLength = 2)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    [Required] public string CurrentPassword { get; set; } = string.Empty;
    [Required] public string NewPassword { get; set; } = string.Empty;
    [Required] public string ConfirmPassword { get; set; } = string.Empty;
}
