using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoIndursa.AccountFunctions;
using ProyectoIndursa.IndursaContext;
using ProyectoIndursa.Models;
using ProyectoIndursa.Services;

namespace ProyectoIndursa.Controllers;

[Authorize(Roles = "Empleado,Gerente")]
public class EmployeeController : Controller
{
    private readonly ILogger<EmployeeController> _logger;
    private readonly IndursaDB _db;
    private readonly BancaService _banca;

    public EmployeeController(ILogger<EmployeeController> logger, IndursaDB db, BancaService banca)
    {
        _logger = logger;
        _db = db;
        _banca = banca;
    }

    public IActionResult Index()
    {
        ViewBag.PendientesCuentas = _db.Cuenta.Count(c => c.TipoCuenta == 1);
        ViewBag.PendientesPrestamos = _db.EstadoPrestamos.Count(e => e.Estado == 0);
        ViewBag.BoletosRifa = _db.Rifas.Count(r => r.Ganador == 0);
        return View();
    }

    public IActionResult Cuentas()
    {
        var cuentas = (
            from u in _db.Usuarios
            join c in _db.Cuenta on u.NoCuenta equals c.NoCuenta
            select new CuentaListaItem
            {
                NoCuenta = u.NoCuenta,
                Nombre = u.Nombre,
                ApellidoPaterno = u.ApellidoPaterno,
                ApellidoMaterno = u.ApellidoMaterno,
                Curp = u.Curp,
                TipoCuenta = c.TipoCuenta,
                MotivoRechazo = c.MotivoRechazo
            }
        ).ToList();

        return View(cuentas);
    }

    public IActionResult Prestamos()
    {
        var estados = _db.EstadoPrestamos.ToList();
        var folios = estados.Select(e => e.Folio).ToList();
        var prestamos = _db.Prestamos.Where(p => folios.Contains(p.Folio)).ToList();
        var datos = _db.DatosPrestamos.Where(d => folios.Contains(d.Folio)).ToList();
        var usuarios = _db.Usuarios.ToList();

        var lista = estados.Select(e =>
        {
            var prestamo = prestamos.FirstOrDefault(p => p.Folio == e.Folio);
            var dato = datos.FirstOrDefault(d => d.Folio == e.Folio);
            var usuario = usuarios.FirstOrDefault(u => u.NoCuenta == dato?.SolicitadoPor);
            var cantidad = prestamo?.Cantidad ?? 0;
            return new PrestamoListaItem
            {
                Folio = e.Folio,
                Cantidad = cantidad,
                PagoRealizados = e.PagoRealizados,
                PagoPedientes = e.PagoPedientes,
                Estado = e.Estado,
                FechaExpedicion = dato?.FechaExpedicion ?? default,
                FechaLimite = dato?.FechaLimite ?? default,
                SolicitadoPor = dato?.SolicitadoPor ?? 0,
                Solicitante = usuario == null ? "N/D" : $"{usuario.Nombre} {usuario.ApellidoPaterno}",
                Mensualidad = Helpers.Banca.Mensualidad(cantidad),
                MotivoRechazo = e.MotivoRechazo
            };
        }).OrderBy(p => p.Estado).ThenByDescending(p => p.FechaExpedicion).ToList();

        return View(lista);
    }

    public IActionResult Rifa()
    {
        var boletos = _db.Rifas.OrderByDescending(r => r.NoBoleto).ToList();
        ViewBag.Participantes = boletos.Count(b => b.Ganador == 0);
        return View(boletos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SortearRifa()
    {
        var resultado = _banca.SortearRifa();
        TempData[resultado.Ok ? "Ok" : "Error"] = resultado.Mensaje;
        return RedirectToAction(nameof(Rifa));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AcceptAccount(int noCuenta)
    {
        if (!Account.ActivateAccount(_db, noCuenta))
        {
            TempData["Error"] = "No se encontró la cuenta.";
        }
        else
        {
            _banca.Notificar(noCuenta, "Tu cuenta fue aprobada. Ya puedes usar NeoFintech.");
            _db.SaveChanges();
            TempData["Ok"] = $"La cuenta {noCuenta} fue aceptada.";
        }

        return RedirectToAction(nameof(Cuentas));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RejectAccount(int noCuenta, string? motivo)
    {
        var cuenta = _db.Cuenta.FirstOrDefault(s => s.NoCuenta == noCuenta);
        if (cuenta == null)
        {
            TempData["Error"] = "No se encontró la cuenta.";
        }
        else
        {
            cuenta.TipoCuenta = 3;
            cuenta.MotivoRechazo = string.IsNullOrWhiteSpace(motivo) ? "Sin motivo" : motivo.Trim();
            _banca.Notificar(noCuenta, $"Tu solicitud de cuenta fue rechazada: {cuenta.MotivoRechazo}");
            _db.SaveChanges();
            TempData["Ok"] = $"La cuenta {noCuenta} fue rechazada.";
        }

        return RedirectToAction(nameof(Cuentas));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AcceptPrestamo(int folio)
    {
        using var tx = _db.Database.BeginTransaction();
        var estado = _db.EstadoPrestamos.FirstOrDefault(e => e.Folio == folio);
        var prestamo = _db.Prestamos.FirstOrDefault(p => p.Folio == folio);
        var datos = _db.DatosPrestamos.FirstOrDefault(d => d.Folio == folio);

        if (estado == null || prestamo == null || datos == null)
        {
            TempData["Error"] = "No se encontró el préstamo.";
            return RedirectToAction(nameof(Prestamos));
        }

        if (estado.Estado != 0)
        {
            TempData["Error"] = "Este préstamo ya fue revisado.";
            return RedirectToAction(nameof(Prestamos));
        }

        estado.Estado = 1;
        datos.FechaAprobacion = DateTime.Now;
        var acreditado = _banca.AcreditarPrestamo(datos.SolicitadoPor, prestamo.Cantidad, folio);
        if (!acreditado.Ok)
        {
            TempData["Error"] = acreditado.Mensaje;
            return RedirectToAction(nameof(Prestamos));
        }

        _banca.Notificar(datos.SolicitadoPor, $"Tu préstamo folio {folio} fue aprobado. Se acreditaron ${prestamo.Cantidad:N2}.");
        _db.SaveChanges();
        tx.Commit();
        TempData["Ok"] = $"Préstamo {folio} aprobado. Se acreditó ${prestamo.Cantidad:N2} a la cuenta {datos.SolicitadoPor}.";
        return RedirectToAction(nameof(Prestamos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RejectPrestamo(int folio, string? motivo)
    {
        using var tx = _db.Database.BeginTransaction();
        var estado = _db.EstadoPrestamos.FirstOrDefault(e => e.Folio == folio);
        var datos = _db.DatosPrestamos.FirstOrDefault(d => d.Folio == folio);
        if (estado == null)
        {
            TempData["Error"] = "No se encontró el préstamo.";
            return RedirectToAction(nameof(Prestamos));
        }

        if (estado.Estado != 0)
        {
            TempData["Error"] = "Este préstamo ya fue revisado.";
            return RedirectToAction(nameof(Prestamos));
        }

        estado.Estado = 2;
        estado.MotivoRechazo = string.IsNullOrWhiteSpace(motivo) ? "Sin motivo" : motivo.Trim();
        if (datos != null)
        {
            _banca.Notificar(datos.SolicitadoPor, $"Tu préstamo folio {folio} fue rechazado: {estado.MotivoRechazo}");
        }

        _db.SaveChanges();
        tx.Commit();
        TempData["Ok"] = $"Préstamo {folio} rechazado.";
        return RedirectToAction(nameof(Prestamos));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
