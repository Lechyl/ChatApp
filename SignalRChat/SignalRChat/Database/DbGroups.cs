using SignalRChat.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRChat.Database
{
    public class DbGroups
    {

        public async Task<int> CreateGroup(string groupName)
        {
            string connectionString = "Server= DESKTOP-K46TA7S; Database= ChatDB; Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "Insert into Groups (groupName) values (@groupName); select cast(SCOPE_IDENTITY() AS INT);";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@groupName", groupName);

                int id = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                cmd.Dispose();
                conn.Close();
                return id;
            }
        }

        public async Task AddUserToGroup(string userID, string groupID)
        {
            string connectionString = "Server= DESKTOP-K46TA7S; Database= ChatDB; Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "insert into Group_connections (userID, groupID) values (@userID, @groupID)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@userID", int.Parse(userID));
                cmd.Parameters.AddWithValue("@groupID", int.Parse(groupID));


                await cmd.ExecuteNonQueryAsync();

                cmd.Dispose();
                conn.Close();
            }
        }
        public async Task<List<FriendsModel>> GetAllusersInGroup(string groupID)
        {
            string connectionString = "Server= DESKTOP-K46TA7S; Database= ChatDB; Integrated Security=True;";
            List<FriendsModel> ls = new List<FriendsModel>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "select u.id,u.name from Group_connections as gc inner join Groups as g on g.id = gc.groupID inner join Users u on u.id = gc.userID where g.id = @groupID";
                SqlCommand cmd = new SqlCommand(query,conn);
                cmd.Parameters.AddWithValue("@groupID",int.Parse(groupID));

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        ls.Add(new FriendsModel()
                        {
                            FriendID = Convert.ToString(reader["id"]),
                            Name = (string)reader["name"]
                        });
                    }
                }

                cmd.Dispose();
                conn.Close();

                return ls;
            }
        }

        public async Task RemoveUserFromGroup(string removeUserID,string groupID)
        {
            string connectionString = "Server= DESKTOP-K46TA7S; Database= ChatDB; Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "delete from Group_connections where userID = @userID and groupID = @groupID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@userID", removeUserID);
                cmd.Parameters.AddWithValue("@groupID", groupID);

                await cmd.ExecuteNonQueryAsync();
            }
            
        }
    }
}
