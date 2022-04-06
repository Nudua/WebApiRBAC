namespace WebApiRBAC.Services;

public interface IUsersService
{
    Task<List<string>> GetRolesAsync(Guid userId);
}
