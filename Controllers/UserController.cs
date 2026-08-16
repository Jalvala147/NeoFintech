using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIndursa.Helpers;
using ProyectoIndursa.IndursaContext;
using ProyectoIndursa.Models;
using ProyectoIndursa.Services;

namespace ProyectoIndursa.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly ILogger<UserController> _logger;
    private readonly IndursaDB _db;
    private readonly BancaService _banca;

    public UserController(ILogger<UserController> logger, IndursaDB db, BancaService banca)
    {
        _logger = logger;
        _db = db;
        _banca = banca;
    }

    public IActionResult Index()
    {
        if (!TryCuenta(out var noCuenta))
        {
            return RedirectToAction("Login", "Home");
        }

        var usuario = _db.Usuarios.FirstOrDefault(u => u.NoCuenta == noCuenta);
        var notificaciones = _db.Notificaciones
            .Where(n => n.NoCuenta == noCuenta && !n.Leida)
            .OrderByDescending(n => n.Fecha)
            .ToList();
        foreach (var item in notificaciones)
        {
            item.Leida = true;
        }

        if (notificaciones.Count > 0)
        {
            _db.SaveChanges();
        }

        ViewBag.Nombre = usuario?.Nombre ?? "cliente";
        ViewBag.NoCuenta = noCuenta;
        ViewBag.Saldo = _db.InfoCuenta.FirstOrDefault(s => s.NoCuenta == noCuenta)?.Saldo ?? 0;
        return View(notificaciones);
    }

    public IActionResult Historial()
    {
        if (!TryCuenta(out var noCuenta))
        {
            return RedirectToAction("Login", "Home");
        }

        return View(ObtenerPrestamos(noCuenta));
    }

    public IActionResult Movimientos()
    {
        if (!TryCuenta(out var noCuenta))
        {
            return RedirectToAction("Login", "Home");
        }

        var lista = _db.Movimientos
            .Where(m => m.NoCuenta == noCuenta)
            .OrderByDescending(m => m.Fecha)
            .ThenByDescending(m => m.Id)
            .ToList();
        return View(lista);
    }

    public IActionResult Saldo()
    {
        if (!TryCuenta(out var noCuenta))
        {
            return RedirectToAction("Login", "Home");
        }

        ViewBag.Saldo = _db.InfoCuenta.FirstOrDefault(s => s.NoCuenta == noCuenta)?.Saldo ?? 0;
        ViewBag.NoCuenta = noCuenta;
        return View();
    }

    [HttpGet]
    public IActionResult Depositar() => View(new OperacionMontoViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Depositar(OperacionMontoViewModel model)
    {
        if (!TryCuenta(out var noCuenta))
        {
            return RedirectToAction("Login", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var resultado = _banca.Depositar(noCuenta, model.Monto, "Depósito en ventanilla digital");
        return ResultadoOperacion(resultado, nameof(Depositar), model);
    }

    [HttpGet]
    public IActionResult Retirar() => View(new OperacionMontoViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Retirar(OperacionMontoViewModel model)
    {
        if (!TryCuenta(out var noCuenta))
        {
            return RedirectToAction("Login", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var resultado = _banca.Retirar(noCuenta, model.Monto, "Retiro en ventanilla digital");
        return ResultadoOperacion(resultado, nameof(Retirar), model);
    }

    [HttpGet]
    public IActionResult Transferir() => View(new TransferirViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Transferir(TransferirViewModel model)
    {
        if (!TryCuenta(out var noCuenta))
        {
            return RedirectToAction("Login", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var resultado = _banca.Transferir(noCuenta, model.CuentaDestino, model.Monto, model.Concepto);
        return ResultadoOperacion(resultado, nameof(Transferir), model);
    }

    public IActionResult Prestamo()
    {
        if (!TryCuenta(out var noCuenta))
        {
            return RedirectToAction("Login", "Home");
        }

        var saldo = _db.InfoCuenta.FirstOrDefault(s => s.NoCuenta == noCuenta)?.Saldo ?? 0;
        var prestamos = ObtenerPrestamos(noCuenta);
        var tieneActivo = prestamos.Any(p => p.Estado is 0 or 1);
        ViewBag.Saldo = saldo;
        ViewBag.Maximo = Banca.MaximoPrestamo(saldo);
        ViewBag.PuedeSolicitar = saldo >= Banca.PrestamoSaldoMinimo && !tieneActivo;
        ViewBag.Mensaje = saldo < Banca.PrestamoSaldoMinimo
            ? $"No cuentas con el saldo suficiente para solicitar un préstamo (mínimo ${Banca.PrestamoSaldoMinimo:N2})."
            : tieneActivo
                ? "Ya tienes un préstamo pendiente o activo."
                : null;
        return View(prestamos);
    }

    [HttpGet]
    public IActionResult SolicitarPrestamo()
    {
        if (!TryCuenta(out var noCuenta))
        {
            return RedirectToAction("Login", "Home");
        }

        if (!PuedeSolicitar(noCuenta, out var mensaje, out var maximo))
        {
            TempData["Error"] = mensaje;
            return RedirectToAction(nameof(Prestamo));
        }

        ViewBag.Maximo = maximo;
        return View(new SolicitarPrestamoViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SolicitarPrestamo(SolicitarPrestamoViewModel model)
    {
        if (!TryCuenta(out var noCuenta))
        {
            return RedirectToAction("Login", "Home");
        }

        if (!PuedeSolicitar(noCuenta, out var mensaje, out var maximo))
        {
            TempData["Error"] = mensaje;
            return RedirectToAction(nameof(Prestamo));
        }

        if (model.Cantidad < Banca.PrestamoMinimo || model.Cantidad > maximo)
        {
            ModelState.AddModelError(nameof(model.Cantidad), $"El monto debe estar entre ${Banca.PrestamoMinimo:N2} y ${maximo:N2}.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Maximo = maximo;
            return View(model);
        }

        var folio = _db.Prestamos.Select(p => p.Folio).ToList().DefaultIfEmpty(100000).Max() + 1;
        _db.Prestamos.Add(new Prestamo { Folio = folio, Cantidad = model.Cantidad });
        _db.DatosPrestamos.Add(new DatosPrestamo
        {
            FechaExpedicion = DateTime.Now,
            FechaAprobacion = DateTime.MinValue,
            FechaLiquidacion = DateTime.MinValue,
            FechaLimite = DateTime.Now.AddMonths(Banca.Plazos),
            Folio = folio,
            SolicitadoPor = noCuenta
        });
        _db.EstadoPrestamos.Add(new EstadoPrestamo
        {
            Folio = folio,
            PagoRealizados = 0,
            PagoPedientes = Banca.Plazos,
            Estado = 0
        });
        _db.SaveChanges();
        TempData["Ok"] = $"Solicitud enviada. Folio {folio}.";
        return RedirectToAction(nameof(Prestamo));
    }

    public IActionResult DetallePrestamo(int folio)
    {
        if (!TryCuenta(out var noCuenta))
        {
            return RedirectToAction("Login", "Home");
        }

        var item = ObtenerPrestamos(noCuenta).FirstOrDefault(p => p.Folio == folio);
        if (item == null)
        {
            TempData["Error"] = "No se encontró el préstamo.";
            return RedirectToAction(nameof(Prestamo));
        }

        ViewBag.Prestamo = item;
        return View(_banca.Amortizacion(item.Cantidad, item.PagoRealizados));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PagarPrestamo(int folio)
    {
        if (!TryCuenta(out var noCuenta))
        {
            return RedirectToAction("Login", "Home");
        }

        var resultado = _banca.PagarPrestamo(noCuenta, folio);
        if (!resultado.Ok)
        {
            TempData["Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Prestamo));
        }

        return View("Comprobante", resultado.Comprobante);
    }

    public IActionResult Perfil()
    {
        if (!TryCuenta(out var noCuenta))
        {
            return RedirectToAction("Login", "Home");
        }

        var usuario = _db.Usuarios.FirstOrDefault(u => u.NoCuenta == noCuenta);
        ViewBag.Usuario = usuario;
        ViewBag.Saldo = _db.InfoCuenta.FirstOrDefault(s => s.NoCuenta == noCuenta)?.Saldo ?? 0;
        ViewBag.Boletos = _db.Rifas.Where(r => r.Cuenta == noCuenta).OrderByDescending(r => r.NoBoleto).ToList();
        return View();
    }

    [HttpGet]
    public IActionResult EditarPerfil()
    {
        if (!TryCuenta(out var noCuenta))
        {
            return RedirectToAction("Login", "Home");
        }

        var usuario = _db.Usuarios.FirstOrDefault(u => u.NoCuenta == noCuenta);
        if (usuario == null)
        {
            return RedirectToAction(nameof(Perfil));
        }

        return View(new EditarPerfilViewModel
        {
            Nombre = usuario.Nombre,
            ApellidoPaterno = usuario.ApellidoPaterno,
            ApellidoMaterno = usuario.ApellidoMaterno
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditarPerfil(EditarPerfilViewModel model)
    {
        if (!TryCuenta(out var noCuenta))
        {
            return RedirectToAction("Login", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = _db.Usuarios.FirstOrDefault(u => u.NoCuenta == noCuenta);
        if (usuario == null)
        {
            return RedirectToAction(nameof(Perfil));
        }

        usuario.Nombre = model.Nombre.Trim();
        usuario.ApellidoPaterno = model.ApellidoPaterno.Trim();
        usuario.ApellidoMaterno = model.ApellidoMaterno.Trim();
        _db.SaveChanges();
        TempData["Ok"] = "Perfil actualizado.";
        return RedirectToAction(nameof(Perfil));
    }

    [HttpGet]
    public IActionResult CambiarPassword() => View(new CambiarPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CambiarPassword(CambiarPasswordViewModel model)
    {
        if (!TryCuenta(out var noCuenta))
        {
            return RedirectToAction("Login", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var cuenta = _db.Cuenta.FirstOrDefault(c => c.NoCuenta == noCuenta);
        if (cuenta == null || !PasswordHelper.Verify(model.Actual, cuenta.Password))
        {
            ModelState.AddModelError(nameof(model.Actual), "La contraseña actual no es correcta.");
            return View(model);
        }

        cuenta.Password = PasswordHelper.Hash(model.Nueva);
        _db.SaveChanges();
        TempData["Ok"] = "Contraseña actualizada.";
        return RedirectToAction(nameof(Perfil));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> SingOut() => Logout();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private IActionResult ResultadoOperacion(OperacionResultado resultado, string vista, object model)
    {
        if (!resultado.Ok)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(vista, model);
        }

        return View("Comprobante", resultado.Comprobante);
    }

    private bool TryCuenta(out int noCuenta)
    {
        var value = UserAccount.GetNoCuenta(User);
        noCuenta = value ?? 0;
        return value is not null;
    }

    private bool PuedeSolicitar(int noCuenta, out string? mensaje, out decimal maximo)
    {
        var saldo = _db.InfoCuenta.FirstOrDefault(s => s.NoCuenta == noCuenta)?.Saldo ?? 0;
        maximo = Banca.MaximoPrestamo(saldo);
        if (saldo < Banca.PrestamoSaldoMinimo)
        {
            mensaje = $"No cuentas con el saldo suficiente para solicitar un préstamo (mínimo ${Banca.PrestamoSaldoMinimo:N2}).";
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
            var cantidad = prestamo?.Cantidad ?? 0;
            return new PrestamoListaItem
            {
                Folio = d.Folio,
                Cantidad = cantidad,
                FechaExpedicion = d.FechaExpedicion,
                FechaLimite = d.FechaLimite,
                PagoRealizados = estado?.PagoRealizados ?? 0,
                PagoPedientes = estado?.PagoPedientes ?? 0,
                Estado = estado?.Estado ?? 0,
                SolicitadoPor = d.SolicitadoPor,
                Mensualidad = Banca.Mensualidad(cantidad),
                Vencido = estado?.Estado == 1 && DateTime.Now.Date > d.FechaLimite.Date,
                MotivoRechazo = estado?.MotivoRechazo
            };
        }).ToList();
    }
}
