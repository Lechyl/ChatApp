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
    class ChatPageVM : HubConnClient, INotifyPropertyChanged
    {

        public ChatPageVM()
        {
            //Instantiate
            StartOptions();
            IsSearching = false;
            HasUsers = true;
            Messages = new ObservableCollection<MessageModel>();
            Friends = new ObservableCollection<FriendsModel>();

            Task.Run(async () => {
                await Connect();
            });


            ViewAllUsersInGroup = new Command(() =>
            {
                Device.BeginInvokeOnMainThread(async() =>
                {
                    await Application.Current.MainPage.Navigation.PushModalAsync(new GroupViewPage(), true);

                });

            });

            SearchCommand = new Command(async () => {
                await SearchUsersByName(currentGroup.groupID, user.UserID, SearchName);
            });

            ShowSearch = new Command(() =>
            {
                //Clickable button to disable/enable
                Friends.Clear();
                SearchUnderText = string.Empty;
                IsSearching = !IsSearching;

            });
            SendMessageCommand = new Command(async () =>
            {

                await SendMessage(currentGroup.groupID, Message, Name, user.UserID);
                // Empty Message
                Message = string.Empty;
            });



            DisconnectCommand = new Command(async () =>
            {
                //Pop Modal Page
                await Application.Current.MainPage.Navigation.PopModalAsync();
            });

            AddUserCommand = new Command<FriendsModel>(async (FriendsModel friend) => {

                //Confirm Alertbox
                bool confirm = await Application.Current.MainPage.DisplayAlert("Notification", $"Do you want to add {friend.Name} to the group?", "Yes", "Cancel");

                if (confirm)
                {
                    await AddUserToGroup(currentGroup.groupName, user.UserID, currentGroup.groupID, friend.FriendID);

                }
            });

            hub.On<string, string>("ReceiveMessage" + currentGroup.groupID, (user, message) =>
            {
                //Add a message to the Message collection as the first element on the list
                Messages.Insert(0, new MessageModel()
                {
                    User = user,
                    Message = message,
                    IsSystemMessage = false,
                    IsOwnMessage = Name == user
                });
            });

            hub.On<List<MessageModel>>("JoinChat" + currentGroup.groupID, (ls) =>
            {
                //Clear list and insert new data
                Messages.Clear();
                foreach (var u in ls)
                {
                    Messages.Insert(0, new MessageModel()
                    {

                        User = u.User,
                        Message = u.Message,
                        IsOwnMessage = u.IsOwnMessage,
                        IsSystemMessage = u.IsSystemMessage

                    });
                }

            });

            hub.On<List<FriendsModel>,bool>("ReceiveSearchUsers", (ls,hasUsers) =>
            {
                //Clear List and insert new data
                Friends.Clear();
                HasUsers = hasUsers;
                SearchUnderText = "Search Result Not Found";

                if (hasUsers)
                {
                    SearchUnderText = "Search Result Found";

                    foreach (var item in ls)
                    {
                        Friends.Add(new FriendsModel()
                        {
                            FriendID = item.FriendID,
                            Name = item.Name
                        });
                    }
                }


            });

            hub.On<string, string,string>("RemovedUserCheck"+currentGroup.groupID,async (removedID, removedName,username) =>
             {
                 if(user.UserID == removedID)
                 {
                     await Application.Current.MainPage.Navigation.PopModalAsync();
                 }
                 else
                 {
                     Messages.Add(new MessageModel()
                     {
                         IsOwnMessage = false,
                         IsSystemMessage = true,
                         Message = $"{removedName} has been kicked by {username}!"
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
        public Command ViewAllUsersInGroup { get; }
        private GroupListModel currentGroup;
        private UserModel user;
        private string groupName;
        private string _name;
        private string _message;
        private string _searchName;
        private string _searchUnderText;

        private ObservableCollection<MessageModel> _messages;
        private ObservableCollection<FriendsModel> _friends;
        private bool isSearching;
        private bool hasUsers;


        public string SearchUnderText
        {
            get => _searchUnderText;
            set
            {
                _searchUnderText = value;
                OnPropertyChanged();
            }
        }
        public bool HasUsers
        {
            get => hasUsers;
            set
            {
                hasUsers = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasUsers)));
            }
        }
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
            //Invoke/Raise hub method, It'll raise/run a specific method in the hub

            await hub.InvokeAsync("AddToGroup", groupName, userID, groupID, friendID);
        }
        async Task SearchUsersByName(string groupID, string userID, string search)
        {
            //Invoke/Raise hub method, It'll raise/run a specific method in the hub

            await hub.InvokeAsync("Search", groupID, userID, search);
        }


        async Task Connect()
        {
            //Start Connection to the hub
            await ConnectAsync();

            //Invoke/Raise hub method, It'll raise/run a specific method in the hub

            await hub.InvokeAsync("JoinChatRoom", user.UserID, currentGroup.groupID);

        }

        async Task SendMessage(string groupID, string message, string user, string userID)
        {
            //Invoke/Raise hub method, It'll raise/run a specific method in the hub
            await hub.InvokeAsync("SendMsgToGroup", groupID, message, user, userID);
        }

        async Task Disconnect()
        {
            //Invoke/Raise hub method, It'll raise/run a specific method in the hub
            await hub.InvokeAsync("LeaveChat",user.UserID, currentGroup.groupID);
        }

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            //Invoke/Raise Event of the specific method name

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        public void StartOptions()
        {

            //Get all User Data from Cache/Local DB
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
