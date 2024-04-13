using SimuCanvas.Data;
using SimuCanvas.Models;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace SimuCanvas.Logic
{
    public class ProfesorLogica
    {
        private string _connectionString = "Data Source=KIMBERLYSLAPTOP\\SQLEXPRESS01;Initial Catalog=SimCanvas;Integrated Security=True;";

        private readonly PerfilLogica _dbUsuario;

        public ProfesorLogica(PerfilLogica dbUsuario)
        {
            _dbUsuario = dbUsuario;
        }

        public Usuario ObtenerUsuarioActual(HttpContext httpContext)
        {
            var claimsIdentity = (ClaimsIdentity)httpContext.User.Identity;
            var nameClaim = claimsIdentity.FindFirst(ClaimTypes.Name);
            var emailClaim = claimsIdentity.FindFirst("Correo");

            var usuario = _dbUsuario.ObtenerUsuarioPorEmail(emailClaim.Value);

            return usuario;
        }

        public List<Course> ObtenerCursosPorProfesor(int profesorId)
        {
            List<Course> cursos = new List<Course>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"SELECT course_id, faculty_id, Title, Description, Credits, InitialDate, FinalDate, MaxStudents 
                         FROM COURSE 
                         WHERE faculty_id = @profesorId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@profesorId", profesorId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var curso = new Course
                        {
                            CourseId = reader.GetInt32(0),
                            FacultyId = reader.GetInt32(1),
                            Title = reader.GetString(2),
                            Description = reader.GetString(3),
                            Credits = reader.GetInt32(4),
                            InitialDate = reader.GetDateTime(5),
                            FinalDate = reader.GetDateTime(6),
                            MaxStudents = reader.GetInt32(7)
                        };

                        cursos.Add(curso);
                    }
                }
            }

            return cursos;
        }

        public string ObtenerNombreProfesor(int facultyId)
        {
            string nombreProfesor = "";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT name FROM USUARIOS WHERE id_usuario = @facultyId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@facultyId", facultyId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        nombreProfesor = reader.GetString(0);
                    }
                }
            }

            return nombreProfesor;
        }

        public int ObtenerTotalEstudiantesPorCurso(int courseId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM REGISTRO WHERE course_id = @courseId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@courseId", courseId);

                int count = (int)command.ExecuteScalar();

                return count;
            }
        }

        public int ObtenerNumeroEstudiantesRegistradosEnCurso(int courseId)
        {
            int count = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM REGISTRO WHERE course_id = @courseId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@courseId", courseId);

                count = (int)command.ExecuteScalar();
            }

            return count;
        }

        public Course ObtenerCursoPorId(int courseId)
        {
            Course curso = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT course_id, faculty_id, Title, Description, Credits, InitialDate, FinalDate, MaxStudents " +
                               "FROM COURSE " +
                               "WHERE course_id = @courseId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@courseId", courseId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        curso = new Course
                        {
                            CourseId = reader.GetInt32(0),
                            FacultyId = reader.GetInt32(1),
                            Title = reader.GetString(2),
                            Description = reader.GetString(3),
                            Credits = reader.GetInt32(4),
                            InitialDate = reader.GetDateTime(5),
                            FinalDate = reader.GetDateTime(6),
                            MaxStudents = reader.GetInt32(7)
                        };
                    }
                }
            }

            return curso;
        }

        public List<Registro> ObtenerEstudiantesRegistradosEnCurso(int courseId)
        {
            List<Registro> estudiantesRegistrados = new List<Registro>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Use the stored procedure to retrieve students by course ID
                string storedProcedure = "GetStudentsByCourseId";
                SqlCommand command = new SqlCommand(storedProcedure, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@CourseId", courseId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var estudiante = new Registro
                        {
                            StudentId = reader.GetInt32(0),
                            CourseId = reader.GetInt32(1),
                            StudentName = reader.GetString(2),
                            StudentEmail = reader.GetString(3),
                            CourseName = reader.GetString(4)
                        };

                        estudiantesRegistrados.Add(estudiante);
                    }
                }
            }

            return estudiantesRegistrados;
        }

        public void InsertAttendance(int studentId, int courseId, DateTime attendanceDate, bool isPresent)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Define the stored procedure name
                string storedProcedure = "InsertAttendance";

                // Create a SqlCommand object for executing the stored procedure
                using (SqlCommand command = new SqlCommand(storedProcedure, connection))
                {
                    // Specify that it's a stored procedure
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters required by the stored procedure
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    command.Parameters.AddWithValue("@CourseId", courseId);
                    command.Parameters.AddWithValue("@AttendanceDate", attendanceDate);
                    command.Parameters.AddWithValue("@IsPresent", isPresent);

                    // Execute the stored procedure
                    command.ExecuteNonQuery();
                }
            }
        }


    }
}