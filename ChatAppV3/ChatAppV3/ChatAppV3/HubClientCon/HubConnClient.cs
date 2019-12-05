using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Xamarin.Forms;

namespace ChatAppV3.HubClientCon
{
    class HubConnClient
    {
        public HubConnection hub;
        public bool IsConnected { get; set; }
        public HubConnClient()
        {
            hub = new HubConnectionBuilder()
                .WithUrl($"http://172.16.3.54:5565/chathub")
                    .WithAutomaticReconnect()
                        .Build();
            hub.Closed += async (error) =>
            {
                IsConnected = false;
                try
                {
                    bool reconnected  = ConnectAsync().Wait(5000);
                    
                    if (!reconnected)
                    {
                      await  DisconnectAsync();

                      await Application.Current.MainPage.Navigation.PopModalAsync(false);
                        
                    }
                }
                finally 
                {
                    await DisconnectAsync();


                }
            };

        }
        public async Task ConnectAsync()
        {
            if (IsConnected)
            {
                return;

            }
            await hub.StartAsync();
            IsConnected = true;
        }
        public async Task DisconnectAsync()
        {
            if (!IsConnected)
            {
                return;

            }
            else
            {
                try
                {
                    await hub.StopAsync();
                    //  await hub.DisposeAsync();
                }
                finally
                {
                    await hub.DisposeAsync();
                }
            }

        }

    }
}
