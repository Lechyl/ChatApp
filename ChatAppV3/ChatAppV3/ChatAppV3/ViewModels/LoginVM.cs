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

namespace ChatAppV3.ViewModels
{
    class LoginVM : INotifyPropertyChanged
    {
        public LoginVM()
        {
            IsError = false;


            // hubConnection = new HubConnectionBuilder()
            //   .WithUrl($"http://172.16.3.63:5565/chathub")
            // .Build();
            HubConnClient hub = new HubConnClient();
            hubConnection = hub.HubConn();

            hubConnection.On<string[]>("ReceiveAccount", async (arr) =>
            {
                if (int.Parse(arr[4]) > 0)
                {

                    UserModel userModel = new UserModel();
                    userModel.Name = arr[0];
                    userModel.Email = arr[1];
                    userModel.Password = arr[2];
                    userModel.ConnectionID = arr[3];
                    userModel.UserID = arr[4];

                    FriendListVM viewModel = new FriendListVM();

                    FriendListPage page = new FriendListPage();

                    page.BindingContext = viewModel;

                    await Application.Current.MainPage.Navigation.PushModalAsync(page);
                }
                else
                {
                    IsError = true;
                    ErrorMsg = "Email and/or Password are incorrect!";
                    await hubConnection.StopAsync();
                }
            });

            LoginCommand = new Command(async() =>
            {
                await hubConnection.StartAsync();
                await Login(Email, Password);
            });

            RegisterCommand = new Command(async () =>
            {
                RegisterVM registerVM = new RegisterVM();

                RegisterPage registerPage = new RegisterPage();

                registerPage.BindingContext = registerVM;
                await hubConnection.StopAsync();
                await Application.Current.MainPage.Navigation.PushModalAsync(registerPage);
            });
        }

        private bool isError;
        private string errorMsg;
        private HubConnection hubConnection; 
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
            await hubConnection.InvokeAsync("LogIn", email, password);
        }


    }
}
