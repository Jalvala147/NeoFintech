using System.Security.Claims;

namespace ProyectoIndursa.Helpers;

public static class UserAccount
{
    public static int? GetNoCuenta(ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out var noCuenta) ? noCuenta : null;
    }
}
