using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRChat.Database
{
    public class Login
    {
        object obj;

        public async Task<string[]> VerifyLogin(string inputEmail, string inputPassword, string inputConnectionID)
        {


            string connetionString = "Server= DESKTOP-K46TA7S; Database= ChatDB; Integrated Security=True;";

            string[] arr = new string[5];
            bool IsConnected = false;

            using (SqlConnection connection = new SqlConnection(connetionString))
            {
                await connection.OpenAsync();
                SqlCommand command = new SqlCommand("select * from Users where email = @email and password = @password",connection);
                command.Parameters.AddWithValue("@email", inputEmail);
                command.Parameters.AddWithValue("@password", inputPassword);
                SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (reader.HasRows)
                    {

                        if ((string)reader["email"] == inputEmail && (string)reader["password"] == inputPassword)
                        {

                            arr[0] = (string)reader["name"];
                            arr[1] = (string)reader["email"];
                            arr[2] = (string)reader["password"];
                            arr[3] = inputConnectionID;
                            arr[4] = Convert.ToString(reader["id"]);

                            IsConnected = true;


                        }
                    }
                }
                connection.Close();

            }
            if (IsConnected)
            {
                using (SqlConnection connection1 = new SqlConnection(connetionString))
                {
                    await connection1.OpenAsync();
                    SqlCommand command1 = new SqlCommand("UPDATE Users SET connectionID = @connectionID WHERE email = @email", connection1);
                    command1.Parameters.AddWithValue("@connectionID", inputConnectionID);
                    command1.Parameters.AddWithValue("@email", inputEmail);

                    await command1.ExecuteReaderAsync();
                    connection1.Close();
                }
            }
            else
            {
                arr[4] = "0";
            }

            return arr;

        }
    }

}


