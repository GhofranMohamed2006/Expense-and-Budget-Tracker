using System.Net;
using System.Text;
using PennywiseAuth.Controllers;

namespace PennywiseAuth.Routes;

/// <summary>
/// Router dispatching incoming HTTP requests to controllers or static file providers.
/// </summary>
public class AuthRoutes
{
    private readonly AuthController _authController;
    private readonly string _frontendDirectory;

    private static readonly Dictionary<string, string> MimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".html", "text/html; charset=utf-8" },
        { ".htm", "text/html; charset=utf-8" },
        { ".css", "text/css; charset=utf-8" },
        { ".js", "application/javascript; charset=utf-8" },
        { ".json", "application/json; charset=utf-8" },
        { ".svg", "image/svg+xml" },
        { ".png", "image/png" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".ico", "image/x-icon" },
        { ".woff2", "font/woff2" }
    };

    public AuthRoutes(AuthController authController, string frontendDirectory)
    {
        _authController = authController;
        _frontendDirectory = frontendDirectory;
    }

    /// <summary>
    /// Processes an incoming HTTP context.
    /// </summary>
    public async Task RouteAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        // Apply global CORS headers
        response.Headers["Access-Control-Allow-Origin"] = "*";
        response.Headers["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS";
        response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";

        // Handle CORS preflight
        if (string.Equals(request.HttpMethod, "OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            response.StatusCode = (int)HttpStatusCode.NoContent;
            response.OutputStream.Close();
            return;
        }

        string path = request.Url?.AbsolutePath ?? "/";

        // Route API endpoints
        if (path.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase))
        {
            await RouteApiAsync(path, request.HttpMethod, context);
            return;
        }

        // Serve Frontend static assets
        await ServeStaticFileAsync(path, context);
    }

    private async Task RouteApiAsync(string path, string method, HttpListenerContext context)
    {
        string normalized = path.TrimEnd('/');

        if (string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(normalized, "/api/auth/register", StringComparison.OrdinalIgnoreCase))
            {
                await _authController.HandleRegisterAsync(context);
                return;
            }

            if (string.Equals(normalized, "/api/auth/login", StringComparison.OrdinalIgnoreCase))
            {
                await _authController.HandleLoginAsync(context);
                return;
            }
        }
        else if (string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(normalized, "/api/auth/health", StringComparison.OrdinalIgnoreCase))
            {
                await _authController.HandleHealthAsync(context);
                return;
            }
        }

        // 404 for unhandled API endpoints
        await AuthController.SendJsonResponseAsync(
            context.Response,
            (int)HttpStatusCode.NotFound,
            new { success = false, message = $"Endpoint '{path}' with method '{method}' was not found." });
    }

    private async Task ServeStaticFileAsync(string relativePath, HttpListenerContext context)
    {
        var response = context.Response;

        // Default root to login.html
        if (string.IsNullOrEmpty(relativePath) || relativePath == "/")
        {
            relativePath = "/login.html";
        }

        // Sanitize path to prevent directory traversal
        string sanitized = relativePath.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        string fullPath = Path.GetFullPath(Path.Combine(_frontendDirectory, sanitized));

        // Security check: ensure path remains inside frontend directory
        if (!fullPath.StartsWith(Path.GetFullPath(_frontendDirectory), StringComparison.OrdinalIgnoreCase))
        {
            response.StatusCode = (int)HttpStatusCode.Forbidden;
            response.OutputStream.Close();
            return;
        }

        if (!File.Exists(fullPath))
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            byte[] errorBytes = Encoding.UTF8.GetBytes($"404 - File Not Found: {relativePath}");
            response.ContentType = "text/plain; charset=utf-8";
            response.ContentLength64 = errorBytes.Length;
            await response.OutputStream.WriteAsync(errorBytes);
            response.OutputStream.Close();
            return;
        }

        string ext = Path.GetExtension(fullPath);
        string contentType = MimeTypes.TryGetValue(ext, out var mime) ? mime : "application/octet-stream";

        try
        {
            byte[] fileBytes = await File.ReadAllBytesAsync(fullPath);
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = contentType;
            response.ContentLength64 = fileBytes.Length;
            await response.OutputStream.WriteAsync(fileBytes);
        }
        catch (Exception ex)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            Console.Error.WriteLine($"[ERROR] Failed to serve static file '{fullPath}': {ex.Message}");
        }
        finally
        {
            response.OutputStream.Close();
        }
    }
}
