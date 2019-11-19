using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using ChatAppV3.HubClientCon;
using ChatAppV3.Views;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ChatAppV3.ViewModels
{
    class RegisterVM : INotifyPropertyChanged
    {

        public RegisterVM()
        {
            IsError = false;
            IsRedirect = false;
            HubConnClient hub = new HubConnClient();
            hubConnection = hub.HubConn();

            LoginCommand = new Command(async () =>
            {

                await Application.Current.MainPage.Navigation.PopModalAsync();
            });

            RegisterCommand = new Command(async () =>
            {
                if (!string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password))
                {

                    await RegisterUser(Name, Email, Password);
                    IsRedirect = true;

                }
                else
                {
                    IsError = true;
                    ErrorMsg = "Some fields are missing or incorrect";
                }



            });

            hubConnection.On<bool>("ReceieveRegisterMsg", async (IsRegister) =>
            {
                if (IsRegister)
                {

                    await Application.Current.MainPage.Navigation.PopModalAsync();

                    await hubConnection.StopAsync();

                }
                else
                {
                    IsRedirect = false;
                    IsError = true;
                    ErrorMsg = "Something went wrong. Please try again or contact support";

                    await hubConnection.StopAsync();

                }
            });
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private HubConnection hubConnection;

        private bool isRedirect;
        private bool isError;
        private string errorMsg;
        private string email;
        private string name;
        private string password;

        public Command LoginCommand { get; }
        public Command RegisterCommand { get; }

        public bool IsRedirect
        {
            get => isRedirect;
            set
            {
                isRedirect = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRedirect)));
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
        public string ErrorMsg
        {
            get => errorMsg;
            set
            {
                errorMsg = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMsg)));
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

        public string Name
        {
            get => name;
            set
            {
                name = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
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

        public async Task RegisterUser(string name, string email, string password)
        {
            await hubConnection.StartAsync();

            await hubConnection.InvokeAsync("Register", name, email, password);

        }


    }
}
