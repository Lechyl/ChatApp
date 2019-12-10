using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SignalRChat.Database;
using SignalRChat.Models;
using System.Text.Json;
using System.Collections.ObjectModel;

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
        /*   public async Task JoinChat(string user, string userID, string groupID)
           {
               await ReconnectUser(userID);
               Console.WriteLine("Connectiong to Chat" + groupID);
               await Clients.Group(groupID).SendAsync("JoinChat" + groupID, user);

               var group = program.users.Groups.Find(g => g.Users.Exists(u => u.UserID == user.UserID));
               group.Users.Find(u => u.UserID == user.UserID).IsConnected = true;
           }
           */


        //Remember to to create instance with your ipaddress, [SignalRChat -> Properties] insert ipaddress for both http and https ###################### #############

        public async Task LeaveChat(string userID, string groupID)
        {
            await ReconnectUser(userID);
            program.users.Groups.Find(g => g.GroupID == groupID).Users.Find(u => u.UserID == userID).IsConnected = false;



        }
        public async Task SendMsgToGroup(string groupID, string message, string user, string userID)
        {
            await ReconnectUser(userID);
            if (program.users.Groups.Exists(g => g.GroupID == groupID))
            {
                var group = program.users.Groups.Find(g => g.GroupID == groupID);
                if (group.Users.Exists(u => u.UserID == userID && u.IsConnected == true))
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

        public async Task JoinChatRoom(string userID, string groupID)
        {
            await ReconnectUser(userID);


            DbSendMessage db = new DbSendMessage();
            if (program.users.Groups.Exists(g => g.GroupID == groupID))
            {
                var group = program.users.Groups.Find(g => g.GroupID == groupID);



                if (group.Users.Exists(u => u.UserID == userID))
                {

                    group.Users.Find(u => u.UserID == userID).IsConnected = true;
                    var messagesFromDB = await db.GetTop100Messages(groupID, userID);
                    List<ClientMessage> messages = new List<ClientMessage>();

                    messages = messagesFromDB;
                    string mtd = "JoinChat" + groupID;
                    string cid = Context.ConnectionId;
                    await Clients.Client(cid).SendAsync(mtd, messages);
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


        public async Task ViewAllUsersInGroup(string userID, string groupID)
        {
            await ReconnectUser(userID);
            //Checking if group exist and if the client is inside the group for security measure against unautherorisized users
            if(program.users.Groups.Find(g => g.GroupID == groupID).Users.Exists( u => u.UserID == userID))
            {
                DbGroups db = new DbGroups();
                List<FriendsModel> newls = new List<FriendsModel>();
                var ls = await db.GetAllusersInGroup(groupID);
                newls = ls;
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveAllUsersInGroup", newls);
            }
        }
        public async Task MyGroupList(string userID)
        {
            await ReconnectUser(userID);


            List<ClientGroupModel> ls = new List<ClientGroupModel>();

            if (program.users.Users.Exists(u => u.UserID == userID && u.IsConnected == true))
            {


                foreach (var item in program.users.Groups.Where(g => g.Users.Exists(u => u.UserID == userID)))
                {
                    ls.Add(new ClientGroupModel() { GroupName = item.GroupName, GroupID = item.GroupID });
                }
            }


            string cid = Context.ConnectionId;

            var userDes = JsonSerializer.Serialize<List<ClientGroupModel>>(ls);

            await Clients.Client(cid).SendAsync("ReceiveGroupList", ls);


        }

        public async Task RemoveFromGroup(string userID, string removeUserID, string groupID)
        {
            await ReconnectUser(userID);
            //Find group from group list or return Null to optimise my code instead of Check if group exist then Find it afterward from Group list.
            var foundGroup = program.users.Groups.Exists(g => g.GroupID == groupID) ? program.users.Groups.Find(g => g.GroupID == groupID) : null;
            //Check if both group and user exist in specific gorup
            if(foundGroup != null && foundGroup.Users.Exists(u => u.UserID == removeUserID))
            {
                //Find user data from user list
                var foundUser = program.users.Users.Find(u => u.UserID == removeUserID);
                //Remove user from group in group list and it's connection to group to ensure the user's  non access to group.
                program.users.Groups.Find(g => g.GroupID == groupID).Users.RemoveAll(u => u.UserID == foundUser.UserID);
                var user = program.users.Users.Find(u => u.UserID == userID);

                DbGroups db = new DbGroups();
                await db.RemoveUserFromGroup(foundUser.UserID, foundGroup.GroupID);

                await Clients.Group(groupID).SendAsync("RemovedUserCheck"+groupID, foundUser.UserID, foundUser.Name,user.Name);
                //remove user connection
                foreach (string item in foundUser.ConnectionID)
                {   
                    await Groups.RemoveFromGroupAsync(item, foundGroup.GroupID);

                }
            }
        }
        //Add player to group and/or create group and put yourself into the group
        public async Task AddToGroup(string groupName, string userID, string groupID = null, string friendID = null)
        {
            await ReconnectUser(userID);

            DbGroups db = new DbGroups();

            //Check if user is logged in
            if (program.users.Users.Exists(u => u.IsConnected == true && u.UserID == userID))
            {
                //Friend or Client
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
                    //check if user exist
                    if (program.users.Users.Exists(u => u.UserID == friendID))
                    {

                        var friendUser = program.users.Users.Find(u => u.UserID == friendID);

                        //Check if group exist
                        if (program.users.Groups.Exists(g => g.GroupID == groupID))
                        {
                            //check if user doesn't exist in group 


                            var group = program.users.Groups.Find(g => g.GroupID == groupID);
                            if (!group.Users.Exists(u => u.UserID == friendID))
                            {

                                //Add user to group
                                program.users.Groups.Find(g => g.GroupID == groupID).Users.Add(friendUser);
                                await db.AddUserToGroup(friendID, groupID);
                                if (friendUser.IsConnected)
                                {
                                    foreach (string item in friendUser.ConnectionID)
                                    {
                                        await Groups.AddToGroupAsync(item, groupID);

                                    }

                                }
                            }

                        }


                    }
                }

            }

        }

        public async Task Search(string groupID, string userID, string searchVal)
        {
            await ReconnectUser(userID);
            if (program.users.Users.Exists(u => u.UserID == userID && u.IsConnected == true))
            {
                DbGetUsers db = new DbGetUsers();
                var group = program.users.Groups.Find(g => g.GroupID == groupID);
                var ls = await db.GetUsers(group, userID, searchVal);
                List<FriendsModel> newls = new List<FriendsModel>();

                newls = ls;

                string cid = Context.ConnectionId;
                await Clients.Client(cid).SendAsync("ReceiveSearchUsers", ls,db.HasUsers);

            }
        }

        //-------------------------------------------------------------------------------------------------- Log in and Log Out Methods
        public async Task LogIn(string email, string password)
        {

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


                stringUser[0] = user.Name;
                stringUser[1] = user.Email;
                stringUser[2] = user.Password;
                stringUser[3] = user.UserID;

                //add user to users list
                //If user exist delete existing user and re add user with the new data from Database because the existing data could've been deleted or corrupted.
                //Remove automatically check if item exist.
                program.users.Users.RemoveAll(u => u.UserID == user.UserID);


                user.IsConnected = true;
                program.users.Users.Add(user);

                var group = program.users.Groups.Find(g => g.Users.Exists(u => u.UserID == user.UserID));
            }
            else
            {
                stringUser[3] = "0";
            }

            await Clients.Client(cid).SendAsync("ReceiveAccount", stringUser);
        }

        public void LogOut(string userID)
        {
            
            program.users.Users.Find(u => u.UserID == userID).IsConnected = false;
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


            if (program.users.Users != null)
            {

                if (program.users.Users.Exists(u => u.UserID == userID))
                {
                    string cid = Context.ConnectionId;

                    //Check if user have this connection id in its list because a client can have multiple connections from multiple devices on a one-to-one connection.
                    if (!program.users.Users.Find(u => u.UserID == userID).ConnectionID.Contains(cid))
                    {
                        program.users.Users.Find(u => u.UserID == userID).ConnectionID.Add(cid);
                    }

                    //Reconnect to all subscripted Groups and makes it possible to connect from multiple devices
                    foreach (var item in program.users.Groups.Where(g => g.Users.Exists(u => u.UserID == userID)))
                    {
                        await Groups.AddToGroupAsync(cid, item.GroupID);
                    }
                }
            }



        }




    }
}
