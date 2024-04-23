﻿using SimuCanvas.Models;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SimuCanvas.Data
{
    public class EstudiantesLogica
    {
        private string _connectionString = "Data Source=KIMBERLYSLAPTOP\\SQLEXPRESS01;Initial Catalog=SimCanvas;Integrated Security=True;";

        public List<Usuario> ObtenerUsuariosPorRol(string rol)
        {
            List<Usuario> usuarios = new List<Usuario>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT u.Id_usuario, u.name, u.address, u.dob, u.Email, u.password, u.role " +
                               "FROM USUARIOS u " +
                               "WHERE u.role = @rol";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@rol", rol);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var usuario = new Usuario
                        {
                            IdUsuario = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Address = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Dob = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3),
                            Email = reader.GetString(4),
                            Password = reader.GetString(5),
                            Role = reader.GetString(6)
                        };

                        usuarios.Add(usuario);
                    }
                }
            }
            return usuarios;
        }

        public bool RegistrarEstudianteACurso(int studentId, int courseId)
        {
            // Verificar si el estudiante ya está registrado en el curso
            bool estudianteRegistrado = VerificarEstudianteRegistradoEnCurso(studentId, courseId);

            if (estudianteRegistrado)
            {
                // Si el estudiante ya está registrado en el curso, no se registra de nuevo
                return false;
            }

            // Obtener información del estudiante y curso
            var student = ObtenerUsuarioPorId(studentId);
            var course = ObtenerCursoPorId(courseId);

            if (student == null || course == null)
            {
                // Si el estudiante o el curso no existen, retornar falso
                return false;
            }

            // Verificar la capacidad máxima del curso
            int estudiantesRegistrados = ObtenerNumeroEstudiantesRegistradosEnCurso(courseId);

            if (estudiantesRegistrados >= course.MaxStudents)
            {
                // Si la capacidad máxima del curso ha sido alcanzada, no se puede registrar más estudiantes
                return false;
            }

            // Insertar el registro del estudiante en el curso en la base de datos
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "INSERT INTO REGISTRO (student_id, course_id, student_name, student_email, course_name) " +
                               "VALUES (@studentId, @courseId, @studentName, @studentEmail, @courseName)";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@studentId", studentId);
                command.Parameters.AddWithValue("@courseId", courseId);
                command.Parameters.AddWithValue("@studentName", student.Name);
                command.Parameters.AddWithValue("@studentEmail", student.Email);
                command.Parameters.AddWithValue("@courseName", course.Title);

                try
                {
                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("Error al intentar insertar el registro: " + ex.Message);
                    return false;
                }
            }
        }

        private int ObtenerNumeroEstudiantesRegistradosEnCurso(int courseId)
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

        public Usuario ObtenerUsuarioPorId(int userId)
        {
            Usuario usuario = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT Id_usuario, name, address, dob, Email, password, role " +
                               "FROM USUARIOS " +
                               "WHERE Id_usuario = @userId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@userId", userId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        usuario = new Usuario
                        {
                            IdUsuario = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Address = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Dob = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3),
                            Email = reader.GetString(4),
                            Password = reader.GetString(5),
                            Role = reader.GetString(6)
                        };
                    }
                }
            }
            return usuario;
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

        private bool VerificarEstudianteRegistradoEnCurso(int studentId, int courseId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM REGISTRO WHERE student_id = @studentId AND course_id = @courseId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@studentId", studentId);
                command.Parameters.AddWithValue("@courseId", courseId);

                int count = (int)command.ExecuteScalar();

                return count > 0;
            }
        }

        public List<Registro> ObtenerEstudiantesRegistrados()
        {
            List<Registro> estudiantesRegistrados = new List<Registro>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT student_id, course_id, student_name, student_email, course_name FROM REGISTRO";

                SqlCommand command = new SqlCommand(query, connection);

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

        public List<Course> ObtenerCursosMatriculados(int estudianteId)
        {
            List<Course> cursosMatriculados = new List<Course>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"SELECT c.course_id, c.faculty_id, c.Title, c.Description, c.Credits, c.InitialDate, c.FinalDate, c.MaxStudents 
                                 FROM COURSE c 
                                 JOIN REGISTRO r ON c.course_id = r.course_id 
                                 WHERE r.student_id = @estudianteId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@estudianteId", estudianteId);

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

                        cursosMatriculados.Add(curso);
                    }
                }
            }
            return cursosMatriculados;
        }

        public bool EliminarEstudianteDeCurso(int studentId, int courseId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "DELETE FROM REGISTRO WHERE student_id = @studentId AND course_id = @courseId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@studentId", studentId);
                command.Parameters.AddWithValue("@courseId", courseId);

                try
                {
                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("Error al intentar eliminar al estudiante del curso: " + ex.Message);
                    return false;
                }
            }

        }
        public List<Attendance> ObtenerAsistenciaEstudianteEnCurso(int studentId, int courseId)
        {
            List<Attendance> attendanceRecords = new List<Attendance>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT attendance_date, is_present FROM Attendance WHERE student_id = @studentId AND course_id = @courseId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@studentId", studentId);
                command.Parameters.AddWithValue("@courseId", courseId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var attendanceRecord = new Attendance
                        {
                            AttendanceDate = reader.GetDateTime(0),
                            IsPresent = reader.GetBoolean(1)
                        };

                        attendanceRecords.Add(attendanceRecord);
                    }
                }
            }

            return attendanceRecords;
        }


        public Assignment GetAssignmentDetails(int assignmentId)
        {
            Assignment assignment = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT assignment_id, course_id, title, description, due_date FROM ASSIGNMENT WHERE assignment_id = @AssignmentId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AssignmentId", assignmentId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        assignment = new Assignment
                        {
                            AssignmentId = reader.GetInt32(0),
                            CourseId = reader.GetInt32(1),
                            Title = reader.GetString(2),
                            Description = reader.GetString(3),
                            DueDate = reader.GetDateTime(4)
                        };
                    }
                }
            }

            return assignment;
        }

        public List<Groups> ObtenerGruposEstudiante(int studentId)
        {
            List<Groups> gruposEstudiante = new List<Groups>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
            SELECT g.group_id, g.group_name, u.name AS member_name
            FROM GROUPS g
            INNER JOIN GROUPMEMBERS gm ON g.group_id = gm.group_id
            INNER JOIN USUARIOS u ON gm.student_id = u.id_usuario
            WHERE gm.group_id IN (
                SELECT group_id
                FROM GROUPMEMBERS
                WHERE student_id = @studentId
            )";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@studentId", studentId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int groupId = reader.GetInt32(0);
                        string groupName = reader.GetString(1);
                        string memberName = !reader.IsDBNull(2) ? reader.GetString(2) : null;

                        // Check if the group already exists in the list
                        Groups existingGroup = gruposEstudiante.FirstOrDefault(g => g.GroupId == groupId);
                        if (existingGroup != null)
                        {
                            if (!string.IsNullOrEmpty(memberName))
                            {
                                existingGroup.Members.Add(memberName);
                            }
                        }
                        else
                        {
                            Groups newGroup = new Groups
                            {
                                GroupId = groupId,
                                GroupName = groupName,
                                Members = new List<string>()
                            };
                            if (!string.IsNullOrEmpty(memberName))
                            {
                                newGroup.Members.Add(memberName);
                            }
                            gruposEstudiante.Add(newGroup);
                        }
                    }
                }
            }

            return gruposEstudiante;
        }

        public List<Assignment> ObtenerAsignaturasCurso(int studentId, int courseId)
        {
            List<Assignment> asignaturasCurso = new List<Assignment>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
    SELECT a.assignment_id, a.course_id, a.title, a.description, a.due_date
    FROM ASSIGNMENT a
    WHERE a.course_id = @courseId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@courseId", courseId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Assignment assignment = new Assignment
                        {
                            AssignmentId = reader.GetInt32(0),
                            CourseId = reader.GetInt32(1),
                            Title = reader.GetString(2),
                            Description = reader.GetString(3),
                            DueDate = reader.GetDateTime(4)
                        };

                        asignaturasCurso.Add(assignment);
                    }
                }
            }

            return asignaturasCurso;
        }

    }
}