using System.ComponentModel.DataAnnotations;
using Pennywise.Api.Models;

namespace Pennywise.Api;

public class RegisterRequest
{
    [Required, StringLength(80, MinimumLength = 2)] public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(120)] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

public record AuthResponse(string Token, ProfileDto Profile);
public record ProfileDto(string FullName, string Email, string? Phone, string? Country, DateTime CreatedAt, int Completion);

public class UpdateProfileRequest
{
    [Required, StringLength(80, MinimumLength = 2)] public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(120)] public string Email { get; set; } = "";
    [RegularExpression(@"^([+\d][\d\s()-]{6,})?$", ErrorMessage = "Enter a valid phone number.")] public string? Phone { get; set; }
    [Required, StringLength(80)] public string Country { get; set; } = "";
}

public class ChangePasswordRequest
{
    [Required] public string CurrentPassword { get; set; } = "";
    [Required] public string NewPassword { get; set; } = "";
    [Required] public string ConfirmPassword { get; set; } = "";
}

public class PreferencesDto
{
    [RegularExpression("^(EGP|USD|EUR|GBP)$")] public string Currency { get; set; } = "EGP";
    [RegularExpression("^(English|Arabic)$")] public string Language { get; set; } = "English";
    [RegularExpression("^(Light|Dark)$")] public string Theme { get; set; } = "Light";
    [RegularExpression("^(DD/MM/YYYY|MM/DD/YYYY|YYYY-MM-DD)$")] public string DateFormat { get; set; } = "DD/MM/YYYY";
    public bool EmailNotifications { get; set; }
    public bool BudgetAlerts { get; set; }
    public bool MonthlySummary { get; set; }
}

public class CategoryRequest
{
    [Required, StringLength(30, MinimumLength = 1)] public string Name { get; set; } = "";
    [Required, RegularExpression("(?i)^(expense|income)$")] public string Type { get; set; } = "expense";
    [Required, RegularExpression("^[a-z0-9-]{1,40}$")] public string Icon { get; set; } = "tags";
    [Range(0, 6)] public int ColorIndex { get; set; }
}

public record CategoryDto(int Id, string Name, string Type, string Icon, int ColorIndex, int TransactionCount);

public static class Mapping
{
    public static ProfileDto ToProfile(this User u)
    {
        var filled = new[] { u.FullName, u.Email, u.Phone, u.Country }.Count(s => !string.IsNullOrWhiteSpace(s));
        return new ProfileDto(u.FullName, u.Email, u.Phone, u.Country, u.CreatedAt, filled * 100 / 4);
    }
}
