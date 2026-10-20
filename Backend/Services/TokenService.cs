using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Pennywise.Api.Models;

namespace Pennywise.Api.Services;

public class TokenService(IConfiguration cfg)
{
    public string Create(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!));
        var token = new JwtSecurityToken(
            issuer: cfg["Jwt:Issuer"],
            audience: cfg["Jwt:Audience"],
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            },
            expires: DateTime.UtcNow.AddMinutes(cfg.GetValue("Jwt:ExpiresMinutes", 120)),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public static class PasswordRules
{
    public const string Message = "Password must be at least 8 characters and include lowercase, uppercase, a number, and a symbol.";

    public static bool IsStrong(string p)
    {
        if (p.Length < 8) return false;
        return p.Any(char.IsLower)
            && p.Any(char.IsUpper)
            && p.Any(char.IsDigit)
            && p.Any(c => !char.IsLetterOrDigit(c));
    }
}

public static class DefaultCategories
{
    public static IEnumerable<Category> For(int userId)
    {
        Category C(string n, CategoryType t, string i, int c) => new() { UserId = userId, Name = n, Type = t, Icon = i, ColorIndex = c };
        yield return C("Food", CategoryType.Expense, "utensils", 0);
        yield return C("Transportation", CategoryType.Expense, "car", 1);
        yield return C("Shopping", CategoryType.Expense, "shopping-bag", 3);
        yield return C("Bills", CategoryType.Expense, "receipt", 2);
        yield return C("Healthcare", CategoryType.Expense, "heart-pulse", 4);
        yield return C("Education", CategoryType.Expense, "graduation-cap", 5);
        yield return C("Entertainment", CategoryType.Expense, "clapperboard", 3);
        yield return C("Other", CategoryType.Expense, "more-horizontal", 6);
        yield return C("Salary", CategoryType.Income, "briefcase", 0);
        yield return C("Freelancing", CategoryType.Income, "laptop", 1);
        yield return C("Gifts", CategoryType.Income, "gift", 3);
        yield return C("Other Income", CategoryType.Income, "more-horizontal", 6);
    }
}
