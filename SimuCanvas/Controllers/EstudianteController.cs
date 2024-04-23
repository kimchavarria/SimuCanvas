using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimuCanvas.Data;
using SimuCanvas.Models;
using SimuCanvas.Logic;

namespace SimuCanvas.Controllers
{
    public class EstudianteController : Controller
    {
        private readonly PerfilLogica _dbUsuario;
        private readonly EstudiantesLogica _estudiantesLogica;
        private readonly ProfesorLogica _profesorLogica;

        public EstudianteController(PerfilLogica dbUsuario, EstudiantesLogica estudiantesLogica, ProfesorLogica profesorLogica)
        {
            _dbUsuario = dbUsuario;
            _estudiantesLogica = estudiantesLogica;
            _profesorLogica = profesorLogica;
        }

        public IActionResult PerfilEstudiante()
        {
            // obtenemos el usuario actualmente autenticado
            var usuario = ObtenerUsuarioActual();
            return View(usuario);
        }

        private Usuario ObtenerUsuarioActual()
        {
            // obtenemos el usuario actualmente autenticado
            var claimsIdentity = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var nameClaim = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.Name);
            var emailClaim = claimsIdentity.FindFirst("Correo");

            // se consulta la base de datos para obtener el usuario completo
            var usuario = _dbUsuario.ObtenerUsuarioPorEmail(emailClaim.Value);

            return usuario;
        }

        public IActionResult CursosEstudiante()
        {
            // obtiene el usuario actualmente autenticado
            var usuario = ObtenerUsuarioActual();

            // obtiene los cursos en los que está matriculado el estudiante
            var cursosMatriculados = _estudiantesLogica.ObtenerCursosMatriculados(usuario.IdUsuario);

            ViewData["EstudiantesLogica"] = _estudiantesLogica;

            return View(cursosMatriculados);
        }

        public IActionResult DetalleCursos(int id)
        {
            var curso = _estudiantesLogica.ObtenerCursoPorId(id);
            ViewData["EstudiantesLogica"] = _estudiantesLogica;

            return View(curso);
        }
        public IActionResult AsistenciaEstudiante(int courseId)
        {
            // Get student ID from current logged in user
            var studentId = ObtenerUsuarioActual().IdUsuario;

            var attendanceRecords = _estudiantesLogica.ObtenerAsistenciaEstudianteEnCurso(studentId, courseId);

            return View(attendanceRecords);
        }
        public IActionResult AsignaturasEstudiante()
        {

            var assignments = _profesorLogica.GetAllAssignments(); // Suponiendo que tienes un método para obtener todas las asignaciones

            return View(assignments);
        }

        public IActionResult AssignmentDetails(int assignmentId)
        {
            // Retrieve assignment details
            Assignment assignment = _estudiantesLogica.GetAssignmentDetails(assignmentId);
            if (assignment == null)
            {
                return NotFound();
            }

            // Display assignment details view
            return View(assignment);
        }

       

        public IActionResult GruposEstudiante()
        {
            var usuario = ObtenerUsuarioActual();
            var gruposEstudiante = _estudiantesLogica.ObtenerGruposEstudiante(usuario.IdUsuario);
            return View(gruposEstudiante);
        }



        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Login");
        }
    }
}
