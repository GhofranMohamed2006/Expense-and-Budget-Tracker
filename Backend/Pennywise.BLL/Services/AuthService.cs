using Pennywise.BLL.DTOs;
using Pennywise.BLL.Exceptions;
using Pennywise.DAL.Entities;
using Pennywise.DAL.Repositories;

namespace Pennywise.BLL.Services;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterDto dto);
    Task<AuthResultDto> LoginAsync(LoginDto dto);
}

public class AuthService(IUnitOfWork uow, ITokenService tokens) : IAuthService
{
    public async Task<AuthResultDto> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await uow.Users.EmailExistsAsync(email))
            throw new ConflictException("This email is already registered.");
        if (!PasswordRules.IsStrong(dto.Password))
            throw new BusinessRuleException("Password must be at least 8 characters and include at least 3 of: lowercase, uppercase, number, symbol.");

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        // Navigation property: default categories are saved together with the user in one SaveChanges.
        foreach (var (name, type) in DefaultCategories.All)
            user.Categories.Add(new Category { Name = name, Type = type, IsDefault = true });

        await uow.Users.AddAsync(user);
        await uow.SaveChangesAsync();
        return new AuthResultDto(tokens.Create(user), ToDto(user));
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto)
    {
        var user = await uow.Users.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant());
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");
        return new AuthResultDto(tokens.Create(user), ToDto(user));
    }

    private static UserDto ToDto(User u) => new(u.Id, u.FullName, u.Email, u.CreatedAt);

}

public static class DefaultCategories
{
    public static readonly (string Name, CategoryType Type)[] All =
    {
        ("Food", CategoryType.Expense), ("Transportation", CategoryType.Expense),
        ("Shopping", CategoryType.Expense), ("Bills", CategoryType.Expense),
        ("Healthcare", CategoryType.Expense), ("Education", CategoryType.Expense),
        ("Entertainment", CategoryType.Expense), ("Other", CategoryType.Expense),
        ("Salary", CategoryType.Income), ("Freelancing", CategoryType.Income),
        ("Gifts", CategoryType.Income), ("Other Income", CategoryType.Income)
    };
}
