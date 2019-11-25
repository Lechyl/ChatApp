using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SignalRChat.Database;
using SignalRChat.Models;

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
        //-------------------------------------------------------------------------------------------------- Test CHat Methods
        public async Task JoinChat(string user, string groupName)
        {
            await Clients.Group(groupName).SendAsync("JoinChat", user);
        }

        public async Task LeaveChat(string user, string groupName)
        {
            await Clients.Group(groupName).SendAsync("LeaveChat", user);
        }
        public async Task SendMessageAll(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
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
            Console.WriteLine("real conn" + Context.ConnectionId);
            Console.WriteLine("Enter MyGroupList");

            // List<Tuple<string, List<string>>> tuples = new List<Tuple<string, List<string>>>();
            List<string> ls = new List<string>();

                users.Groups.ForEach(g => {

                    if (g.Users.Exists(u => u.UserID == userID))
                    {

                        Console.WriteLine("Group " +g.GroupName);
                        ls.Add(g.GroupName);
                       

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
                await Clients.Client(cid).SendAsync("ReceiveGroupList", ls);
            

        }

        //Add player to group and/or create group and put yourself into the group
        public async Task AddToGroup(string groupName, string userID)
        {
            await ReconnectUser(userID);

            var user = users.Users.Find(u => u.UserID == userID);
            //  var group = users.Groups.Find(g => g.GroupName == groupName);
            //var usersInGroup = group.Users.ToList();
            DbGroups db = new DbGroups();

            if (users.Groups.Exists(g => g.GroupName == groupName))
            {
                //user doesn't exist in group list

                if (!users.Groups.Exists(g => g.Users.Exists(u => u.UserID == userID)))
                {
                    //add user to group list
                    users.Groups.Find(g => g.GroupName == groupName).Users.Add(user);

                }


            }
            else
            {
                int groupID = await db.CreateGroup(groupName);
                users.Groups.Add(new GroupModel()
                {
                    GroupName = groupName,
                    GroupID = groupID.ToString(),
                    Users = new List<UserModel>()

                });
                users.Groups.Find(g => g.GroupName == groupName).Users.Add(user);

            }
            await db.AddUserToGroup(userID, users.Groups.Find(g => g.GroupName == groupName).GroupID);
            await Groups.AddToGroupAsync(user.ConnectionID, groupName);



        }

        public async Task SendMsgToGroup(string groupName, string message, string user)
        {
           

            await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
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
                users.Users.Add(user);
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
            users.Users.RemoveAll(u => u.UserID == userID);
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

        //-------------------------------------------------------------------------------------------------- FriendList

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

                }
            }



        }




    }
}
