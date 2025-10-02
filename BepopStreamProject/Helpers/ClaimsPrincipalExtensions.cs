using System.Security.Claims;

namespace BepopStreamProject.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserLevel(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirst("membershipLevel")?.Value ?? "0");
        }

        public static int GetUserId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirst("userId")?.Value ?? "0");
        }
        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst("username")?.Value ?? "Misafir";
        }
    }
}
