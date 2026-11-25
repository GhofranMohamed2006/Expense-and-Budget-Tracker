using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Pennywise.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
