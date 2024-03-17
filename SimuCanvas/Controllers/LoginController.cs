using Microsoft.AspNetCore.Mvc;
using SimuCanvas.Models;
using SimuCanvas.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SimuCanvas.Controllers
{
    public class LoginController : Controller
    {
        private readonly LoginLogica _dbUsuario;

        public LoginController(LoginLogica dbUsuario)
        {
            _dbUsuario = dbUsuario;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Usuario _usuario)
        {
            if (!ModelState.IsValid)
            {
                return View(_usuario);
            }

            var usuario = _dbUsuario.ValidarUsuario(_usuario.Email, _usuario.Password);

            if (usuario != null)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Name),
                new Claim("Correo", usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Role) // Añadido directamente el rol como reclamación
            };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                if (usuario.Role.Contains("estudiante"))
                {
                    return RedirectToAction("Estudiantes", "Home");
                }
                else if (usuario.Role.Contains("profesor"))
                {
                    return RedirectToAction("Profesor", "Home");
                }
                else
                {
                    return RedirectToAction("Login", "Home");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Correo electrónico o contraseña incorrectos.");
                return View(_usuario);
            }
        }

        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Login");
        }
    }
}