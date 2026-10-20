using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Pennywise.Api.Data;
using Pennywise.Api.Services;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

var signingKey = cfg["Jwt:Key"];
if (string.IsNullOrWhiteSpace(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32)
{
    throw new InvalidOperationException(
        "A JWT signing key of at least 32 bytes is required. Set Jwt:Key with .NET user-secrets or the Jwt__Key environment variable.");
}

var sqliteConnection = new SqliteConnectionStringBuilder(
    cfg.GetConnectionString("Default") ?? "Data Source=pennywise.db");
if (sqliteConnection.DataSource != ":memory:" && !Path.IsPathRooted(sqliteConnection.DataSource))
{
    sqliteConnection.DataSource = Path.GetFullPath(
        sqliteConnection.DataSource,
        builder.Environment.ContentRootPath);
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(sqliteConnection.ToString()));
builder.Services.AddScoped<TokenService>();
builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = cfg["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = cfg["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var tokenId = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (tokenId is null)
                    return;

                var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                if (await db.RevokedTokens.AnyAsync(token => token.TokenId == tokenId))
                    context.Fail("This access token has been revoked.");
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var frontendPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "Frontend"));
var authFrontendPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "Login & Register", "Frontend"));
var assetsPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "assests"));
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(frontendPath)
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(authFrontendPath),
    RequestPath = "/auth"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(assetsPath),
    RequestPath = "/assests"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/landing.html"));
app.MapGet("/login", () => Results.Redirect("/auth/login.html"));
app.MapGet("/register", () => Results.Redirect("/auth/register.html"));
app.MapGet("/app", () => Results.Redirect("/app.html"));
app.MapGet("/dashboard", () => Results.Redirect("/app.html#dashboard"));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();
app.Run();
