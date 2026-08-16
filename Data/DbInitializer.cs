using ProyectoIndursa.Helpers;
using ProyectoIndursa.IndursaContext;
using ProyectoIndursa.Models;

namespace ProyectoIndursa.Data;

public static class DbInitializer
{
    public const int EmpleadoNoCuenta = 10000;
    public const string EmpleadoPassword = "Empleado123";

    public static void Seed(IndursaDB db)
    {
        Schema.Ensure(db);

        if (!db.Cuenta.Any(c => c.NoCuenta == EmpleadoNoCuenta))
        {
            db.Cuenta.Add(new Cuentum
            {
                NoCuenta = EmpleadoNoCuenta,
                Password = PasswordHelper.Hash(EmpleadoPassword),
                TipoCuenta = 2
            });
            db.InfoCuenta.Add(new InfoCuentum
            {
                NoCuenta = EmpleadoNoCuenta,
                Saldo = 10000
            });
            db.Usuarios.Add(new Usuario
            {
                Curp = "EMPL000000HDFRRN01",
                Nombre = "Empleado",
                ApellidoPaterno = "NeoFintech",
                ApellidoMaterno = "Demo",
                FechaDeNacimiento = new DateTime(1990, 1, 1),
                NoCuenta = EmpleadoNoCuenta
            });
        }

        if (!db.Empleados.Any(e => e.NoCuenta == EmpleadoNoCuenta))
        {
            var nomina = db.Empleados.Select(e => e.Nomina).ToList().DefaultIfEmpty(0).Max() + 1;
            db.Empleados.Add(new Empleado
            {
                Nomina = nomina,
                NoCuenta = EmpleadoNoCuenta,
                DiasVacaciones = 12
            });
        }

        db.SaveChanges();

        var empleadoDemo = db.Empleados.First(e => e.NoCuenta == EmpleadoNoCuenta);
        if (!db.Gerentes.Any(g => g.Nomina == empleadoDemo.Nomina))
        {
            empleadoDemo.DiasVacaciones = empleadoDemo.DiasVacaciones == 0 ? 12 : empleadoDemo.DiasVacaciones;
            db.Gerentes.Add(new Gerente
            {
                Nomina = empleadoDemo.Nomina,
                DiasVacaciones = empleadoDemo.DiasVacaciones
            });
        }

        AsegurarCuentaDemo(db, EmpleadoNoCuenta, EmpleadoPassword, 2);
        AsegurarCuentaDemo(db, 72235, "waeee", 2);
        AsegurarCuentaDemo(db, 93386, "eeeaaa", 2);

        foreach (var cuenta in db.Cuenta.ToList())
        {
            if (!db.InfoCuenta.Any(i => i.NoCuenta == cuenta.NoCuenta))
            {
                db.InfoCuenta.Add(new InfoCuentum { NoCuenta = cuenta.NoCuenta, Saldo = 10000 });
            }
        }

        db.SaveChanges();
    }

    private static void AsegurarCuentaDemo(IndursaDB db, int noCuenta, string password, int tipoCuenta)
    {
        var cuenta = db.Cuenta.FirstOrDefault(c => c.NoCuenta == noCuenta);
        if (cuenta == null)
        {
            return;
        }

        cuenta.Password = PasswordHelper.Hash(password);
        cuenta.TipoCuenta = tipoCuenta;
        cuenta.MotivoRechazo = null;
    }
}
