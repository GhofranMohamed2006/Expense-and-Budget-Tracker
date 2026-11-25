using Microsoft.AspNetCore.Mvc;
using Pennywise.BLL.DTOs;
using Pennywise.BLL.Services;

namespace Pennywise.API.Controllers;

[Route("api/auth")]
public class AuthController(IAuthService auth) : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResultDto>> Register(RegisterDto dto) => Ok(await auth.RegisterAsync(dto));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login(LoginDto dto) => Ok(await auth.LoginAsync(dto));
}
