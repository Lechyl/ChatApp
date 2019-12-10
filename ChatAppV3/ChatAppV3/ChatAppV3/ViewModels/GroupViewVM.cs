using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChatAppV3.HubClientCon;
using ChatAppV3.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Xamarin.Forms;

namespace ChatAppV3.ViewModels
{
    class GroupViewVM : HubConnClient, INotifyPropertyChanged
    {
        public GroupViewVM()
        {

            UsersInGroup = new ObservableCollection<FriendsModel>();
            StartOptions();
            Task.Run(async() =>
            {
               await ConnectAsync();
                await ViewAllUsersInGroup(user.UserID, currentGroup.groupID);

            });

            KickUserCommand = new Command<FriendsModel>(async(friend) =>
            {

                bool answer = await Application.Current.MainPage.DisplayAlert("", $"Do you want to kick {friend.Name} out of the group?", "Kick","Cancel");
                if (answer)
                {
                    UsersInGroup.Remove(friend);
                    await RemoveUserFromGroup(user.UserID, friend.FriendID, currentGroup.groupID);

                }
            });

            GoBack = new Command(async() =>
            {

                await Application.Current.MainPage.Navigation.PopModalAsync();
            });

            hub.On<List<FriendsModel>>("ReceiveAllUsersInGroup", (ls) =>
            {
                foreach (var item in ls)
                {
                    UsersInGroup.Add(new FriendsModel()
                    {
                        FriendID = item.FriendID,
                        Name = item.Name
                    });
                }
            });

        }
        public event PropertyChangedEventHandler PropertyChanged;
        public Command KickUserCommand { get; }
        public Command GoBack { get; }
        private ObservableCollection<FriendsModel> usersInGroup;
        private UserModel user;
        private GroupListModel currentGroup;

        public ObservableCollection<FriendsModel> UsersInGroup
        {
            get => usersInGroup;
            set
            {
                usersInGroup = value;
                OnPropertyChanged();
            }
        }

        protected async Task ViewAllUsersInGroup(string userID,string groupID)
        {
            await hub.InvokeAsync("ViewAllUsersInGroup", userID, groupID);
        }

        protected async Task RemoveUserFromGroup(string userID,string removeUserID,string groupID)
        {
            await hub.InvokeAsync("RemoveFromGroup", userID, removeUserID, groupID);
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
                


                var groupJson = Application.Current.Properties["CurrentGroup"];
                var groupDes = JsonSerializer.Deserialize<GroupListModel>(groupJson.ToString());

                currentGroup = new GroupListModel()
                {
                    groupName = groupDes.groupName,
                    groupID = groupDes.groupID
                };

            }

        }

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            //Invoke/Raise Event of the specific method name

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
    }
}
