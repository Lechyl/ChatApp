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
    class SideBarVM : HubConnClient
    {

        public SideBarVM()
        {
            LogOutCommand = new Command(async () =>
            {
                //Display Prompt with yes/no answer
                bool answer = await Application.Current.MainPage.DisplayAlert("Log Out Prompt", "Would you like to Log Out?", "Logout", "Cancel");
                if (answer)
                    await LogOut();
            });

        }
        public Command LogOutCommand { get; }

        public async Task LogOut()
        {
            //Clear Cache/Storage
            Application.Current.Properties.Clear();
            //Close Hub Connection
            await DisconnectAsync();
            //Pop Modal Page
            await Application.Current.MainPage.Navigation.PopModalAsync();
        }
    }
}
