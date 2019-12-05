using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using SignalRChat.Models;

namespace SignalRChat.Database
{
    public class DbSendMessage
    {

        public async Task SendMessage(string message, string userID, string groupID)
        {

            string connectionString = "Server= DESKTOP-K46TA7S; Database= ChatDB; Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "insert into GroupMessage(message, fromUserID,groupID) values (@message ,@user , @groupID)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@message", message);
                cmd.Parameters.AddWithValue("@user", userID);
                cmd.Parameters.AddWithValue("@groupID", groupID);

                await cmd.ExecuteNonQueryAsync();
                conn.Close();
            }

        }

        public async Task<List<ClientMessage>> GetTop100Messages(string groupID, string userID)
        {
            string connectionString = "Server= DESKTOP-K46TA7S; Database= ChatDB; Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "Select Top 100 Users.name,message, fromUserID as userID from GroupMessage inner join Users on Users.id = GroupMessage.fromUserID where groupID = @groupID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@groupID", groupID);
                
                SqlDataReader reader = cmd.ExecuteReader();
                List<ClientMessage> ls = new List<ClientMessage>();
                while ( await reader.ReadAsync())
                {
                    ls.Add(new ClientMessage() { message = (string)reader["message"], user = (string)reader["name"], isOwnMessage = Convert.ToString(reader["userID"]) == userID, isSystemMessage = false });
                }
                conn.Close();
                return ls;
            }
        }
    }
}
