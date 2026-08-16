using ProyectoIndursa.IndursaContext;

namespace ProyectoIndursa.AccountFunctions;

public partial class Account
{
    private static int NewID(IndursaDB db)
    {
        var max = db.Cuenta.Select(s => s.NoCuenta).ToList().DefaultIfEmpty(9999).Max();
        return max + 1;
    }
}
