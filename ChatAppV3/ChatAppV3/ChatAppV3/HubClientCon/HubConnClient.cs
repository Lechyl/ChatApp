using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatAppV3.HubClientCon
{
    class HubConnClient
    {
        public HubConnection hub;
        public bool IsConnected { get; set; }
        public HubConnClient()
        {
            IsConnected = false;
            hub = new HubConnectionBuilder()
                .WithUrl($"http://172.16.3.49:5565/chathub")
                    .WithAutomaticReconnect()
                        .Build();
            hub.Closed += async (error) =>
            {
                IsConnected = false;
                try
                {
                    ConnectAsync().Wait(5000);
                }
                catch (Exception)
                {

                    throw;
                }
            };

        }
        public async Task ConnectAsync()
        {
            if (IsConnected)
                return;
            await hub.StartAsync();
            IsConnected = true;
        }
        public async Task DisconnectAsync()
        {
            if (!IsConnected)
                return;
            try
            {
                await hub.DisposeAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
