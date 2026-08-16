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

        if (!db.Empleados.Any())
        {
            db.Empleados.Add(new Empleado
            {
                Nomina = 1,
                NoCuenta = EmpleadoNoCuenta
            });
        }

        db.SaveChanges();

        if (!db.Gerentes.Any())
        {
            var empleado = db.Empleados.First();
            empleado.DiasVacaciones = 12;
            db.Gerentes.Add(new Gerente
            {
                Nomina = empleado.Nomina,
                DiasVacaciones = 12
            });
            db.SaveChanges();
        }
    }
}
