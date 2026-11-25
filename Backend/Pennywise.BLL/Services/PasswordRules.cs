namespace Pennywise.BLL.Services;

public static class PasswordRules
{
    public const string Message = "Password must be at least 8 characters and include at least 3 of: lowercase, uppercase, number, symbol.";

    public static bool IsStrong(string p)
    {
        if (p.Length < 8) return false;
        var kinds = new[] { p.Any(char.IsLower), p.Any(char.IsUpper), p.Any(char.IsDigit), p.Any(c => !char.IsLetterOrDigit(c)) };
        return kinds.Count(k => k) >= 3;
    }
}
