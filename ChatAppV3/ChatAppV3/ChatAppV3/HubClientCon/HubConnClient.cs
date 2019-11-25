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
        public HubConnClient()
        {
            hub = new HubConnectionBuilder()
                .WithUrl($"http://172.16.3.53:5565/chathub")
                    .WithAutomaticReconnect()
                        .Build();

        }


    }
}
