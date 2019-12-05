using ChatAppV3.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using ChatAppV3.Models;
using System.Threading.Tasks;
using ChatAppV3.HubClientCon;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatAppV3.ViewModels
{
    class LoginVM : HubConnClient, INotifyPropertyChanged
    {
        public LoginVM()
        {
            IsError = false;



            LoginCommand = new Command(async () =>
            {
                await ConnectAsync();

                if (!string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password))
                {
                    await Login(Email, Password);
                }
                else
                {

                    IsError = true;
                    ErrorMsg = "Email and/or Password are incorrect!";
                }


            });

            RegisterCommand = new Command(async () =>
            {
                //Navigate to Modal Page
                await Application.Current.MainPage.Navigation.PushModalAsync(new RegisterPage(), false);

            });

            hub.On<string[]>("ReceiveAccount", async (arr) =>
            {

                if (int.Parse(arr[3]) > 0)
                {
                    //Receive User Data from DB/Hub
                    UserModel user = new UserModel()
                    {
                        Name = arr[0],
                        Email = arr[1],
                        Password = arr[2],
                        UserID = arr[3]


                    };

                    //Serialize Object to json because the storage only takes primitive types
                    var userJSON = JsonSerializer.Serialize<UserModel>(user);
                    if (Application.Current.Properties.ContainsKey("UserData"))
                    {
                        Application.Current.Properties["UserData"] = userJSON;

                    }
                    else
                    {
                        //add key/pair to storage

                        Application.Current.Properties.Add("UserData", userJSON);

                    }
                    //Save changes
                    await Application.Current.SavePropertiesAsync();

                    //Create MasterDetailPage
                    //MasterDetailPage is two pages with Sidebar functionality.
                    //Detail is the Root Page
                    //Master is the page/Sidebar you call when you click on Menu Icon
                    var page = new MasterDetailPage()
                    {
                        Master = new SideBarPage() { Title = "Main Page" },
                        Detail = new NavigationPage(new FriendListPage()) { BarBackgroundColor = Color.FromHex("#2ac2c5") }

                    };

                    //Navigate to Modal page
                    await Application.Current.MainPage.Navigation.PushModalAsync(page, false);

                }
                else
                {

                    IsError = true;
                    ErrorMsg = "Email and/or Password are incorrect!";

                }
            });


        }

        private bool isError;
        private string errorMsg;
        public Command RegisterCommand { get; }
        public Command LoginCommand { get; set; }
        private string email;
        private string password;
        public event PropertyChangedEventHandler PropertyChanged;

        public string ErrorMsg
        {
            get => errorMsg;
            set
            {
                errorMsg = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMsg)));
            }
        }

        public bool IsError
        {
            get => isError;
            set
            {
                isError = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsError)));
            }
        }

        public string Email
        {
            get => email;
            set
            {
                email = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Email)));
            }
        }
        public string Password
        {
            get => password;
            set
            {
                password = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password)));

            }
        }

        async Task Login(string email, string password)
        {
            await hub.InvokeAsync("LogIn", email, password);
        }

    }
}
