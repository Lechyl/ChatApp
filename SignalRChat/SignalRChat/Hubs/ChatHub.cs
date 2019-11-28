﻿using Microsoft.AspNetCore.SignalR;
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
        Program program = new Program();

        /*static List<UserModel> userlist = new List<UserModel>();
        static List<GroupModel> groups = new List<GroupModel>();
        public static UserContext users = new UserContext()
        {
            Users = userlist,
            Groups = groups


        };*/


        //public UserModel Users = new UserModel();
        //public GroupModel GroupList = new GroupModel();
        //--------------------------------------------------------------------------------------------------  CHat Methods
        public async Task JoinChat(string user, string userID, string groupID)
        {
            await ReconnectUser(userID);
            Console.WriteLine("Connectiong to Chat" + groupID);
            await Clients.Group(groupID).SendAsync("JoinChat" + groupID, user);
        }

        public async Task LeaveChat(string user, string userID, string groupID)
        {
            await ReconnectUser(userID);
            Console.WriteLine("Leaving Chat");
            await Clients.Group(groupID).SendAsync("LeaveChat" + groupID, user);
        }
        public async Task SendMsgToGroup(string groupID, string message, string user, string userID)
        {
            await ReconnectUser(userID);
            if (program.users.Groups.Exists(g => g.GroupID == groupID))
            {
                var group = program.users.Groups.Find(g => g.GroupID == groupID);
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
            if (program.users.Groups.Exists(g => g.GroupID == groupID))
            {
                Console.WriteLine("Found Group fo Messages");
                var group = program.users.Groups.Find(g => g.GroupID == groupID);

                foreach (var item in group.Users)
                {
                    Console.WriteLine("user in group ="+item.UserID + " connected = "+item.IsConnected.ToString());
                }
                if (group.Users.Exists(u => u.UserID == userID && u.IsConnected == true))
                {
                    Console.WriteLine("Getting messages from DB");
                    List<ClientMessage> messages = db.GetTop100Messages(groupID, userID);
                    await Clients.Group(groupID).SendAsync("ReceiveDBMessages" + groupID, messages);
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

            // List<Tuple<string, List<string>>> tuples = new List<Tuple<string, List<string>>>();
            List<ClientGroupModel> ls = new List<ClientGroupModel>();

            if (program.users.Users.Exists(u => u.UserID == userID && u.IsConnected == true))
            {
            Console.WriteLine("Enter MyGroupList");


                foreach (var item in program.users.Groups.Where(g => g.Users.Exists(u => u.UserID == userID)))
                {
                    Console.WriteLine("Group " + item.GroupName);
                    ls.Add(new ClientGroupModel() { GroupName = item.GroupName, GroupID = item.GroupID });
                }
            }

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
        public async Task AddToGroup(string groupName, string userID, string groupID = null, string friendID = null)
        {
            await ReconnectUser(userID);

            //  var group = program.users.Groups.Find(g => g.GroupName == groupName);
            //var usersInGroup = group.Users.ToList();
            DbGroups db = new DbGroups();

            //Friend or Client
            if (program.users.Users.Exists(u => u.IsConnected == true && u.UserID == userID))
            {
                Console.WriteLine("c");
                if (friendID == null)
                {
                    var user = program.users.Users.Find(u => u.UserID == userID);
                    //If you're searching for a group
                    if (groupID != null)
                    {
                        //user doesn't exist in group list

                        if (!program.users.Groups.Exists(g => g.GroupID == groupID))
                        {
                            //add user to group list
                            var group = program.users.Groups.Find(g => g.GroupID == groupID);
                            if (group.Users.Exists(u => u.UserID == userID))
                            {
                                program.users.Groups.Find(g => g.GroupID == groupID).Users.Add(user);

                            }

                        }


                    }
                    else
                    {
                        int newGroupID = await db.CreateGroup(groupName);
                        program.users.Groups.Add(new GroupModel()
                        {
                            GroupName = groupName,
                            GroupID = newGroupID.ToString(),
                            Users = new List<UserModel>()

                        });
                        groupID = newGroupID.ToString();
                        program.users.Groups.Find(g => g.GroupID == groupID).Users.Add(user);

                    }
                    await db.AddUserToGroup(userID, groupID);
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupID);
                }
                else
                {
                    var friendUser = program.users.Users.Find(u => u.UserID == friendID);

                    if (program.users.Groups.Exists(g => g.GroupID == groupID))
                    {
                        //user doesn't exist in group list

                        if (!program.users.Groups.Exists(g => g.GroupID == groupID))
                        {
                            var group = program.users.Groups.Find(g => g.GroupID == groupID);
                            if (group.Users.Exists(u => u.UserID == friendID))
                            {
                                program.users.Groups.Find(g => g.GroupID == groupID).Users.Add(friendUser);

                            }
                            //add user to group list

                        }


                    }
                    else
                    {
                        int newGroupID = await db.CreateGroup(groupName);
                        program.users.Groups.Add(new GroupModel()
                        {
                            GroupName = groupName,
                            GroupID = groupID.ToString(),
                            Users = new List<UserModel>()

                        });
                        groupID = newGroupID.ToString();
                        program.users.Groups.Find(g => g.GroupID == groupID).Users.Add(friendUser);

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

            // program.users.Users = new List<UserModel>();
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
                if (program.users.Users.Exists(u => u.UserID == user.UserID))
                {
                    program.users.Users.Find(u => u.UserID == user.UserID).IsConnected = true;
                    var group = program.users.Groups.Find(g => g.Users.Exists(u => u.UserID == user.UserID));
                    group.Users.Find(u => u.UserID == user.UserID).IsConnected = true;

                    Console.WriteLine("exist user "+program.users.Users.Find(u => u.UserID == user.UserID).IsConnected.ToString());

                }
                else
                {
                    user.IsConnected = true;
                    program.users.Users.Add(user);

                    var group = program.users.Groups.Find(g => g.Users.Exists(u => u.UserID == user.UserID));
                    group.Users.Find(u => u.UserID == user.UserID).IsConnected = true;

                    Console.WriteLine("new user "+program.users.Users.Find(u => u.UserID == user.UserID).IsConnected.ToString());

                }
              /*  DbStartUp db = new DbStartUp();
                var result = await db.GetGroups();
                program.users.Groups = result;*/
                Console.WriteLine("done = " + program.users.Users.Find(u => u.UserID == user.UserID).Name);

            }
            else
            {
                stringUser[3] = "0";
            }

            await Clients.Client(cid).SendAsync("ReceiveAccount", stringUser);
        }

        public void LogOut(string userID)
        {
            //remove user from user list
            program.users.Users.Find(u => u.UserID == userID).IsConnected = false;

            var group = program.users.Groups.Find(g => g.Users.Exists(u => u.UserID == userID));
            group.Users.Find(u => u.UserID == userID).IsConnected = false;
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


            //Console.WriteLine("cw = " + program.users.Users.Find(u => u.UserID == userID).Password);
            string cid = Context.ConnectionId;
            if (program.users.Users != null)
            {

                if (program.users.Users.Exists(u => u.UserID == userID))
                {
                    program.users.Users.Find(u => u.UserID == userID).ConnectionID = cid;

                    Console.WriteLine("hey");
                    //program.users.Groups.Where(g => g.Users.Exists(u => u.UserID == userID));

                    //Reconnect to all subscripted Groups
                    foreach (var item in program.users.Groups.Where(g => g.Users.Exists(u => u.UserID == userID)))
                    {
                        await Groups.AddToGroupAsync(cid, item.GroupID);
                    }
                }
            }



        }




    }
}
