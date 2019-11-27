using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRChat.Models
{
    public class ClientMessage
    {
        public string message { get; set; }
        public string user { get; set; }
        public bool isOwnMessage { get; set; }
        public bool isSystemMessage { get; set; }
    }
}
