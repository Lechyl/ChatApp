using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;
using ChatAppV3.Models;
using System.Text.Json;
using ChatAppV3.HubClientCon;
using System.Collections.Generic;

namespace ChatAppV3.ViewModels
{
    class ChatPageVM :  HubConnClient,INotifyPropertyChanged
    {


        public ChatPageVM()
        {
            //Instantiate
            StartOptions();

            Messages = new ObservableCollection<MessageModel>();

            Task.Run(async () => {
                await Connect();
            });

            SendMessageCommand = new Command(async () =>
            {

                await SendMessage(currentGroup.groupID, Message, Name, user.UserID);
                Message = string.Empty;
            });



            DisconnectCommand = new Command(async () =>
            {
                //await Disconnect();
                await Application.Current.MainPage.Navigation.PopModalAsync();
            });

            AddUserCommand = new Command(async () => { 
            
            
            });


            //Listening to the hub with specific method
            hub.On<string>("JoinChat" + currentGroup.groupID, (user) =>
            {
                //Add a message to the Message collection
                Messages.Add(new MessageModel()
                {
                    User = Name,
                    Message = $"{user} has joined the chat",
                    IsSystemMessage = true
                });
            });

            /*hub.On<string>("LeaveChat" + currentGroup.groupID, (user) =>
            {
                //Add a message to the Message collection
                Messages.Add(new MessageModel()
                {
                    User = Name,
                    Message = $"{user} has left the chat",
                    IsSystemMessage = true
                });
            });*/

            hub.On<string, string>("ReceiveMessage"+ currentGroup.groupID, (user, message) =>
            {
                //Add a message to the Message collection
                Messages.Add(new MessageModel()
                {
                    User = user,
                    Message = message,
                    IsSystemMessage = false,
                    IsOwnMessage = Name == user
                });
            });

            hub.On<List<MessageModel>>("ReceiveDBMessages" + currentGroup.groupID, (ls) => 
            {
                
                foreach (var u in ls)
                {
                    Messages.Add(new MessageModel()
                    {
                        
                        User = u.User,
                        Message = u.Message,
                        IsOwnMessage = u.IsOwnMessage,
                        IsSystemMessage = u.IsSystemMessage

                    });
                }


            
            });

        }


        //Create Fields
        public event PropertyChangedEventHandler PropertyChanged;

        public Command SendMessageCommand { get; }
        public Command DisconnectCommand { get; }
        public Command AddUserCommand { get; }
        private GroupListModel currentGroup;
        private UserModel user;
        private string groupName;
        private string _name;
        private string _message;
        private ObservableCollection<MessageModel> _messages;


        public string GroupName
        {
            get => groupName;
            set
            {
                groupName = value;
                OnPropertyChanged();
            }
        }
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<MessageModel> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                OnPropertyChanged();
            }
        }



        

        //Send data to the hub

        
        async Task AddUserToChat(string groupName, string userID, string groupID, string friendID)
        {
            await hub.InvokeAsync("AddToGroup", groupName, userID, groupID, friendID);
        }
        async Task Connect()
        {
            //Start Connection to the hub
            await ConnectAsync();
             await hub.InvokeAsync("GetMessages",user.UserID, currentGroup.groupID);

            //Invoke/Raise hub method, It'll raise/run a specific method in the hub
            // await hub.InvokeAsync("JoinChat", Name,user.UserID, currentGroup.groupID);

        }

        async Task SendMessage(string groupID,string message, string user, string userID)
        {
            //Invoke/Raise hub method, It'll raise/run a specific method in the hub
            await hub.InvokeAsync("SendMsgToGroup", groupID, message, user, userID );
        }

       /* async Task Disconnect()
        {
            //Invoke/Raise hub method, It'll raise/run a specific method in the hub
            await hub.InvokeAsync("LeaveChat", Name,user.UserID,currentGroup.groupID);

            //Stop  Connection to the hub

            //trigger IsConnected event, to disable display to chat
        } */

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            //Invoke/Raise Event of the specific method name
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //PropertyChangedEventHandler handler = PropertyChanged;
            //if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void StartOptions()
        {


            if (Application.Current.Properties.ContainsKey("UserData"))
            {
                var userJson = Application.Current.Properties["UserData"];
                var userDes = JsonSerializer.Deserialize<UserModel>(userJson.ToString());

                user = new UserModel()
                {
                    Name = userDes.Name,
                    Email = userDes.Email,
                    Password = userDes.Password,
                    UserID = userDes.UserID
                };
                Name = user.Name;


                var groupJson = Application.Current.Properties["CurrentGroup"];
                var groupDes = JsonSerializer.Deserialize<GroupListModel>(groupJson.ToString());

                currentGroup = new GroupListModel()
                {
                    groupName = groupDes.groupName,
                    groupID = groupDes.groupID
                };
                GroupName = currentGroup.groupName;

            }



        }
    }
}
