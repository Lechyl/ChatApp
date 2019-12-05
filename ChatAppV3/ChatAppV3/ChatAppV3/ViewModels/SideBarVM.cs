using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;
using ChatAppV3.HubClientCon;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using ChatAppV3.Models;
using ChatAppV3.Views;
using System.Text.Json;

namespace ChatAppV3.ViewModels
{
    class SideBarVM : HubConnClient,INotifyPropertyChanged
    {
      
        public SideBarVM()
        {
            StartOptions();
            ShowGroupList = true;
            Error = false;

            AddGroupCommand = new Command(async () =>
            {
                if (!string.IsNullOrWhiteSpace(GroupName))
                {
                    await AddGroupOrUserToGroup(GroupName,user.UserID);

                }
                else
                {
                    Error = true;
                }
            });

            ShowAddGroupCommand = new Command(() =>
            {
                //GroupName = user.Email + user.ConnectionID;
                

                ShowGroupList = !ShowGroupList;
            });

            LogOutCommand = new Command(async () =>
            {
                await LogOut();
            });

        }

        



        private string groupName;
        private bool showGroupList;
        private bool error;
        private UserModel user;

        public event PropertyChangedEventHandler PropertyChanged;

        public Command LogOutCommand { get; }

        public Command ShowAddGroupCommand { get; }

        public Command AddGroupCommand { get; }
        public bool Error
        {
            get => error;
            set
            {
                error = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Error)));
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

        public string GroupName
        {
            get => groupName;
            set
            {
                groupName = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GroupName)));
            }
        }




        public async Task LogOut()
        {
            Application.Current.Properties.Clear();
            //await Application.Current.SavePropertiesAsync();
            /*LoginVM viewModel = new LoginVM();
            LoginPage page = new LoginPage();

            page.BindingContext = viewModel;*/
            await DisconnectAsync();

            await Application.Current.MainPage.Navigation.PopModalAsync();
        }
        public async Task AddGroupOrUserToGroup(string groupName, string userID)
        {
            await ConnectAsync();
            await hub.InvokeAsync("AddToGroup", groupName, userID, null, null);
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
