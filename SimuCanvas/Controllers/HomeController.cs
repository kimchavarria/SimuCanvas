using Microsoft.AspNetCore.Mvc;
using SimuCanvas.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace SimuCanvas.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Authorize(Roles = "estudiante")]
        public IActionResult Estudiantes()
        {
            return View();
        }
        [Authorize(Roles = "profesor")]
        public IActionResult Profesor()
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
