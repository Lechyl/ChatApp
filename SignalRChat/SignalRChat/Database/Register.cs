using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRChat.Database
{
    public class Register
    {
        public async Task<bool> RegisterUser(string name, string email, string password, string connectionID)
        {

            string connectionstring = "Server= DESKTOP-K46TA7S; Database= ChatDB; Integrated Security=True;";
            bool IsRegistered = false;
            using (SqlConnection connection = new SqlConnection(connectionstring))
            {
                await connection.OpenAsync();
                string query = "insert into Users (name,email,password,connectionID) values(@name,@email,@password,@conn)";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@conn", connectionID);

                await cmd.ExecuteNonQueryAsync();
                IsRegistered = true;
            }
            return IsRegistered;
        }
    }
}
