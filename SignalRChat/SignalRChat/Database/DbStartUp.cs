using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using SignalRChat.Models;
namespace SignalRChat.Database
{
    public class DbStartUp
    {


        public List<GroupModel> GetGroups()
        {

            string connectionString = "Server= DESKTOP-K46TA7S; Database= ChatDB; Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                 conn.Open();
                string query = "select gc.groupID,g.groupName, u.id,u.name,u.email,u.password,u.connectionID from Group_connections as gc inner join Users as u on u.id = gc.userID inner join Groups as g on g.id = gc.groupID order by gc.groupID";
                SqlCommand cmd = new SqlCommand(query, conn);


                SqlDataReader reader = cmd.ExecuteReader();

                List<GroupModel> groups = new List<GroupModel>();
                
                List<UserModel> users = new List<UserModel>();
                
                while( reader.Read())
                {
                    if (reader.HasRows)
                    {
                        UserModel user = new UserModel() {
                            Name = (string)reader["name"],
                            Email = (string)reader["email"],
                            Password = (string)reader["password"],
                            ConnectionID = (string)reader["connectionID"],
                            UserID = Convert.ToString(reader["id"]),
                            IsConnected = false
                        };
                      //  Console.WriteLine((string)reader["groupName"] + " " + (string)reader["name"]);
                        if(groups.Exists(g => g.GroupID == Convert.ToString( reader["groupID"])))
                        {
                            //Exist
                            var group = groups.Find(g => g.GroupID == Convert.ToString( reader["groupID"]));
                            group.Users.Add(user);
                        }
                        else
                        {
                            GroupModel group = new GroupModel() {
                                GroupID = Convert.ToString( reader["groupID"]),
                                GroupName = (string)reader["groupName"],
                                Users = new List<UserModel>()
                            };
                            group.Users.Add(user);
                            groups.Add(group);
                            //New Group
                        }
                    }
                }

                conn.Close();
                Console.WriteLine("Up and running...");
                return groups;
            }



        }

        public List<UserModel> GetUsers()
        {


            string connectionString = "Server= DESKTOP-K46TA7S; Database= ChatDB; Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "select name,id from Users";
                SqlCommand cmd = new SqlCommand(query,conn);

                SqlDataReader reader = cmd.ExecuteReader();

                List<UserModel> ls = new List<UserModel>();
                while (reader.Read())
                {
                    ls.Add(new UserModel()
                    {
                        UserID =Convert.ToString( reader["id"]),
                        Name = (string)reader["name"]

                    });

                }


                conn.Close();

                return ls;
            }

        }
    }
}
