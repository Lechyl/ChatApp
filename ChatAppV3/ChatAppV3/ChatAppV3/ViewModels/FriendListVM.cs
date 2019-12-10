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


        public FriendListVM()
        {

            StartOptions();
            Groups = new ObservableCollection<GroupListModel>();

            IsRefreshed = false;
            Error = false;
            IsCheckmarked = false;

            Task.Run(async () => {
                await GetFriendList(user.UserID);

            });


            AddGroupCommand = new Command(async () =>
            {
                //Display Prompt for typing input
                //Create new group
                GroupName = await Application.Current.MainPage.DisplayPromptAsync("Creating Group", "Type the name of your new group", "Create", "Cancel", "Group name", 150);

                if (!string.IsNullOrWhiteSpace(GroupName))
                {
                    //Show Checkmark Icon
                    IsCheckmarked = true;
                    //
                    await AddGroupOrUserToGroup(GroupName, user.UserID);
                    //Refresh Group List and redisplay with updated Group List
                    await GetFriendList(user.UserID);
                }
                else
                {
                    Error = true;
                }

                //Hide checkmark Icon
                IsCheckmarked = false;


            });

            ChatRoomCommand = new Command(async () => {
                ChatPageVM vm = new ChatPageVM();
                ChatPage page = new ChatPage();
                //Binding ViewModel to View
                page.BindingContext = vm;
                //Navigate to page
                await Application.Current.MainPage.Navigation.PushAsync(page);

            });

            RefreshCommand = new Command(async () =>
            {
                //Show Refresh Icon
                IsRefreshed = true;

                await GetFriendList(user.UserID);

                //Hide Refresh ICon
                IsRefreshed = false;

            });




            hub.On<List<GroupListModel>>("ReceiveGroupList", (ls) => {
                //Clear List and Refill List
                Groups.Clear();
                foreach (var item in ls)
                {

                    Groups.Add(new GroupListModel() { groupName = item.groupName, groupID = item.groupID });
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private UserModel user;

        private bool isRefreshed;
        private string groupName;
        private bool error;
        private bool isCheckmarked;
        public Command RefreshCommand { get; }
        public Command ChatRoomCommand { get; }
        public Command AddGroupCommand { get; }

        private ObservableCollection<GroupListModel> groups;



        public bool IsCheckmarked
        {
            get => isCheckmarked;
            set
            {
                isCheckmarked = value;
                //Create event and Raising it.
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCheckmarked)));
            }
        }
        public bool Error
        {
            get => error;
            set
            {
                error = value;

                //Create event and Raising it.
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Error)));
            }
        }


        public string GroupName
        {
            get => groupName;
            set
            {
                groupName = value;
                //Create event and Raising it.
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GroupName)));
            }
        }

        public ObservableCollection<GroupListModel> Groups
        {
            get => groups;
            set
            {
                groups = value;
                //Create event and Raising it.
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Groups)));
            }

        }
        public bool IsRefreshed
        {
            get => isRefreshed;
            set
            {
                isRefreshed = value;
                //Create event and Raising it
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs((nameof(IsRefreshed))));
            }
        }

        public async Task AddGroupOrUserToGroup(string groupName, string userID)
        {
            await ConnectAsync();
            //Invoke/Raise hub method, It'll raise/run a specific method in the hub
            await hub.InvokeAsync("AddToGroup", groupName, userID, null, null);
        }

        public async Task GetFriendList(string userID)
        {
            await ConnectAsync();
            //Invoke/Raise hub method, It'll raise/run a specific method in the hub
            await hub.InvokeAsync("MyGroupList", userID);


        }

        public void StartOptions()
        {
            //Get User data from Cache/Local DB
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
