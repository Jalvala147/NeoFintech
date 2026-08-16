using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIndursa.AccountFunctions;
using ProyectoIndursa.Helpers;
using ProyectoIndursa.IndursaContext;
using ProyectoIndursa.Models;

namespace ProyectoIndursa.Controllers;

[Authorize(Roles = "Gerente")]
public class GerenteController : Controller
{
    private readonly IndursaDB _db;

    public GerenteController(IndursaDB db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var empleados = _db.Empleados.ToList();
        var usuarios = _db.Usuarios.ToList();
        var cuentas = _db.Cuenta.ToList();
        var gerentes = _db.Gerentes.ToList();

        var lista = empleados.Select(e =>
        {
            var usuario = usuarios.FirstOrDefault(u => u.NoCuenta == e.NoCuenta);
            var cuenta = cuentas.FirstOrDefault(c => c.NoCuenta == e.NoCuenta);
            return new EmpleadoListaItem
            {
                Nomina = e.Nomina,
                NoCuenta = e.NoCuenta,
                Nombre = usuario == null ? "N/D" : $"{usuario.Nombre} {usuario.ApellidoPaterno}",
                DiasVacaciones = e.DiasVacaciones,
                EsGerente = gerentes.Any(g => g.Nomina == e.Nomina),
                Activo = cuenta?.TipoCuenta == 2
            };
        }).ToList();

        return View(lista);
    }

    [HttpGet]
    public IActionResult Registrar() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Registrar(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.FechaDeNacimiento is null || model.FechaDeNacimiento.Value.Year < 1900)
        {
            ModelState.AddModelError(nameof(model.FechaDeNacimiento), "Ingresa una fecha válida.");
            return View(model);
        }

        var user = new Usuario
        {
            Curp = model.Curp.Trim().ToUpperInvariant(),
            Nombre = model.Nombre.Trim(),
            ApellidoPaterno = model.ApellidoPaterno.Trim(),
            ApellidoMaterno = model.ApellidoMaterno.Trim(),
            FechaDeNacimiento = model.FechaDeNacimiento.Value
        };

        if (!Account.CheckCurp(_db, user.Curp))
        {
            ModelState.AddModelError(nameof(model.Curp), "Esta CURP ya está registrada.");
            return View(model);
        }

        var noCuenta = Account.CreateAccount(_db, user, model.Password, tipoCuenta: 2);
        var nomina = _db.Empleados.Select(e => e.Nomina).ToList().DefaultIfEmpty(0).Max() + 1;
        _db.Empleados.Add(new Empleado
        {
            Nomina = nomina,
            NoCuenta = noCuenta
        });
        _db.SaveChanges();
        TempData["Ok"] = $"Empleado creado. Nómina {nomina}, cuenta {noCuenta}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Vacaciones(long nomina, int dias)
    {
        var empleado = _db.Empleados.FirstOrDefault(e => e.Nomina == nomina);
        if (empleado == null)
        {
            TempData["Error"] = "No se encontró al empleado.";
            return RedirectToAction(nameof(Index));
        }

        empleado.DiasVacaciones = Math.Max(0, dias);
        var gerente = _db.Gerentes.FirstOrDefault(g => g.Nomina == nomina);
        if (gerente != null)
        {
            gerente.DiasVacaciones = empleado.DiasVacaciones;
        }

        _db.SaveChanges();
        TempData["Ok"] = "Días de vacaciones actualizados.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Desactivar(int noCuenta)
    {
        var actual = UserAccount.GetNoCuenta(User);
        if (actual == noCuenta)
        {
            TempData["Error"] = "No puedes desactivar tu propia cuenta.";
            return RedirectToAction(nameof(Index));
        }

        var cuenta = _db.Cuenta.FirstOrDefault(c => c.NoCuenta == noCuenta);
        if (cuenta == null)
        {
            TempData["Error"] = "No se encontró la cuenta.";
            return RedirectToAction(nameof(Index));
        }

        cuenta.TipoCuenta = 3;
        cuenta.MotivoRechazo = "Empleado desactivado por gerencia";
        _db.SaveChanges();
        TempData["Ok"] = $"La cuenta {noCuenta} fue desactivada.";
        return RedirectToAction(nameof(Index));
    }
}
