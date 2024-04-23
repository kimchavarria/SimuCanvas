using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimuCanvas.Data;
using SimuCanvas.Logic;
using SimuCanvas.Models;
using System.Text.RegularExpressions;
using System.Data;

namespace SimuCanvas.Controllers
{
    public class ProfesorController : Controller
    {
        private readonly PerfilLogica _dbUsuario;
        private readonly EstudiantesLogica _estudiantesLogica;
        private readonly ProfesorLogica _profesorLogica;

        public ProfesorController(PerfilLogica dbUsuario, EstudiantesLogica estudiantesLogica, ProfesorLogica profesorLogica)
        {
            _dbUsuario = dbUsuario;
            _estudiantesLogica = estudiantesLogica;
            _profesorLogica = profesorLogica;
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
            var usuario = _profesorLogica.ObtenerUsuarioActual(HttpContext);
            var cursos = _profesorLogica.ObtenerCursosPorProfesor(usuario.IdUsuario);
            ViewBag.Cursos = cursos;

            var professorName = _profesorLogica.ObtenerNombreProfesor(usuario.IdUsuario);

            ViewBag.ProfessorName = professorName;

            var cursosConEstudiantes = new List<(Course curso, int totalEstudiantes)>();

            foreach (var curso in cursos)
            {
                int totalEstudiantes = _profesorLogica.ObtenerNumeroEstudiantesRegistradosEnCurso(curso.CourseId);
                cursosConEstudiantes.Add((curso, totalEstudiantes));
            }

            ViewBag.CursosConEstudiantes = cursosConEstudiantes;

            return View();
        }

        public IActionResult DetalleCursos(int courseId)
        {
            var curso = _profesorLogica.ObtenerCursoPorId(courseId);
            if (curso == null)
            {
                return NotFound();
            }
            return View(curso);
        }

        public IActionResult AsistenciaProfesor(int courseId)
        {
            var estudiantes = _profesorLogica.ObtenerEstudiantesRegistradosEnCurso(courseId);
            return View(estudiantes);
        }

        [HttpPost]
        public IActionResult AsistenciaProfesor(int courseId, Dictionary<int, bool> Estudiantes)
        {
            ProfesorLogica profesorLogica = new ProfesorLogica(_dbUsuario);

            foreach (var kvp in Estudiantes)
            {
                int studentId = kvp.Key;
                bool isPresent = kvp.Value;

                profesorLogica.InsertAttendance(studentId, courseId, DateTime.Today, isPresent);
            }
            TempData["AttendanceSaved"] = true;
            return RedirectToAction("AsistenciaProfesor", new { courseId = courseId });
        }

        public IActionResult AsignaturasProfesor(int idCurso)
        {
            var assignments = _profesorLogica.GetAllAssignments();
            ViewBag.Assignments = assignments;
            ViewBag.CourseId = idCurso;
            return View();
        }

        [HttpGet]
        public IActionResult CrearAsignacion(int idCurso)
        {
            ViewBag.CourseId = idCurso;
            return View();
        }

        [HttpPost]
        public IActionResult CrearAsignacion(Assignment assignment)
        {
            try
            {
                _profesorLogica.CreateAssignment(assignment.CourseId, assignment.Title, assignment.Description, assignment.DueDate);
                TempData["Message"] = "Assignment created successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to create assignment: " + ex.Message;
            }

            return RedirectToAction("AsignaturasProfesor", new { idCurso = assignment.CourseId });
        }



        public IActionResult GruposProfesor(int idCurso)
        {
            var groups = _profesorLogica.GetGroupsByCourseId(idCurso);
            var students = _profesorLogica.ObtenerEstudiantesRegistradosEnCurso(idCurso);
            ViewBag.CourseId = idCurso;
            ViewBag.Students = students;
            return View(groups);
        }

        [HttpPost]
        public IActionResult CrearGrupo(string groupName, int idCurso)
        {
            try
            {
                _profesorLogica.CreateGroup(idCurso, groupName);
                TempData["Message"] = "Group created successfully!";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("GruposProfesor", new { idCurso = idCurso });
        }


        [HttpPost]
        public IActionResult AsignarEstudianteAGrupo(int studentId, int groupId, int courseId)
        {
            bool success = _profesorLogica.AssignStudentToGroup(studentId, groupId);
            if (success)
            {
                TempData["Message"] = "Student assigned to group successfully!";
                return Json(new { success = true });
            }
            else
            {
                TempData["Error"] = "Failed to assign student to group.";
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public IActionResult RemoverEstudianteDeGrupo(int studentId, int groupId, int courseId)
        {
            bool success = _profesorLogica.RemoveStudentFromGroup(studentId, groupId);
            if (success)
            {
                TempData["Message"] = "Student removed from group successfully!";
                return Json(new { success = true });
            }
            else
            {
                TempData["Error"] = "Failed to remove student from group.";
                return Json(new { success = false });
            }
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