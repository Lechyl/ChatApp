using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;
using ChatAppV3.Models;

namespace ChatAppV3.ViewModels
{
    class ChatPageVM : INotifyPropertyChanged
    {
        //Create Fields
        public event PropertyChangedEventHandler PropertyChanged;

        private HubConnection hubConnection;
        public Command SendMessageCommand { get; }
        public Command ConnectCommand { get; }
        public Command DisconnectCommand { get; }

        private string _name;
        private string _message;
        private ObservableCollection<MessageModel> _messages;
        private bool _isConnected;

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
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }



        public ChatPageVM()
        {
            //Instantiate
            Messages = new ObservableCollection<MessageModel>();

            SendMessageCommand = new Command(async () => 
            { 
                
                //await SendMessage(groupName,Name, Message);
                Message = string.Empty;
            });

            ConnectCommand = new Command(async () => 
            {
                await Connect();
            });

            DisconnectCommand = new Command(async () =>
            {
                await Disconnect();
            });

            IsConnected = false;

            hubConnection = new HubConnectionBuilder()
         .WithUrl($"http://10.142.69.153:5565/chathub")
         .Build();


            //Listening to the hub with specific method
            hubConnection.On<string>("JoinChat", (user) =>
            {
                //Add a message to the Message collection
                Messages.Add(new MessageModel() 
                { 
                    User = Name, 
                    Message = $"{user} has joined the chat",
                    IsSystemMessage = true 
                });
            });

            hubConnection.On<string>("LeaveChat", (user) =>
            {
                //Add a message to the Message collection
                Messages.Add(new MessageModel() 
                { 
                    User = Name, 
                    Message = $"{user} has left the chat", 
                    IsSystemMessage = true 
                });
            });

            hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                //Add a message to the Message collection
                Messages.Add(new MessageModel() 
                { 
                    User = user,
                    Message = message, 
                    IsSystemMessage = false, 
                    IsOwnMessage = Name == user 
                });
            });


        }

        //Send data to the hub
        async Task Connect()
        {
            //Start Connection to the hub

            //Invoke/Raise hub method, It'll raise/run a specific method in the hub
            await hubConnection.InvokeAsync("JoinChat", Name);
            
            IsConnected = true;
        }

        async Task SendMessage(string groupName,string user, string message)
        {
            //Invoke/Raise hub method, It'll raise/run a specific method in the hub
            await hubConnection.InvokeAsync("SendMsgToGroup", groupName ,user, message);
        }

        async Task Disconnect()
        {
            //Invoke/Raise hub method, It'll raise/run a specific method in the hub
            await hubConnection.InvokeAsync("LeaveChat", Name);

            //Stop  Connection to the hub
            await hubConnection.StopAsync();

            //trigger IsConnected event, to disable display to chat
            IsConnected = false;
        }

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            //Invoke/Raise Event of the specific method name
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //PropertyChangedEventHandler handler = PropertyChanged;
            //if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
