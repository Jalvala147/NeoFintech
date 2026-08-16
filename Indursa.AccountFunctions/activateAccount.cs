using ProyectoIndursa.Helpers;
using ProyectoIndursa.IndursaContext;

namespace ProyectoIndursa.AccountFunctions;

public partial class Account
{
    public static bool ActivateAccount(IndursaDB db, int noCuenta, string? password = null)
    {
        var cuenta = db.Cuenta.FirstOrDefault(s => s.NoCuenta == noCuenta);
        if (cuenta == null)
        {
            return false;
        }

        cuenta.TipoCuenta = 2;
        if (!string.IsNullOrWhiteSpace(password))
        {
            cuenta.Password = PasswordHelper.Hash(password);
        }

        db.SaveChanges();
        return true;
    }
}
