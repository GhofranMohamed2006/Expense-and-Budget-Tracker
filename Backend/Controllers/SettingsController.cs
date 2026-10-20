using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pennywise.Api.Data;
using Pennywise.Api.Services;

namespace Pennywise.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/settings")]
public class SettingsController(AppDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("profile")]
    public async Task<ActionResult<ProfileDto>> GetProfile()
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == UserId);
        return user is null ? NotFound() : Ok(user.ToProfile());
    }

    [HttpPut("profile")]
    public async Task<ActionResult<ProfileDto>> UpdateProfile(UpdateProfileRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == UserId);
        if (user is null) return NotFound();

        var email = req.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email && u.Id != user.Id))
            return Problem(title: "This email is already used by another account.", statusCode: 409);

        user.FullName = req.FullName.Trim();
        user.Email = email;
        user.Phone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim();
        user.Country = req.Country.Trim();
        await db.SaveChangesAsync();
        return Ok(user.ToProfile());
    }

    [HttpPost("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == UserId);
        if (user is null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
            return Problem(title: "Current password is incorrect.", statusCode: 400);
        if (req.NewPassword != req.ConfirmPassword)
            return Problem(title: "Passwords do not match.", statusCode: 400);
        if (req.NewPassword == req.CurrentPassword)
            return Problem(title: "New password must differ from the current one.", statusCode: 400);
        if (!PasswordRules.IsStrong(req.NewPassword))
            return Problem(title: PasswordRules.Message, statusCode: 400);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<PreferencesDto>> GetPreferences()
    {
        var p = await db.Preferences.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == UserId);
        if (p is null) return NotFound();
        return Ok(new PreferencesDto
        {
            Currency = p.Currency, Language = p.Language, Theme = p.Theme, DateFormat = p.DateFormat,
            EmailNotifications = p.EmailNotifications, BudgetAlerts = p.BudgetAlerts, MonthlySummary = p.MonthlySummary
        });
    }

    [HttpPut("preferences")]
    public async Task<ActionResult<PreferencesDto>> UpdatePreferences(PreferencesDto dto)
    {
        var p = await db.Preferences.FirstOrDefaultAsync(x => x.UserId == UserId);
        if (p is null) return NotFound();
        p.Currency = dto.Currency; p.Language = dto.Language; p.Theme = dto.Theme; p.DateFormat = dto.DateFormat;
        p.EmailNotifications = dto.EmailNotifications; p.BudgetAlerts = dto.BudgetAlerts; p.MonthlySummary = dto.MonthlySummary;
        await db.SaveChangesAsync();
        return Ok(dto);
    }
}
