using ProyectoIndursa.IndursaContext;
using ProyectoIndursa.Models;

namespace ProyectoIndursa.AccountFunctions;

public partial class Account
{
    public static bool CheckCurp(IndursaDB db, string curp)
    {
        return !db.Usuarios.Any(s => s.Curp == curp);
    }

    public static bool checkCurp(Usuario user)
    {
        using var db = new IndursaDB();
        return CheckCurp(db, user.Curp);
    }
}
