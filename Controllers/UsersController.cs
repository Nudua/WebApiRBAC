using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace WebApiRBAC.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    // GET: https://localhost:5001/users/roles
    [Authorize]
    [HttpGet("roles")]
    [SwaggerOperation(Summary = "Returns a list of your current role claims")]
    public IActionResult GetRoles()
    {
        // Find all our role claims
        var claims = User.FindAll(ClaimTypes.Role);

        var items = new List<string>();

        foreach (var claim in claims)
        {
            items.Add($"Type: {claim.Type} Value: {claim.Value} Issuer: {claim.Issuer}");
        }

        // Return a list of all role claims
        return Ok(items);
    }

    // GET: https://localhost:5001/users/admin
    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    [SwaggerOperation(Summary = "Test endpoint that is only accessible to users with the Admin role")]
    public IActionResult AdminOnly()
    {
        return Ok("Admin only here");
    }
}