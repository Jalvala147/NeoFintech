using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProyectoIndursa.Models;
using ProyectoIndursa.IndursaContext;
using System.Linq;

namespace ProyectoIndursa.Controllers
{
public class EmployeeController : Controller
{
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(ILogger<EmployeeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
    public IActionResult Cuentas()
    {
        return View();
    }
    public IActionResult Prestamos()
    {
        return View();
    }
     public ActionResult AcceptAccount(string button)
    {
        
        int id=Int32.Parse(button);
        using(var db=new IndursaDB())
        {
            var updateStatus=db.Cuenta.First(s=>s.NoCuenta==id);
            updateStatus.TipoCuenta=2;
            db.SaveChanges();
        }
        return View("~/Views/Employee/Cuentas.cshtml");
    }
    public ActionResult RejectAccount(string button)
    {
        
        int id=Int32.Parse(button);
        using(var db=new IndursaDB())
        {
            var updateStatus=db.Cuenta.First(s=>s.NoCuenta==id);
            updateStatus.TipoCuenta=3;
            db.SaveChanges();
        }
        return View("~/Views/Employee/Cuentas.cshtml");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
}