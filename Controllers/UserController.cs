using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIndursa.Helpers;
using ProyectoIndursa.IndursaContext;
using ProyectoIndursa.Models;

namespace ProyectoIndursa.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly ILogger<UserController> _logger;
    private readonly IndursaDB _db;

    public UserController(ILogger<UserController> logger, IndursaDB db)
    {
        _logger = logger;
        _db = db;
    }

    public IActionResult Index()
    {
        var noCuenta = UserAccount.GetNoCuenta(User);
        if (noCuenta is null)
        {
            return RedirectToAction("Login", "Home");
        }

        var usuario = _db.Usuarios.FirstOrDefault(u => u.NoCuenta == noCuenta);
        ViewBag.Nombre = usuario?.Nombre ?? "cliente";
        ViewBag.NoCuenta = noCuenta;
        return View();
    }

    public IActionResult Historial()
    {
        var noCuenta = UserAccount.GetNoCuenta(User);
        if (noCuenta is null)
        {
            return RedirectToAction("Login", "Home");
        }

        return View(ObtenerPrestamos(noCuenta.Value));
    }

    public IActionResult Saldo()
    {
        var noCuenta = UserAccount.GetNoCuenta(User);
        if (noCuenta is null)
        {
            return RedirectToAction("Login", "Home");
        }

        var info = _db.InfoCuenta.FirstOrDefault(s => s.NoCuenta == noCuenta);
        ViewBag.Saldo = info?.Saldo ?? 0;
        ViewBag.NoCuenta = noCuenta;
        return View();
    }

    public IActionResult Prestamo()
    {
        var noCuenta = UserAccount.GetNoCuenta(User);
        if (noCuenta is null)
        {
            return RedirectToAction("Login", "Home");
        }

        var saldo = _db.InfoCuenta.FirstOrDefault(s => s.NoCuenta == noCuenta)?.Saldo ?? 0;
        var prestamos = ObtenerPrestamos(noCuenta.Value);
        var tieneActivo = prestamos.Any(p => p.Estado is 0 or 1);

        ViewBag.Saldo = saldo;
        ViewBag.PuedeSolicitar = saldo >= 10000 && !tieneActivo;
        ViewBag.Mensaje = saldo < 10000
            ? "No cuentas con el saldo suficiente para solicitar un préstamo (mínimo $10,000)."
            : tieneActivo
                ? "Ya tienes un préstamo pendiente o activo."
                : null;

        return View(prestamos);
    }

    [HttpGet]
    public IActionResult SolicitarPrestamo()
    {
        var noCuenta = UserAccount.GetNoCuenta(User);
        if (noCuenta is null)
        {
            return RedirectToAction("Login", "Home");
        }

        if (!PuedeSolicitar(noCuenta.Value, out var mensaje))
        {
            TempData["Error"] = mensaje;
            return RedirectToAction(nameof(Prestamo));
        }

        return View(new SolicitarPrestamoViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SolicitarPrestamo(SolicitarPrestamoViewModel model)
    {
        var noCuenta = UserAccount.GetNoCuenta(User);
        if (noCuenta is null)
        {
            return RedirectToAction("Login", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!PuedeSolicitar(noCuenta.Value, out var mensaje))
        {
            TempData["Error"] = mensaje;
            return RedirectToAction(nameof(Prestamo));
        }

        var folio = _db.Prestamos.Select(p => p.Folio).DefaultIfEmpty(100000).Max() + 1;
        _db.Prestamos.Add(new Prestamo
        {
            Folio = folio,
            Cantidad = model.Cantidad
        });
        _db.DatosPrestamos.Add(new DatosPrestamo
        {
            FechaExpedicion = DateTime.Now,
            FechaAprobacion = DateTime.MinValue,
            FechaLiquidacion = DateTime.MinValue,
            FechaLimite = DateTime.Now.AddMonths(12),
            Folio = folio,
            SolicitadoPor = noCuenta.Value
        });
        _db.EstadoPrestamos.Add(new EstadoPrestamo
        {
            Folio = folio,
            PagoRealizados = 0,
            PagoPedientes = 12,
            Estado = 0
        });
        _db.SaveChanges();

        TempData["Ok"] = $"Solicitud enviada. Folio {folio}.";
        return RedirectToAction(nameof(Prestamo));
    }

    public IActionResult Perfil()
    {
        var noCuenta = UserAccount.GetNoCuenta(User);
        if (noCuenta is null)
        {
            return RedirectToAction("Login", "Home");
        }

        var usuario = _db.Usuarios.FirstOrDefault(u => u.NoCuenta == noCuenta);
        var cuenta = _db.Cuenta.FirstOrDefault(c => c.NoCuenta == noCuenta);
        var info = _db.InfoCuenta.FirstOrDefault(s => s.NoCuenta == noCuenta);

        ViewBag.Usuario = usuario;
        ViewBag.TipoCuenta = cuenta?.TipoCuenta ?? 0;
        ViewBag.Saldo = info?.Saldo ?? 0;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> SingOut() => SignOut();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private bool PuedeSolicitar(int noCuenta, out string? mensaje)
    {
        var saldo = _db.InfoCuenta.FirstOrDefault(s => s.NoCuenta == noCuenta)?.Saldo ?? 0;
        if (saldo < 10000)
        {
            mensaje = "No cuentas con el saldo suficiente para solicitar un préstamo (mínimo $10,000).";
            return false;
        }

        if (ObtenerPrestamos(noCuenta).Any(p => p.Estado is 0 or 1))
        {
            mensaje = "Ya tienes un préstamo pendiente o activo.";
            return false;
        }

        mensaje = null;
        return true;
    }

    private List<PrestamoListaItem> ObtenerPrestamos(int noCuenta)
    {
        var datos = _db.DatosPrestamos.Where(d => d.SolicitadoPor == noCuenta).ToList();
        var folios = datos.Select(d => d.Folio).ToList();
        var prestamos = _db.Prestamos.Where(p => folios.Contains(p.Folio)).ToList();
        var estados = _db.EstadoPrestamos.Where(e => folios.Contains(e.Folio)).ToList();

        return datos.Select(d =>
        {
            var prestamo = prestamos.FirstOrDefault(p => p.Folio == d.Folio);
            var estado = estados.FirstOrDefault(e => e.Folio == d.Folio);
            return new PrestamoListaItem
            {
                Folio = d.Folio,
                Cantidad = prestamo?.Cantidad ?? 0,
                FechaExpedicion = d.FechaExpedicion,
                PagoRealizados = estado?.PagoRealizados ?? 0,
                PagoPedientes = estado?.PagoPedientes ?? 0,
                Estado = estado?.Estado ?? 0,
                SolicitadoPor = d.SolicitadoPor
            };
        }).ToList();
    }
}
