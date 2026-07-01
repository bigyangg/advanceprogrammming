using Microsoft.AspNetCore.Http;

namespace CEMS.Infrastructure;

public static class SessionUserExtensions
{
    private const string UserIdKey = "CurrentUserId";
    private const string UserRoleKey = "CurrentUserRole";
    private const string UserNameKey = "CurrentUserName";

    public static void SignInAs(this ISession session, int userId, string role, string name)
    {
        session.SetInt32(UserIdKey, userId);
        session.SetString(UserRoleKey, role);
        session.SetString(UserNameKey, name);
    }

    public static void SignOut(this ISession session)
    {
        session.Remove(UserIdKey);
        session.Remove(UserRoleKey);
        session.Remove(UserNameKey);
    }

    public static int? CurrentUserId(this ISession session) => session.GetInt32(UserIdKey);

    public static string? CurrentUserRole(this ISession session) => session.GetString(UserRoleKey);

    public static string? CurrentUserName(this ISession session) => session.GetString(UserNameKey);
}
