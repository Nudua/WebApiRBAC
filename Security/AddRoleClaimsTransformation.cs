using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Security.Claims;
using WebApiRBAC.Services;

namespace WebApiRBAC.Security;

public class AddRoleClaimsTransformation : IClaimsTransformation
{
    private readonly IUsersService _usersService;
    private readonly IMemoryCache _memoryCache;

    public AddRoleClaimsTransformation(IUsersService usersService, IMemoryCache memoryCache)
    {
        _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // User is not authenticated so just return right away
        if (principal.Identity?.IsAuthenticated is false)
        {
            return principal;
        }

        // To be able to find the roles assigned to an user we need to use an unique identifier for this person.
        // Generally the 'sub' (subject) claim can be used as an unique identifier.
        // By default the 'sub' claim is mapped by ASP.NET Core to the NameIdentifier
        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

        // If it is not you can try to grab it by using the 'sub' name directly.
        // var idClaim = principal.FindFirst("sub");

        if (idClaim is null)
        {
            Debug.WriteLine("Id claim missing for user.");
            return principal;
        }

        Debug.WriteLine($"Adding roles to the user: {idClaim.Value}");

        // Fetch roles based on the ID of the authenticated user
        // Non-cached version
        // var roles = await _usersService.GetRolesAsync(Guid.Parse(idClaim.Value));

        // Cached version
        var roles = await _memoryCache.GetOrCreateAsync($"{idClaim.Value}_roles", async (entry) =>
        {
            // This code block is only executed if the value is not in the cache (cache miss)
            var roles = await _usersService.GetRolesAsync(Guid.Parse(idClaim.Value));

            entry.SetValue(roles);
            entry.SetSlidingExpiration(TimeSpan.FromMinutes(15));
            entry.SetAbsoluteExpiration(TimeSpan.FromHours(4));

            return roles;
        });

        // If we don't find any roles, just return
        if (roles.Count == 0)
        {
            Debug.WriteLine($"No roles found for: {idClaim.Value}");
            return principal;
        }

        // Clone the principal
        var clonedPrincipal = principal.Clone();
        var clonedIdentity = (ClaimsIdentity)clonedPrincipal.Identity;

        foreach (var role in roles)
        {
            // Here we add each role as a Role Claim type.
            clonedIdentity.AddClaim(new Claim(ClaimTypes.Role, role, ClaimValueTypes.String, "https://localhost:5001"));
        }

        return clonedPrincipal;
    }
}