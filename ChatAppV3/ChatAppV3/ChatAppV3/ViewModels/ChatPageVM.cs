using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;
using ChatAppV3.Models;
using ChatAppV3.Views;
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
            IsSearching = false;
            Messages = new ObservableCollection<MessageModel>();
            Friends = new ObservableCollection<FriendsModel>();
            
            Task.Run(async () => {
                await Connect();
            });

            SearchCommand = new Command(async() => {
                await SearchUsersByName(currentGroup.groupID ,user.UserID, SearchName);
            
            });

            ShowSearch = new Command(() =>
            {

                IsSearching = !IsSearching;

            });
            SendMessageCommand = new Command(async () =>
            {

                await SendMessage(currentGroup.groupID, Message, Name, user.UserID);
                Message = string.Empty;
            });

            

            DisconnectCommand = new Command(async () =>
            {
                //await Application.Current.MainPage.DisplayAlert("ss", "Disconnect", "ok");
                await Application.Current.MainPage.Navigation.PopModalAsync();
            });

            AddUserCommand = new Command<FriendsModel>(async (FriendsModel friend) => {
                bool confirm = await Application.Current.MainPage.DisplayAlert("Notification", $"Do you want to add {friend.Name} to the group?", "Yes","Cancel");

                if (confirm)
                {
                    await AddUserToGroup(currentGroup.groupName, user.UserID, currentGroup.groupID, friend.FriendID);

                }
            });


            //Listening to the hub with specific method
            /*hub.On<string>("JoinChat" + currentGroup.groupID, (user) =>
            {

                //Add a message to the Message collection
                Messages.Add(new MessageModel()
                {
                    User = Name,
                    Message = $"{user} has joined the chat",
                    IsSystemMessage = true
                });
            });
            */
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
                Messages.Insert(0,new MessageModel()
                {
                    User = user,
                    Message = message,
                    IsSystemMessage = false,
                    IsOwnMessage = Name == user
                });
            });

            hub.On<List<MessageModel>>("JoinChat"+ currentGroup.groupID, (ls) => 
            {

                Messages.Clear();
                    foreach (var u in ls)
                    {
                        Messages.Insert(0,new MessageModel()
                        {

                            User = u.User,
                            Message = u.Message,
                            IsOwnMessage = u.IsOwnMessage,
                            IsSystemMessage = u.IsSystemMessage

                        });
                    }
            
            });

            hub.On<List<FriendsModel>>("ReceiveSearchUsers", (ls) =>
            {

                Friends.Clear();
                foreach (var item in ls)
                {
                    Friends.Add(new FriendsModel() { 
                        FriendID = item.FriendID,
                        Name = item.Name
                    });
                }

            });



        }


        //Create Fields
        public event PropertyChangedEventHandler PropertyChanged;
        public Command ShowSearch { get; }
        public Command SendMessageCommand { get; }
        public Command DisconnectCommand { get; }
        public Command AddUserCommand { get; }
        public Command SearchCommand { get; }
        private GroupListModel currentGroup;
        private UserModel user;
        private string groupName;
        private string _name;
        private string _message;
        private string _searchName;
        private ObservableCollection<MessageModel> _messages;
        private ObservableCollection<FriendsModel> _friends;
        private bool isSearching;

        public ObservableCollection<FriendsModel> Friends
        {
            get => _friends;
            set
            {
                _friends = value;
                OnPropertyChanged();
            }
        }
        public string SearchName
        {
            get => _searchName;
            set
            {
                _searchName = value;
                OnPropertyChanged();
            }
        }
        public bool IsSearching
        {
            get => isSearching;
            set
            {
                isSearching = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSearching)));
            }
        }

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

        async Task AddUserToGroup(string groupName, string userID, string groupID, string friendID)
        {
            await hub.InvokeAsync("AddToGroup", groupName, userID, groupID, friendID);
        }
        async Task SearchUsersByName(string groupID, string userID, string search)
        {
            await hub.InvokeAsync("Search",groupID, userID, search);
        }
        
   
        async Task Connect()
        {
            //Start Connection to the hub
            await ConnectAsync();

             await hub.InvokeAsync("JoinChatRoom", user.UserID, currentGroup.groupID);

            // await hub.InvokeAsync("GetMessages",user.UserID, currentGroup.groupID);

            //Invoke/Raise hub method, It'll raise/run a specific method in the hub

        }

        async Task SendMessage(string groupID,string message, string user, string userID)
        {
            //Invoke/Raise hub method, It'll raise/run a specific method in the hub
            await hub.InvokeAsync("SendMsgToGroup", groupID, message, user, userID );
        }

        async Task Disconnect()
        {
            //Invoke/Raise hub method, It'll raise/run a specific method in the hub
            await hub.InvokeAsync("LeaveChat", Name,user.UserID,currentGroup.groupID);

            //Stop  Connection to the hub

            //trigger IsConnected event, to disable display to chat
        } 

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
