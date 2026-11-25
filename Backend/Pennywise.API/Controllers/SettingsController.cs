using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pennywise.BLL.DTOs;
using Pennywise.BLL.Services;

namespace Pennywise.API.Controllers;

[Authorize]
[Route("api/settings")]
public class SettingsController(IProfileService profile) : ApiControllerBase
{
    [HttpGet("profile")]
    public async Task<ActionResult<ProfileDto>> GetProfile() => Ok(await profile.GetAsync(UserId));

    [HttpPut("profile")]
    public async Task<ActionResult<ProfileDto>> UpdateProfile(UpdateProfileDto dto) => Ok(await profile.UpdateAsync(UserId, dto));

    [HttpPost("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        await profile.ChangePasswordAsync(UserId, dto);
        return NoContent();
    }
}
