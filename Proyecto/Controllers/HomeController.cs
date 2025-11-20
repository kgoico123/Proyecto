using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Proyecto.Models;
using Proyecto.shared;

namespace Proyecto.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(VCG.Role_Admin))
                    return RedirectToAction("Dashboard", "Administrador");
                if (User.IsInRole(VCG.Role_Docente))
                    return RedirectToAction("Dashboard", "Docente");
                if (User.IsInRole(VCG.Role_Tutor))
                    return RedirectToAction("Dashboard", "Tutor");

                return RedirectToAction("Dashboard", "Estudiante");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
