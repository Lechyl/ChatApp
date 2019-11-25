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

                conn.Close();
            }
        }
    }
}
