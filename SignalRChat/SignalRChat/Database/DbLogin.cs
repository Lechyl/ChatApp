using SignalRChat.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRChat.Database
{
    public class DbLogin
    {
        public bool VerifiedUser;

        public async Task<UserModel> VerifyLogin(string inputEmail, string inputPassword, string inputConnectionID)
        {

            UserModel user = new UserModel();
            //user.Friends = new List<FriendsModel>();
            List<GroupModel> assignedGroups = new List<GroupModel>();
            string connetionString = "Server= DESKTOP-K46TA7S; Database= ChatDB; Integrated Security=True;";

           // string[] arr = new string[5];
            VerifiedUser = false;

            //Check if user exist
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

                            
                            user.Name = (string)reader["name"];
                            user.Email = (string)reader["email"];
                            user.Password = (string)reader["password"];
                            user.ConnectionID = inputConnectionID;
                            user.UserID = Convert.ToString(reader["id"]);

                            VerifiedUser = true;


                        }
                    }
                }
                connection.Close();

            }
            if (VerifiedUser)
            {
                //Update Connection
                using (SqlConnection connection = new SqlConnection(connetionString))
                {
                    await connection.OpenAsync();
                    SqlCommand command1 = new SqlCommand("UPDATE Users SET connectionID = @connectionID WHERE email = @email", connection);
                    command1.Parameters.AddWithValue("@connectionID", inputConnectionID);
                    command1.Parameters.AddWithValue("@email", inputEmail);

                    await command1.ExecuteReaderAsync();
                    connection.Close();
                }

                // get friends
                using (SqlConnection connection = new SqlConnection(connetionString))
                {
                    await connection.OpenAsync();
                    SqlCommand command1 = new SqlCommand("select case when f.userIDLink1 != u.id then f.userIDLink1 when f.userIDLink2 != u.id then f.userIDLink2 else 'ERROR' end as FriendID, case when f.userIDLink1 != u.id then e.name when f.userIDLink2 != u.id then d.name else 'ERROR' end as Name from Friends as f inner join Users as u on(u.id = f.userIDLink1) or(u.id = f.userIDLink2) left join Users as e on(e.id = f.userIDLink1) left join Users as d on(d.id = f.userIDLink2) where u.email = @email and u.password = @password", connection);
                    command1.Parameters.AddWithValue("@password", inputPassword);
                    command1.Parameters.AddWithValue("@email", inputEmail);
                    SqlDataReader reader = await command1.ExecuteReaderAsync();
                    
                    while (await reader.ReadAsync())
                    {
                        if (reader.HasRows)
                        {
                            

                            user.Friends.Add(new FriendsModel()
                            {
                                FriendID = Convert.ToString(reader["FriendID"]),
                                Name = Convert.ToString(reader["Name"])
                            });
                        }

                    }
                    connection.Close();
                }
                // Get Groups

            }

            return user;

        }
    }

}


