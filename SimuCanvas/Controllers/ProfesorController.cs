using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimuCanvas.Data;
using SimuCanvas.Models;

namespace SimuCanvas.Controllers
{
    public class ProfesorController : Controller
    {
        private readonly PerfilLogica _dbUsuario;
        private readonly EstudiantesLogica _estudiantesLogica;

        public ProfesorController(PerfilLogica dbUsuario, EstudiantesLogica estudiantesLogica)
        {
            _dbUsuario = dbUsuario;
            _estudiantesLogica = estudiantesLogica;
        }

        [HttpGet]
        public IActionResult PerfilProfesor()
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

        public IActionResult CursosProfesor()
        {
            return View();
        }
        public IActionResult DetalleCursos()
        {
            return View();
        }

        public IActionResult AsistenciaProfesor()
        {
            return View();
        }

        public IActionResult AsignaturasProfesor()
        {
            return View();
        }

        public IActionResult GruposProfesor()
        {
            return View();
        }

        public IActionResult AdminEstudiantes()
        {
            var estudiantes = _estudiantesLogica.ObtenerUsuariosPorRol("Estudiante");
            return View(estudiantes);
        }

        [HttpPost]
        public IActionResult RegistrarEstudianteACurso(int student_id, int course_id)
        {
            // Obtener información del estudiante y curso
            var student = _estudiantesLogica.ObtenerUsuarioPorId(student_id);
            var course = _estudiantesLogica.ObtenerCursoPorId(course_id);

            if (student == null)
            {
                return BadRequest("El estudiante no existe.");
            }

            if (course == null)
            {
                return BadRequest("El curso no existe.");
            }

            // Realizar el registro del estudiante al curso
            bool registroExitoso = _estudiantesLogica.RegistrarEstudianteACurso(student_id, course_id);

            if (registroExitoso)
            {
                return Ok("Estudiante registrado al curso exitosamente.");
            }
            else
            {
                return BadRequest("El estudiante ya está registrado en este curso.");
            }
        }
        [HttpGet]
        public IActionResult ObtenerEstudiantesRegistrados()
        {
            var estudiantesRegistrados = _estudiantesLogica.ObtenerEstudiantesRegistrados();
            return Json(estudiantesRegistrados);
        }

        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Login");
        }
    }
}