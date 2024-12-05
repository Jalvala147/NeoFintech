using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProyectoIndursa.Models;
using ProyectoIndursa.IndursaContext;
using ProyectoIndursa.AccountFunctions;

namespace ProyectoIndursa.Controllers
{
public class HomeController : Controller
{
    Random clave=new Random();
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    public IActionResult Register()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult RegisterCompleted(string Curp,string Nombre,string ApellidoPaterno,string ApellidoMaterno,DateTime FechaDeNacimiento)
    {
        if(ModelState.IsValid)
        {
        Usuario user=new Usuario
        {
            Curp=Curp,
            Nombre=Nombre,
            ApellidoPaterno=ApellidoPaterno,
            ApellidoMaterno=ApellidoMaterno,
            FechaDeNacimiento=FechaDeNacimiento
        };
        Account.CreateAccount(user);
        return View();
        }
        else
        {
            return View("Register");
        }
    }
    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
}