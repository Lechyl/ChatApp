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
        public List<UserModel> users = new List<UserModel>();


        //-------------------------------------------------------------------------------------------------- Test CHat Methods
        public async Task JoinChat(string user)
        {
            await Clients.All.SendAsync("JoinChat", user);
        }

        public async Task LeaveChat(string user)
        {
            await Clients.All.SendAsync("LeaveChat", user);
        }
        public async Task SendMessageAll(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        //-------------------------------------------------------------------------------------------------- Single Chat Methods
        public async Task SendMessageClient(string user, string message, string toUserID)
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

        //-------------------------------------------------------------------------------------------------- Group Methods
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }


        //-------------------------------------------------------------------------------------------------- Log in and Log Out Methods
        public async Task LogIn(string email, string password)
        {
            Login login = new Login();

            string cid = Context.ConnectionId;
            string[] arr = await login.VerifyLogin(email, password, cid);

            UserModel user = new UserModel(arr[0],arr[3],arr[4]);
            users.Add(user);

            await Clients.Client(cid).SendAsync("ReceiveAccount",arr);
        }

        public void LogOut(string userID)
        {
             users.RemoveAll(user => user.UserID == userID);
        }

        //-------------------------------------------------------------------------------------------------- Register Method

        public async Task Register(string name, string email, string password)
        {
            
            string cid = "New User";
            string tempCid = Context.ConnectionId;

            Register register = new Register();
            bool IsRegistered = await register.RegisterUser(name, email, password, cid);

            await Clients.Client(tempCid).SendAsync("ReceieveRegisterMsg", IsRegistered);

            ;

        }


    }
}
