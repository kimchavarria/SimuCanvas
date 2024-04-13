using SimuCanvas.Models;
using System.Data.SqlClient;

namespace SimuCanvas.Data
{
    public class LoginLogica
    {
        private string _connectionString = "Data Source=KIMBERLYSLAPTOP\\SQLEXPRESS01;Initial Catalog=SimCanvas;Integrated Security=True;";

        public Usuario ValidarUsuario(string _correo, string _clave)
        {
            Usuario usuario = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT u.Id_usuario, u.name, u.address, u.dob, u.Email, u.password, u.role " +
                               "FROM USUARIOS u " +
                               "WHERE u.Email = @correo AND u.password = @clave";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@correo", _correo);
                command.Parameters.AddWithValue("@clave", _clave);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        usuario = new Usuario
                        {
                            IdUsuario = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Address = reader.GetString(2),
                            Dob = reader.GetDateTime(3),
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