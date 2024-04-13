using SimuCanvas.Models;
using System.Data.SqlClient;

namespace SimuCanvas.Data
{
    public class PerfilLogica
    {
        private string _connectionString = "Data Source=KIMBERLYSLAPTOP\\SQLEXPRESS01;Initial Catalog=SimCanvas;Integrated Security=True;";
        public Usuario ObtenerUsuarioPorEmail(string email)
        {
            Usuario usuario = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT u.Id_usuario, u.name, u.address, u.dob, u.Email, u.password, u.role " +
                               "FROM USUARIOS u " +
                               "WHERE u.Email = @correo";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@correo", email);

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
                            Role = reader.GetString(6) // Cambiado de List<string> a string
                        };
                    }
                }
            }

            return usuario;
        }
    }
}