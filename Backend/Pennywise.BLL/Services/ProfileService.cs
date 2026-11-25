using Pennywise.BLL.DTOs;
using Pennywise.BLL.Exceptions;
using Pennywise.DAL.Entities;
using Pennywise.DAL.Repositories;

namespace Pennywise.BLL.Services;

public interface IProfileService
{
    Task<ProfileDto> GetAsync(int userId);
    Task<ProfileDto> UpdateAsync(int userId, UpdateProfileDto dto);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
}

public class ProfileService(IUnitOfWork uow) : IProfileService
{
    public async Task<ProfileDto> GetAsync(int userId) => ToDto(await Find(userId));

    public async Task<ProfileDto> UpdateAsync(int userId, UpdateProfileDto dto)
    {
        var user = await Find(userId);
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await uow.Users.AnyAsync(u => u.Email == email && u.Id != userId))
            throw new ConflictException("This email is already used by another account.");

        user.FullName = dto.FullName.Trim();
        user.Email = email;
        await uow.SaveChangesAsync();
        return ToDto(user);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await Find(userId);
        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new BusinessRuleException("Current password is incorrect.");
        if (dto.NewPassword != dto.ConfirmPassword)
            throw new BusinessRuleException("Passwords do not match.");
        if (dto.NewPassword == dto.CurrentPassword)
            throw new BusinessRuleException("New password must differ from the current one.");
        if (!PasswordRules.IsStrong(dto.NewPassword))
            throw new BusinessRuleException(PasswordRules.Message);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await uow.SaveChangesAsync();
    }

    private async Task<User> Find(int userId) =>
        await uow.Users.GetByIdAsync(userId) ?? throw new NotFoundException("User not found.");

    private static ProfileDto ToDto(User u) => new(u.FullName, u.Email, u.CreatedAt);
}
