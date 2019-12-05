using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using ChatAppV3.Models;
using ChatAppV3.HubClientCon;
using ChatAppV3.Views;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;


namespace ChatAppV3.ViewModels
{

    class FriendListVM : HubConnClient, INotifyPropertyChanged
    {

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public FriendListVM()
        {
            
            StartOptions();
            IsRefreshed = false;
            if (!IsConnected)
            {
                Reconnect = false;

            }



            Groups = new ObservableCollection<GroupListModel>();
            //GetFriendList(user.UserID);

            Task.Run(async () => {
                await GetFriendList(user.UserID);

            });

            ChatRoomCommand = new Command(async () => {
                // await GoToChat();
                ChatPageVM vm = new ChatPageVM();
                ChatPage page = new ChatPage();
                page.BindingContext = vm;

                await Application.Current.MainPage.Navigation.PushAsync(page, false);

            });

            RefreshCommand = new Command(async () =>
            {
                IsRefreshed = true;

                await GetFriendList(user.UserID);

                IsRefreshed = false;

            });
            ReconnectCommand = new Command(async () =>
            {
               // await GoToChat();
               await ConnectAsync();
                Reconnect = true;


            });



            hub.On<List<GroupListModel>>("ReceiveGroupList", (ls) => {
                Groups.Clear();


                foreach (var item in ls)
                {
                    
                    Groups.Add(new GroupListModel() { groupName = item.groupName, groupID = item.groupID });

                    
                }
               


            });



        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private UserModel user;

        private bool reconnect;
        private bool isRefreshed;


        public Command RefreshCommand { get; }
        public Command ReconnectCommand { get; }
        public Command ChatRoomCommand { get; }
        private ObservableCollection<GroupListModel> groups;

        public ObservableCollection<GroupListModel> Groups
        {
            get => groups;
            set
            {
                groups = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Groups)));
            }

        }
        public bool IsRefreshed
        {
            get => isRefreshed;
            set
            {
                isRefreshed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs((nameof(IsRefreshed))));
            }
        }

        public bool Reconnect
        {
            get => reconnect;
            set
            {
                reconnect = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Reconnect)));
            }
        }






        public async Task GetFriendList(string userID)
        {
            await ConnectAsync();
            await hub.InvokeAsync("MyGroupList", userID);


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


            }



        }
       
    }
}
