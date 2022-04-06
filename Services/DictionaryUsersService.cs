namespace WebApiRBAC.Services;

public class DictionaryUsersService : IUsersService
{
    private readonly Dictionary<Guid, List<string>> _users = new()
    {
        // Update these Guids to match users that you've added to your IDP
        { Guid.Parse("6989e9a5-0813-44bc-8c5b-e74de37450a2"), new List<string> { "Admin", "User" } }, // Alice
        { Guid.Parse("80e0a2a8-0535-4497-93f2-a4e2acebc27e"), new List<string> { "User" } },          // Bob
    };

    public async Task<List<string>> GetRolesAsync(Guid userId)
    {
        // Simulate a database hit
        await Task.Delay(1000);

        // Get the roles associated with the ID if it exists in the dictionary
        if (_users.TryGetValue(userId, out List<string> roles))
        {
            return roles;
        }

        // User not found, so just return an empty list
        return new List<string>();
    }
}
