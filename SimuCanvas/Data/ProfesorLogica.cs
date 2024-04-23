using SimuCanvas.Data;
using SimuCanvas.Models;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Text.RegularExpressions;

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

        public void CreateGroup(int courseId, string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new ArgumentException("Group name cannot be empty or null.", nameof(groupName));
            }

            // Check if the courseId exists in the COURSE table
            if (!CourseExists(courseId))
            {
                throw new ArgumentException("Course does not exist.");
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Check if the group already exists
                string checkQuery = "SELECT COUNT(*) FROM Groups WHERE course_id = @courseId AND group_name = @groupName";
                SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@courseId", courseId);
                checkCommand.Parameters.AddWithValue("@groupName", groupName);
                int existingGroupsCount = (int)checkCommand.ExecuteScalar();

                if (existingGroupsCount > 0)
                {
                    throw new InvalidOperationException("Group already exists for this course.");
                }

                // If group doesn't exist, create it
                string insertQuery = "INSERT INTO Groups (course_id, group_name) VALUES (@courseId, @groupName)";
                SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@courseId", courseId);
                insertCommand.Parameters.AddWithValue("@groupName", groupName);
                insertCommand.ExecuteNonQuery();
            }
        }

        public bool CourseExists(int courseId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM COURSE WHERE course_id = @courseId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@courseId", courseId);

                return (int)command.ExecuteScalar() > 0;
            }
        }

        public List<Groups> GetGroupsByCourseId(int courseId)
        {
            List<Groups> groups = new List<Groups>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT group_id, group_name FROM Groups WHERE course_id = @courseId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@courseId", courseId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var groupId = reader.GetInt32(0);
                        var groupName = reader.GetString(1);

                        // Obtener estudiantes por grupo
                        var members = ObtenerEstudiantesPorGrupo(groupId);

                        var group = new Groups
                        {
                            GroupId = groupId,
                            GroupName = groupName,
                            Members = members
                        };

                        groups.Add(group);
                    }
                }
            }
            return groups;
        }

        public List<string> ObtenerEstudiantesPorGrupo(int groupId)
        {
            List<string> estudiantes = new List<string>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
            SELECT u.name AS member_name
            FROM GROUPMEMBERS gm
            INNER JOIN USUARIOS u ON gm.student_id = u.id_usuario
            WHERE gm.group_id = @groupId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@groupId", groupId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string memberName = !reader.IsDBNull(0) ? reader.GetString(0) : null;
                        estudiantes.Add(memberName);
                    }
                }
            }

            return estudiantes;
        }

        public bool AssignStudentToGroup(int studentId, int groupId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Check if the student exists
                string studentCheckQuery = "SELECT COUNT(*) FROM USUARIOS WHERE id_usuario = @studentId";
                SqlCommand studentCheckCommand = new SqlCommand(studentCheckQuery, connection);
                studentCheckCommand.Parameters.AddWithValue("@studentId", studentId);
                int studentCount = (int)studentCheckCommand.ExecuteScalar();

                if (studentCount == 0)
                {
                    throw new ArgumentException($"Student with ID {studentId} does not exist."); // Provide detailed error message
                }

                // Check if the group exists
                string groupCheckQuery = "SELECT COUNT(*) FROM Groups WHERE group_id = @groupId";
                SqlCommand groupCheckCommand = new SqlCommand(groupCheckQuery, connection);
                groupCheckCommand.Parameters.AddWithValue("@groupId", groupId);
                int groupCount = (int)groupCheckCommand.ExecuteScalar();

                if (groupCount == 0)
                {
                    throw new ArgumentException("Group does not exist.");
                }

                // Check if the student is already in the group
                string memberCheckQuery = "SELECT COUNT(*) FROM GROUPMEMBERS WHERE student_id = @studentId AND group_id = @groupId";
                SqlCommand memberCheckCommand = new SqlCommand(memberCheckQuery, connection);
                memberCheckCommand.Parameters.AddWithValue("@studentId", studentId);
                memberCheckCommand.Parameters.AddWithValue("@groupId", groupId);
                int memberCount = (int)memberCheckCommand.ExecuteScalar();

                if (memberCount > 0)
                {
                    throw new InvalidOperationException("Student is already in the group.");
                }

                // If validations pass, assign the student to the group
                string insertQuery = "INSERT INTO GROUPMEMBERS (student_id, group_id) VALUES (@studentId, @groupId)";
                SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@studentId", studentId);
                insertCommand.Parameters.AddWithValue("@groupId", groupId);
                insertCommand.ExecuteNonQuery();

                return true; // Return true if successful
            }
        }

        public bool RemoveStudentFromGroup(int studentId, int groupId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Verificar si el estudiante está en el grupo
                string memberCheckQuery = "SELECT COUNT(*) FROM GROUPMEMBERS WHERE student_id = @studentId AND group_id = @groupId";
                SqlCommand memberCheckCommand = new SqlCommand(memberCheckQuery, connection);
                memberCheckCommand.Parameters.AddWithValue("@studentId", studentId);
                memberCheckCommand.Parameters.AddWithValue("@groupId", groupId);
                int memberCount = (int)memberCheckCommand.ExecuteScalar();

                if (memberCount == 0)
                {
                    throw new InvalidOperationException($"El estudiante con ID {studentId} no está en el grupo con ID {groupId}.");
                }

                // Eliminar al estudiante del grupo
                string deleteQuery = "DELETE FROM GROUPMEMBERS WHERE student_id = @studentId AND group_id = @groupId";
                SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);
                deleteCommand.Parameters.AddWithValue("@studentId", studentId);
                deleteCommand.Parameters.AddWithValue("@groupId", groupId);
                int rowsAffected = deleteCommand.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    // Si no se eliminó ninguna fila, podría ser un problema
                    throw new InvalidOperationException($"Error al eliminar al estudiante con ID {studentId} del grupo con ID {groupId}.");
                }

                return true; // Devolver true si fue exitoso
            }
        }

        public List<Groups> GetGroupsByStudentAndCourse(int studentId, int courseId)
        {
            List<Groups> groups = new List<Groups>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"SELECT g.group_id, g.group_name 
                         FROM Groups g
                         INNER JOIN GROUPMEMBERS gm ON g.group_id = gm.group_id
                         WHERE g.course_id = @courseId AND gm.student_id = @studentId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@courseId", courseId);
                command.Parameters.AddWithValue("@studentId", studentId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var group = new Groups
                        {
                            GroupId = reader.GetInt32(0),
                            GroupName = reader.GetString(1)
                        };

                        groups.Add(group);
                    }
                }
            }

            return groups;
        }
        public List<Groups> GetGroupsByCourseIdAndStudentId(int courseId, int studentId)
        {
            List<Groups> groups = new List<Groups>();

            string query = "SELECT g.group_id, g.group_name, gm.student_id " +
                           "FROM GROUPS g " +
                           "JOIN GROUPMEMBERS gm ON g.group_id = gm.group_id " +
                           "WHERE g.course_id = @courseId AND gm.student_id = @studentId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@courseId", courseId);
                    command.Parameters.AddWithValue("@studentId", studentId);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Groups group = new Groups
                            {
                                GroupId = Convert.ToInt32(reader["group_id"]),
                                GroupName = reader["group_name"].ToString(),
                                // You can populate other properties of the Groups class as needed
                            };
                            groups.Add(group);
                        }
                    }
                }
            }

            return groups;
        }
        public void CreateAssignment(int courseId, string titulo, string descripcion, DateTime fechaLimite)
        {
            if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(descripcion))
            {
                throw new ArgumentException("Title and description cannot be empty or null.");
            }

            // Check if the courseId exists in the COURSE table
            if (!CourseExists(courseId))
            {
                throw new ArgumentException("Course does not exist.");
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Construct the SQL query for inserting the assignment
                    string query = "INSERT INTO ASSIGNMENT (course_id, title, description, due_date) " +
                                   "VALUES (@CourseId, @Title, @Description, @DueDate)";

                    // Create a SqlCommand object with the query and connection
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters to the SqlCommand
                        command.Parameters.AddWithValue("@CourseId", courseId);
                        command.Parameters.AddWithValue("@Title", titulo);
                        command.Parameters.AddWithValue("@Description", descripcion);
                        command.Parameters.AddWithValue("@DueDate", fechaLimite);

                        // Execute the query
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Capturar y manejar cualquier excepción que pueda ocurrir durante la inserción de la asignación
                throw new Exception("Error creating assignment: " + ex.Message);
            }
        }


        public List<Assignment> GetAllAssignments()
        {
            List<Assignment> assignments = new List<Assignment>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM ASSIGNMENT";

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Assignment assignment = new Assignment
                    {
                        AssignmentId = Convert.ToInt32(reader["assignment_id"]),
                        CourseId = Convert.ToInt32(reader["course_id"]),
                        Title = reader["title"].ToString(),
                        Description = reader["description"].ToString(),
                        DueDate = Convert.ToDateTime(reader["due_date"])
                    };

                    assignments.Add(assignment);
                }
            }

            return assignments;
        }

        public List<Assignment> GetAssignmentsByCourseId(int courseId)
        {
            List<Assignment> assignments = new List<Assignment>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM ASSIGNMENT WHERE course_id = @CourseId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CourseId", courseId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Assignment assignment = new Assignment
                                {
                                    AssignmentId = (int)reader["assignment_id"],
                                    CourseId = (int)reader["course_id"],
                                    Title = reader["title"].ToString(),
                                    Description = reader["description"].ToString(),
                                    DueDate = (DateTime)reader["due_date"]
                                };
                                assignments.Add(assignment);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving assignments by course id: " + ex.Message);
            }

            return assignments;
        }

    }
}