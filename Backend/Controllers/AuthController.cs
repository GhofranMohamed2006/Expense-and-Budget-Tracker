using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pennywise.Api.Data;
using Pennywise.Api.Models;
using Pennywise.Api.Services;

namespace Pennywise.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, TokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email))
            return Problem(title: "This email is already registered.", statusCode: 409);
        if (!PasswordRules.IsStrong(req.Password))
            return Problem(title: PasswordRules.Message, statusCode: 400);

        var user = new User
        {
            FullName = req.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Preferences = new UserPreferences()
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.Categories.AddRange(DefaultCategories.For(user.Id));
        await db.SaveChangesAsync();

        return Ok(new AuthResponse(tokens.Create(user), user.ToProfile()));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Problem(title: "Invalid email or password.", statusCode: 401);
        return Ok(new AuthResponse(tokens.Create(user), user.ToProfile()));
    }
}
