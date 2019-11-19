using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatAppV3.HubClientCon
{
    class HubConnClient
    {
        public HubConnection HubConn()
        {

            HubConnection hubConnection = new HubConnectionBuilder()
                .WithUrl($"http://172.16.3.63:5565/chathub")
                .Build();

            return hubConnection;
        }
    }
}
