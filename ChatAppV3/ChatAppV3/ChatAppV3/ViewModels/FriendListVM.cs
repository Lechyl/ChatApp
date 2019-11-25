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
            GroupName += "D ";
            StartOptions();

            Reconnect = false;
            ShowGroupList = true;
            Error = false;

            groups = new ObservableCollection<GroupListModel>();
            GetFriendList(user.UserID);


            ReconnectCommand = new Command(async () =>
            {

                Reconnect = true;

            });

            AddGroupCommand = new Command(async () =>
            {
                if (!string.IsNullOrWhiteSpace(GroupName))
                {
                    await AddGroupOrUserToGroup(GroupName, user.UserID);

                }
                else
                {
                    Error = true;
                }
            });

            ShowAddGroupCommand = new Command( () =>
            {
                //GroupName = user.Email + user.ConnectionID;
                ShowGroupList = !ShowGroupList;
            });

            LogOutCommand = new Command(async() =>
            {
                await LogOut();
            });

            hub.On<List<string>>("ReceiveGroupList", (ls) => {

                GroupName = "a";
                foreach (var item in ls)
                {
                    groups.Add(new GroupListModel() { groupName = item });
                    GroupName += item;
                }
               
                /*foreach (var groupName in ls)
                {
                    groups.Add(groupName);
                }*/

            });



        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private UserModel user;

        private bool error;
        private bool showGroupList;
        private string groupName;
        private bool reconnect;
        public Command AddGroupCommand { get; }
        public Command ShowAddGroupCommand { get; }
        public Command LogOutCommand { get; }

        public Command ReconnectCommand { get; }
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

        public bool Reconnect
        {
            get => reconnect;
            set
            {
                reconnect = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Reconnect)));
            }
        }



        public bool Error
        {
            get => error;
            set
            {
                error = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Error)));
            }
        }
        public string GroupName
        {
            get => groupName;
            set
            {
                groupName = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GroupName)));
            }
        }
        public bool ShowGroupList
        {
            get => showGroupList;
            set
            {
                showGroupList = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowGroupList)));
            }
        }
        public async Task GetFriendList(string userID)
        {
            await hub.StartAsync();
            await hub.InvokeAsync("MyGroupList", userID);
        }

        public async Task AddGroupOrUserToGroup(string groupName,string userID)
        {

            await hub.InvokeAsync("AddToGroup", groupName, userID);
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



        private async Task OnReconnect()
        {
            await _semaphore.WaitAsync();
            try
            {
                await hub.StartAsync();

                await hub.InvokeAsync("ReconnectUser", user.UserID);
                GroupName += "A ";

            }
            catch (Exception)
            {

                throw;
            }

            
        }

        public async Task LogOut()
        {
            Application.Current.Properties.Clear();
            await Application.Current.SavePropertiesAsync();
            LoginVM viewModel = new LoginVM();
            LoginPage page = new LoginPage();

            page.BindingContext = viewModel;

            await Application.Current.MainPage.Navigation.PushModalAsync(page);
            await hub.StopAsync();
        }


    }
}
