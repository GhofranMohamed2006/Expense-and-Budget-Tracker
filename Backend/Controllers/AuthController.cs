using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        if (!PasswordRules.IsStrong(req.Password))
            return Problem(title: PasswordRules.Message, statusCode: 400);

        var user = new User
        {
            FullName = req.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Preferences = new UserPreferences()
        };
        await using var transaction = await db.Database.BeginTransactionAsync();
        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            return Problem(title: "This email is already registered.", statusCode: 409);
        }

        db.Categories.AddRange(DefaultCategories.For(user.Id));
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
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

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var tokenId = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var expiration = User.FindFirstValue(JwtRegisteredClaimNames.Exp)
            ?? User.FindFirstValue(ClaimTypes.Expiration);
        if (tokenId is null || !long.TryParse(expiration, out var expiresAt))
            return Problem(title: "The access token is missing revocation claims.", statusCode: 401);

        var now = DateTime.UtcNow;
        await db.RevokedTokens
            .Where(token => token.ExpiresAt <= now)
            .ExecuteDeleteAsync();
        db.RevokedTokens.Add(new RevokedToken
        {
            TokenId = tokenId,
            ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAt).UtcDateTime
        });
        await db.SaveChangesAsync();
        return NoContent();
    }
}
