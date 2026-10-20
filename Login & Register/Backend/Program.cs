using System.Net;
using System.Runtime.InteropServices;
using PennywiseAuth.Controllers;
using PennywiseAuth.Interfaces;
using PennywiseAuth.Repositories;
using PennywiseAuth.Routes;
using PennywiseAuth.Services;

namespace PennywiseAuth;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=================================================");
        Console.WriteLine("   Pennywise Authentication Module Server        ");
        Console.WriteLine("=================================================");

        // Determine port from environment or fallback to 5005
        string portStr = Environment.GetEnvironmentVariable("PORT") ?? "5005";
        if (!int.TryParse(portStr, out int port))
        {
            port = 5005;
        }

        // Determine database connection string (Development SQLite Adapter)
        string dbConnection = Environment.GetEnvironmentVariable("PENNYWISE_DB_CONN") ?? "Data Source=pennywise_dev.db";

        // Determine Dashboard redirect destination
        string dashboardUrl = Environment.GetEnvironmentVariable("PENNYWISE_DASHBOARD_URL") ?? "/dashboard.html";

        // Locate Frontend directory
        string frontendPath = ResolveFrontendDirectory();
        Console.WriteLine($"[INFO] Serving frontend assets from: {frontendPath}");
        Console.WriteLine($"[INFO] Dashboard destination URL: {dashboardUrl}");
        Console.WriteLine($"[INFO] Initializing SQLite development repository ({dbConnection})...");

        // Initialize repository and schema
        IUserRepository userRepository = new SqliteUserRepository(dbConnection);
        await userRepository.InitializeAsync();
        Console.WriteLine("[INFO] Database repository initialized successfully.");

        // Wire services, controller, and router
        var authService = new AuthService(userRepository, dashboardUrl);
        var authController = new AuthController(authService);
        var router = new AuthRoutes(authController, frontendPath);

        // Configure HttpListener with cross-platform resilience
        using var listener = new HttpListener();
        ConfigureListenerPrefixes(listener, port);

        try
        {
            listener.Start();
        }
        catch (HttpListenerException ex)
        {
            Console.WriteLine($"[WARN] Could not bind wildcard prefix ({ex.Message}). Falling back to http://localhost:{port}/");
            listener.Prefixes.Clear();
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();
        }

        Console.WriteLine($"[INFO] Server running and listening on: http://localhost:{port}/");
        Console.WriteLine($"[INFO] Open in browser: http://localhost:{port}/login.html");
        Console.WriteLine("[INFO] Press Ctrl+C to terminate the server.\n");

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            Console.WriteLine("\n[INFO] Graceful shutdown initiated...");
            cts.Cancel();
            try { listener.Stop(); } catch { }
        };

        // Main HTTP processing loop
        while (!cts.IsCancellationRequested)
        {
            try
            {
                var getContextTask = listener.GetContextAsync();
                var completedTask = await Task.WhenAny(getContextTask, Task.Delay(-1, cts.Token));

                if (completedTask != getContextTask)
                    break; // Cancellation requested

                var context = await getContextTask;
                // Dispatch asynchronously without blocking next request
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await router.RouteAsync(context);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"[ERROR] Route processing failure: {ex.Message}");
                        try
                        {
                            context.Response.StatusCode = 500;
                            context.Response.OutputStream.Close();
                        }
                        catch { }
                    }
                });
            }
            catch (HttpListenerException) when (cts.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                if (!cts.IsCancellationRequested)
                {
                    Console.Error.WriteLine($"[ERROR] Listener loop error: {ex.Message}");
                }
            }
        }

        Console.WriteLine("[INFO] Server stopped safely.");
    }

    private static void ConfigureListenerPrefixes(HttpListener listener, int port)
    {
        // On Unix/Linux/Replit containers, wildcard or 0.0.0.0 prefix allows external routing
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            listener.Prefixes.Add($"http://*:{port}/");
        }
        else
        {
            // On Windows, localhost avoids requiring administrator URL reservations
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        }
    }

    private static string ResolveFrontendDirectory()
    {
        // Check relative paths from current working directory or binary directory
        string[] candidatePaths =
        [
            Path.Combine(Directory.GetCurrentDirectory(), "Frontend"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "Frontend"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Frontend"),
            Path.Combine(AppContext.BaseDirectory, "Frontend")
        ];

        foreach (var path in candidatePaths)
        {
            string full = Path.GetFullPath(path);
            if (Directory.Exists(full))
            {
                return full;
            }
        }

        // Default to "../Frontend"
        return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Frontend"));
    }
}
