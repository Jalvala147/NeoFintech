using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIndursa.AccountFunctions;
using ProyectoIndursa.Helpers;
using ProyectoIndursa.IndursaContext;
using ProyectoIndursa.Models;

namespace ProyectoIndursa.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IndursaDB _db;

    public HomeController(ILogger<HomeController> logger, IndursaDB db)
    {
        _logger = logger;
        _db = db;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectAfterLogin();
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var cuenta = _db.Cuenta.FirstOrDefault(s => s.NoCuenta == model.NoCuenta);
        if (cuenta == null || !PasswordHelper.Verify(model.Password, cuenta.Password))
        {
            ModelState.AddModelError(string.Empty, "Número de cuenta o contraseña incorrectos.");
            return View(model);
        }

        if (cuenta.TipoCuenta == 1)
        {
            ModelState.AddModelError(string.Empty, "Tu cuenta aún está pendiente de aprobación.");
            return View(model);
        }

        if (cuenta.TipoCuenta == 3)
        {
            var motivo = string.IsNullOrWhiteSpace(cuenta.MotivoRechazo)
                ? string.Empty
                : $" Motivo: {cuenta.MotivoRechazo}";
            ModelState.AddModelError(string.Empty, "Tu solicitud de cuenta fue rechazada." + motivo);
            return View(model);
        }

        if (cuenta.TipoCuenta != 2)
        {
            ModelState.AddModelError(string.Empty, "No puedes iniciar sesión con esta cuenta.");
            return View(model);
        }

        if (!PasswordHelper.IsHashed(cuenta.Password))
        {
            cuenta.Password = PasswordHelper.Hash(model.Password);
            _db.SaveChanges();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, cuenta.NoCuenta.ToString()),
            new(ClaimTypes.Role, "Usuario")
        };

        var empleado = _db.Empleados.FirstOrDefault(e => e.NoCuenta == cuenta.NoCuenta);
        if (empleado != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Empleado"));
            if (_db.Gerentes.Any(g => g.Nomina == empleado.Nomina))
            {
                claims.Add(new Claim(ClaimTypes.Role, "Gerente"));
            }
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        if (empleado != null)
        {
            return RedirectToAction("Index", "Employee");
        }

        return RedirectToAction("Index", "User");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.FechaDeNacimiento is null || model.FechaDeNacimiento.Value.Year < 1900)
        {
            ModelState.AddModelError(nameof(model.FechaDeNacimiento), "Ingresa una fecha de nacimiento válida.");
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

        var noCuenta = Account.CreateAccount(_db, user, model.Password);
        TempData["NoCuenta"] = noCuenta;
        return RedirectToAction(nameof(RegisterCompleted));
    }

    [HttpGet]
    public IActionResult RegisterCompleted()
    {
        if (TempData["NoCuenta"] is null)
        {
            return RedirectToAction(nameof(Register));
        }

        ViewBag.NoCuenta = TempData["NoCuenta"];
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Recuperar()
    {
        return View(new RecuperarPasswordViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult Recuperar(RecuperarPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = _db.Usuarios.FirstOrDefault(u =>
            u.NoCuenta == model.NoCuenta &&
            u.Curp.ToUpper() == model.Curp.Trim().ToUpper());
        if (usuario == null ||
            model.FechaDeNacimiento is null ||
            usuario.FechaDeNacimiento.Date != model.FechaDeNacimiento.Value.Date)
        {
            ModelState.AddModelError(string.Empty, "Los datos no coinciden con ninguna cuenta.");
            return View(model);
        }

        var cuenta = _db.Cuenta.FirstOrDefault(c => c.NoCuenta == model.NoCuenta);
        if (cuenta == null || cuenta.TipoCuenta != 2)
        {
            ModelState.AddModelError(string.Empty, "La cuenta no está activa.");
            return View(model);
        }

        cuenta.Password = PasswordHelper.Hash(model.Nueva);
        _db.SaveChanges();
        TempData["Ok"] = "Contraseña restablecida. Ya puedes iniciar sesión.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private IActionResult RedirectAfterLogin()
    {
        if (User.IsInRole("Empleado") || User.IsInRole("Gerente"))
        {
            return RedirectToAction("Index", "Employee");
        }

        return RedirectToAction("Index", "User");
    }
}
