using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SignalRChat.Database;
using SignalRChat.Models;
using System.Text.Json;

namespace SignalRChat.Hubs
{
    public class ChatHub : Hub
    {
        static List<UserModel> userlist = new List<UserModel>();
        static List<GroupModel> groups = new List<GroupModel>();
        public UserContext users = new UserContext()
        {
            Users = userlist,
            Groups = groups


        };

        
        //public UserModel Users = new UserModel();
        //public GroupModel GroupList = new GroupModel();
        //--------------------------------------------------------------------------------------------------  CHat Methods
        public async Task JoinChat(string user,string userID, string groupID)
        {
            await ReconnectUser(userID);
            Console.WriteLine("Connectiong to Chat" + groupID);
            await Clients.Group(groupID).SendAsync("JoinChat" + groupID, user);
        }

        public async Task LeaveChat(string user,string userID, string groupID)
        {
            await ReconnectUser(userID);
            Console.WriteLine("Leaving Chat");
            await Clients.Group(groupID).SendAsync("LeaveChat" + groupID, user);
        }
        public async Task SendMsgToGroup(string groupID, string message, string user, string userID)
        {
            await ReconnectUser(userID);
            if (users.Groups.Exists(g => g.GroupID == groupID))
            {
                var group = users.Groups.Find(g => g.GroupID == groupID);
                if (group.Users.Exists(u => u.UserID == userID))
                {
                    DbSendMessage db = new DbSendMessage();
                    await db.SendMessage(message, userID, groupID);
                    string method = "ReceiveMessage" + groupID;
                    await Clients.Group(groupID).SendAsync(method, user, message);
                    Console.WriteLine(method);
                }

            }
        }

        //-------------------------------------------------------------------------------------------------- GET Friendlist messages

        public async Task GetMessages(string userID, string groupID)
        {
            DbSendMessage db = new DbSendMessage();
            if(users.Groups.Exists(g => g.GroupID == groupID))
            {
                var group = users.Groups.Find(g => g.GroupID == groupID);

                if(group.Users.Exists(u => u.UserID == userID && u.IsConnected == true))
                {
                    var messages = db.GetTop100Messages(groupID,userID);
                    await Clients.Group(groupID).SendAsync("ReceiveDBMessages"+groupID,messages);
                }
            }

        }



        //-------------------------------------------------------------------------------------------------- Single Chat Methods
        /* public async Task SendMessageClient(string user, string message, string toUserID)
         {
             if (users.Exists(user1 => user1.UserID == toUserID))
             {
                 UserModel userExist = users.Find(user1 => user1.UserID == toUserID);

                 await Clients.Client(userExist.ConnectionID).SendAsync("ReceiveMessage", user, message);

             }
             else
             {

             }
         }
         */
        //-------------------------------------------------------------------------------------------------- Group Methods

        public async Task MyGroupList(string userID)
        {
            await ReconnectUser(userID);
            
            Console.WriteLine("real conn" + Context.User.Identity.Name);
            Console.WriteLine("Enter MyGroupList");

            // List<Tuple<string, List<string>>> tuples = new List<Tuple<string, List<string>>>();
            List<ClientGroupModel> ls = new List<ClientGroupModel>();

                users.Groups.ForEach(g => {

                    if (g.Users.Exists(u => u.UserID == userID && u.IsConnected == true))
                    {

                        Console.WriteLine("Group " +g.GroupName);
                        ls.Add(new ClientGroupModel() {  GroupName = g.GroupName, GroupID = g.GroupID});
                       

                    }
                });
            /*
            List<string> ls = new List<string>();
            foreach(var item in groups)
            {
                ls.Add(item.GroupName);
            }*/
            string cid = Context.ConnectionId;
                Console.WriteLine("send");

            
            var userDes = JsonSerializer.Serialize<List<ClientGroupModel>>(ls);

            await Clients.Client(cid).SendAsync("ReceiveGroupList", ls);
            

        }

        //Add player to group and/or create group and put yourself into the group
        public async Task AddToGroup(string groupName, string userID,string groupID = null, string friendID = null)
        {
            await ReconnectUser(userID);

            //  var group = users.Groups.Find(g => g.GroupName == groupName);
            //var usersInGroup = group.Users.ToList();
            DbGroups db = new DbGroups();

            //Friend or Client
            if(users.Users.Exists(u => u.IsConnected == true))
            {

            if(friendID == null)
            {
                var user = users.Users.Find(u => u.UserID == userID);
                //If you're searching for a group
                if (groupID != null)
                {
                    //user doesn't exist in group list

                    if (!users.Groups.Exists(g => g.GroupID == groupID))
                    {
                        //add user to group list
                        var group = users.Groups.Find(g => g.GroupID == groupID);
                        if(group.Users.Exists(u => u.UserID == userID))
                        {
                            users.Groups.Find(g => g.GroupID == groupID).Users.Add(user);

                        }

                    }


                }
                else
                {
                    int newGroupID = await db.CreateGroup(groupName);
                    users.Groups.Add(new GroupModel()
                    {
                        GroupName = groupName,
                        GroupID = newGroupID.ToString(),
                        Users = new List<UserModel>()

                    });
                    groupID = newGroupID.ToString();
                    users.Groups.Find(g => g.GroupID == groupID).Users.Add(user);

                }
                await db.AddUserToGroup(userID, groupID);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupID);
            }
            else
            {
                var friendUser = users.Users.Find(u => u.UserID == friendID);

                if (users.Groups.Exists(g => g.GroupID == groupID))
                {
                    //user doesn't exist in group list

                    if (!users.Groups.Exists(g => g.GroupID == groupID))
                    {
                        var group = users.Groups.Find(g => g.GroupID == groupID);
                        if (group.Users.Exists(u => u.UserID == friendID))
                        {
                            users.Groups.Find(g => g.GroupID == groupID).Users.Add(friendUser);

                        }
                        //add user to group list

                    }


                }
                else
                {
                    int newGroupID = await db.CreateGroup(groupName);
                    users.Groups.Add(new GroupModel()
                    {
                        GroupName = groupName,
                        GroupID = groupID.ToString(),
                        Users = new List<UserModel>()

                    });
                    groupID = newGroupID.ToString();
                    users.Groups.Find(g => g.GroupID == groupID).Users.Add(friendUser);

                }
                await db.AddUserToGroup(friendID, groupID);
                if (friendUser.IsConnected)
                {
                    await Groups.AddToGroupAsync(friendUser.ConnectionID, groupID);

                }
                }

            }

        }



        //-------------------------------------------------------------------------------------------------- Log in and Log Out Methods
        public async Task LogIn(string email, string password)
        {

            // users.Users = new List<UserModel>();
            DbLogin login = new DbLogin();

            //Find ConnectionID
            string cid = Context.ConnectionId;
            //Verify and create user
            UserModel user = await login.VerifyLogin(email, password, cid);

            string[] stringUser = new string[4];
            //if user exist
            if (login.VerifiedUser)
            {
                await ReconnectUser(user.UserID);

                Console.WriteLine("starting");

                stringUser[0] = user.Name;
                stringUser[1] = user.Email;
                stringUser[2] = user.Password;
                stringUser[3] = user.UserID;
                //add user to users list
                if(users.Users.Exists(u => u.UserID == user.UserID))
                {
                    users.Users.Find(u => u.UserID == user.UserID).IsConnected = true;

                }
                else
                {
                    users.Users.Add(user);
                }

                Console.WriteLine("done = " + users.Users.Find(u => u.UserID == user.UserID).Name);

            }
            else
            {
                stringUser[4] = "0";
            }

            await Clients.Client(cid).SendAsync("ReceiveAccount", stringUser);
        }

        public void LogOut(string userID)
        {
            //remove user from user list
            users.Users.Find(u => u.UserID == userID).IsConnected = false;
        }

        //-------------------------------------------------------------------------------------------------- Register Method

        public async Task Register(string name, string email, string password)
        {
            //new users has "new user" as default connnection string
            string cid = "New User";
            //create temp connection to send confirm msg.
            string tempCid = Context.ConnectionId;

            DbRegister register = new DbRegister();
            //Register user to db
            bool IsRegistered = await register.RegisterUser(name, email, password, cid);

            await Clients.Client(tempCid).SendAsync("ReceieveRegisterMsg", IsRegistered);

            ;

        }


        //-------------------------------------------------------------------------------------------------- Reconnect
        public async Task ReconnectUser(string userID)
        {


            //Console.WriteLine("cw = " + users.Users.Find(u => u.UserID == userID).Password);
            string cid = Context.ConnectionId;
            if (users.Users != null)
            {

                if (users.Users.Exists(u => u.UserID == userID))
                {
                    users.Users.Find(u => u.UserID == userID).ConnectionID = cid;

                    Console.WriteLine("hey");
                     //users.Groups.Where(g => g.Users.Exists(u => u.UserID == userID));

                    //Reconnect to all subscripted Groups
                    foreach(var item in users.Groups.Where(g => g.Users.Exists(u => u.UserID == userID)))
                    {
                        await Groups.AddToGroupAsync(cid, item.GroupID);
                    }
                }
            }



        }




    }
}
