namespace Infraestructure.Constants;

public class RoleContants
{
    public const string Admin = nameof(Admin);
    public const string Basic = nameof(Basic);

    public static IReadOnlyCollection<string> DefaultRoles => [Admin, Basic];

    public static bool IsDefaultRole(string roleName) => DefaultRoles.Contains(roleName);
}
