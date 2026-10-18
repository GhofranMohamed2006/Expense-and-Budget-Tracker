using System.Net;
using System.Text;
using PennywiseAuth.Models;
using PennywiseAuth.Services;
using PennywiseAuth.Utils;

namespace PennywiseAuth.Controllers;

/// <summary>
/// HTTP controller processing authentication endpoints and transforming results into standard HTTP responses.
/// </summary>
public class AuthController
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Handles POST /api/auth/register
    /// </summary>
    public async Task HandleRegisterAsync(HttpListenerContext context)
    {
        try
        {
            string body = await ReadRequestBodyAsync(context.Request);
            var request = JsonHelper.Deserialize<RegisterRequest>(body);

            var result = await _authService.RegisterAsync(request);

            var responsePayload = result.IsSuccess
                ? AuthResponse.Ok(result.Message, result.Data)
                : AuthResponse.Fail(result.Message, result.Errors);

            await SendJsonResponseAsync(context.Response, result.StatusCode, responsePayload);
        }
        catch (Exception ex)
        {
            // Log internally for developer diagnostics without leaking internal state to client
            Console.Error.WriteLine($"[ERROR] Unhandled error during registration: {ex.Message}");
            var safeError = AuthResponse.Fail("An unexpected error occurred while processing your registration. Please try again later.");
            await SendJsonResponseAsync(context.Response, 500, safeError);
        }
    }

    /// <summary>
    /// Handles POST /api/auth/login
    /// </summary>
    public async Task HandleLoginAsync(HttpListenerContext context)
    {
        try
        {
            string body = await ReadRequestBodyAsync(context.Request);
            var request = JsonHelper.Deserialize<LoginRequest>(body);

            var result = await _authService.LoginAsync(request);

            var responsePayload = result.IsSuccess
                ? AuthResponse.Ok(result.Message, result.Data, _authService.GetDashboardRedirectUrl())
                : AuthResponse.Fail(result.Message, result.Errors);

            await SendJsonResponseAsync(context.Response, result.StatusCode, responsePayload);
        }
        catch (Exception ex)
        {
            // Log internally for developer diagnostics without leaking internal state to client
            Console.Error.WriteLine($"[ERROR] Unhandled error during login: {ex.Message}");
            var safeError = AuthResponse.Fail("An unexpected server error occurred during authentication. Please try again later.");
            await SendJsonResponseAsync(context.Response, 500, safeError);
        }
    }

    /// <summary>
    /// Handles GET /api/auth/health for service liveness checks.
    /// </summary>
    public async Task HandleHealthAsync(HttpListenerContext context)
    {
        var healthPayload = new
        {
            status = "healthy",
            service = "Pennywise Authentication Module",
            timestamp = DateTime.UtcNow
        };
        await SendJsonResponseAsync(context.Response, 200, healthPayload);
    }

    private static async Task<string> ReadRequestBodyAsync(HttpListenerRequest request)
    {
        if (!request.HasEntityBody)
            return string.Empty;

        using var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    public static async Task SendJsonResponseAsync<T>(HttpListenerResponse response, int statusCode, T payload)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(JsonHelper.Serialize(payload));
        response.StatusCode = statusCode;
        response.ContentType = "application/json; charset=utf-8";
        response.ContentLength64 = buffer.Length;

        // Apply CORS headers
        response.Headers["Access-Control-Allow-Origin"] = "*";
        response.Headers["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS";
        response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";

        await response.OutputStream.WriteAsync(buffer);
        response.OutputStream.Close();
    }
}
