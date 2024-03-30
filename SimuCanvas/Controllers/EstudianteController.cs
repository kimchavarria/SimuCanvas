using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimuCanvas.Data;

namespace SimuCanvas.Controllers
{
    public class EstudianteController : Controller
    {
        private readonly PerfilLogica _dbUsuario;
        private readonly EstudiantesLogica _estudiantesLogica;

        public EstudianteController(PerfilLogica dbUsuario, EstudiantesLogica estudiantesLogica)
        {
            _dbUsuario = dbUsuario;
            _estudiantesLogica = estudiantesLogica;
        }

        public IActionResult PerfilEstudiante()
        {
            // Obtener el usuario actualmente autenticado
            var usuario = ObtenerUsuarioActual();
            return View(usuario);
        }

        private Usuario ObtenerUsuarioActual()
        {
            // Obteniendo el usuario actualmente autenticado
            var claimsIdentity = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var nameClaim = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.Name);
            var emailClaim = claimsIdentity.FindFirst("Correo");

            // Consultar la base de datos para obtener el usuario completo
            var usuario = _dbUsuario.ObtenerUsuarioPorEmail(emailClaim.Value);

            return usuario;
        }

        public IActionResult CursosEstudiante()
        {
            // Obtener el usuario actualmente autenticado
            var usuario = ObtenerUsuarioActual();

            // Obtener los cursos en los que está matriculado el estudiante
            var cursosMatriculados = _estudiantesLogica.ObtenerCursosMatriculados(usuario.IdUsuario);

            ViewData["EstudiantesLogica"] = _estudiantesLogica;

            return View(cursosMatriculados);
        }

        public IActionResult DetalleCursos(int id)
        {
            // Aquí podrías implementar la lógica para obtener los detalles del curso con el id proporcionado
            var curso = _estudiantesLogica.ObtenerCursoPorId(id);
            ViewData["EstudiantesLogica"] = _estudiantesLogica;

            return View(curso);
        }


        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Login");
        }
    }
}
