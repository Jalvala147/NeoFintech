using ProyectoIndursa.Helpers;
using ProyectoIndursa.IndursaContext;
using ProyectoIndursa.Models;

namespace ProyectoIndursa.AccountFunctions;

public partial class Account
{
    public static int CreateAccount(IndursaDB db, Usuario user, string password, int tipoCuenta = 1)
    {
        if (!CheckCurp(db, user.Curp))
        {
            throw new InvalidOperationException("La CURP ya está registrada.");
        }

        var newId = NewID(db);
        var cuenta = new Cuentum
        {
            NoCuenta = newId,
            TipoCuenta = tipoCuenta,
            Password = PasswordHelper.Hash(password)
        };
        var infocuenta = new InfoCuentum
        {
            NoCuenta = newId
        };
        user.NoCuenta = newId;
        db.Cuenta.Add(cuenta);
        db.InfoCuenta.Add(infocuenta);
        db.Usuarios.Add(user);
        db.SaveChanges();
        return newId;
    }
}
