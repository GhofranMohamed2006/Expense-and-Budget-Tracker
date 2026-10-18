using Microsoft.AspNetCore.Mvc;
using PennywiseApi.Models;

namespace PennywiseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    /// <summary>Values for the Category and Account dropdowns.</summary>
    [HttpGet]
    public object Get() => new { categories = Lookups.Categories, accounts = Lookups.Accounts };
}
