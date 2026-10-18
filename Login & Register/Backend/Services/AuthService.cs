using System.Net.Mail;
using System.Text.RegularExpressions;
using PennywiseAuth.Interfaces;
using PennywiseAuth.Models;
using PennywiseAuth.Security;

namespace PennywiseAuth.Services;

/// <summary>
/// Result wrapper carrying authentication outcome, HTTP status code, and payload.
/// </summary>
public class ServiceResult<T>
{
    public int StatusCode { get; set; }
    public bool IsSuccess => StatusCode >= 200 && StatusCode <= 299;
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }

    public static ServiceResult<T> Success(int statusCode, string message, T? data = default) =>
        new() { StatusCode = statusCode, Message = message, Data = data };

    public static ServiceResult<T> Failure(int statusCode, string message, List<string>? errors = null) =>
        new() { StatusCode = statusCode, Message = message, Errors = errors ?? (string.IsNullOrEmpty(message) ? null : new List<string> { message }) };
}

/// <summary>
/// Core authentication service managing business logic, input validation, and password security.
/// </summary>
public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly string _dashboardRedirectUrl;

    // Email regex complying with RFC 5322 simplified standard
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Password requirements: min 8 chars, 1 uppercase, 1 lowercase, 1 number, 1 special character
    private static readonly Regex PasswordComplexityRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
        RegexOptions.Compiled);

    public AuthService(IUserRepository userRepository, string dashboardRedirectUrl = "/dashboard.html")
    {
        _userRepository = userRepository;
        _dashboardRedirectUrl = dashboardRedirectUrl;
    }

    /// <summary>
    /// Registers a new user following full validation and password hashing.
    /// </summary>
    public async Task<ServiceResult<UserProfileDto>> RegisterAsync(RegisterRequest? request)
    {
        if (request == null)
        {
            return ServiceResult<UserProfileDto>.Failure(400, "Request body is required.");
        }

        var validationErrors = new List<string>();

        // 1. Name validation
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            validationErrors.Add("Full name is required.");
        }
        else if (request.Name.Trim().Length < 2 || request.Name.Trim().Length > 100)
        {
            validationErrors.Add("Full name must be between 2 and 100 characters.");
        }

        // 2. Email validation
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            validationErrors.Add("Email address is required.");
        }
        else if (!EmailRegex.IsMatch(request.Email.Trim()))
        {
            validationErrors.Add("Please enter a valid email address.");
        }

        // 3. Password presence & confirmation
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            validationErrors.Add("Password is required.");
        }
        else if (!PasswordComplexityRegex.IsMatch(request.Password))
        {
            validationErrors.Add("Password must be at least 8 characters long and include an uppercase letter, a lowercase letter, a number, and a special character.");
        }

        if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
        {
            validationErrors.Add("Password confirmation is required.");
        }
        else if (request.Password != request.ConfirmPassword)
        {
            validationErrors.Add("Passwords do not match.");
        }

        if (validationErrors.Count > 0)
        {
            return ServiceResult<UserProfileDto>.Failure(400, "Validation failed.", validationErrors);
        }

        string normalizedEmail = request.Email!.Trim().ToLowerInvariant();

        // 4. Duplicate email check
        bool exists = await _userRepository.EmailExistsAsync(normalizedEmail);
        if (exists)
        {
            return ServiceResult<UserProfileDto>.Failure(409, "An account with this email address already exists.");
        }

        // 5. Secure password hashing
        string passwordHash = PasswordHasher.HashPassword(request.Password!, out string salt);

        // 6. Persistence
        var newUser = new AuthUser
        {
            Name = request.Name!.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Salt = salt,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _userRepository.CreateUserAsync(newUser);

        var profile = new UserProfileDto
        {
            Id = created.Id,
            Name = created.Name,
            Email = created.Email,
            CreatedAt = created.CreatedAt
        };

        return ServiceResult<UserProfileDto>.Success(201, "Account created successfully.", profile);
    }

    /// <summary>
    /// Authenticates existing credentials and returns safe user profile.
    /// </summary>
    public async Task<ServiceResult<UserProfileDto>> LoginAsync(LoginRequest? request)
    {
        if (request == null)
        {
            return ServiceResult<UserProfileDto>.Failure(400, "Request body is required.");
        }

        var validationErrors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            validationErrors.Add("Email address is required.");
        }
        else if (!EmailRegex.IsMatch(request.Email.Trim()))
        {
            validationErrors.Add("Please enter a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            validationErrors.Add("Password is required.");
        }

        if (validationErrors.Count > 0)
        {
            return ServiceResult<UserProfileDto>.Failure(400, "Validation failed.", validationErrors);
        }

        string normalizedEmail = request.Email!.Trim().ToLowerInvariant();

        // Look up user
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (user == null)
        {
            // Use generic error to avoid user enumeration
            return ServiceResult<UserProfileDto>.Failure(401, "Invalid email or password.");
        }

        // Verify password hash with constant-time equality check
        bool isValid = PasswordHasher.VerifyPassword(request.Password!, user.PasswordHash, user.Salt);
        if (!isValid)
        {
            return ServiceResult<UserProfileDto>.Failure(401, "Invalid email or password.");
        }

        // Update last login
        var now = DateTime.UtcNow;
        await _userRepository.UpdateLastLoginAsync(user.Id, now);

        var profile = new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            LastLoginAt = now
        };

        return ServiceResult<UserProfileDto>.Success(200, "Login successful.", profile);
    }

    public string GetDashboardRedirectUrl() => _dashboardRedirectUrl;
}
