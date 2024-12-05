using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIndursa.Models;
using ProyectoIndursa.IndursaContext;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace ProyectoIndursa.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        public IActionResult Historial()
        {
            return View();
        }
        public IActionResult Saldo()
        {
            string ncuenta=this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var db=new IndursaDB();
            var saldo=from s in db.InfoCuenta where s.NoCuenta==Int32.Parse(ncuenta) select s.Saldo;
            ViewBag.Saldo=saldo.First();
            return View();
        }
        public IActionResult Prestamo()
        {
            return View();
        }
        [HttpGet]
        public IActionResult SolicitarPrestamo()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Index(int nocuenta, string password)
        {
            var user = new Cuentum
            {
                NoCuenta = nocuenta,
                Password = password
            };
            var db = new IndursaDB();
            var exist = from s in db.Cuenta where s.NoCuenta == user.NoCuenta select s.NoCuenta;
            if (exist.Count() > 0)
            {
            var passcheck = db.Cuenta.First(s => s.NoCuenta == user.NoCuenta);
                if(passcheck.TipoCuenta==2)
                {
                    if (passcheck.Password != password)
                    {
                        return View("Index");

                    }
                }
                else
                {
                    return View("Index");
                }
            }
            else
            {
                return View("Index");
            }
            LoginUser(user);
            return View();
        }
        public async Task<IActionResult> LoginUser(Cuentum user)
        {

            var claims = new List<Claim>{
                new Claim(ClaimTypes.NameIdentifier as string,user.NoCuenta.ToString()),
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            return Redirect("/");
        }
        public async Task<IActionResult> SingOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login","Home");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
