using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using SignalRChat.Models;

namespace SignalRChat.Database
{
    public class DbGetUsers
    {
        public bool HasUsers { get; set; }
        public async Task<List<FriendsModel>> GetUsers(GroupModel group, string userID, string searchVal)
        {

            string ids = "";
            foreach (var item in group.Users)
            {
                ids += item.UserID + ",";
            }
            ids = ids.Substring(0, (ids.Length - 1));
            string connectionString = "Server= DESKTOP-K46TA7S; Database= ChatDB; Integrated Security=True;";
            string search = "%" + searchVal + "%";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                List<FriendsModel> ls = new List<FriendsModel>();
                await conn.OpenAsync();
                string query = $"select name,id from Users where (name like @name or email like @email) and id not in({ids})";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", "%" + search + "%");
                cmd.Parameters.AddWithValue("@email", "%" + search + "%");



                SqlDataReader reader = cmd.ExecuteReader();
                while (await reader.ReadAsync())
                {
                    if (reader.HasRows)
                    {
                        HasUsers = true;
                        ls.Add(new FriendsModel()
                        {
                            FriendID = Convert.ToString(reader["id"]),
                            Name = (string)reader["name"]

                        });

                    }
                    else
                    {
                        HasUsers = false;
                    }

                }

                conn.Close();
                return ls;
            }
        }
    }
}
