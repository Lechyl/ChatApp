using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRChat.Models
{
    public class UserModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConnectionID { get; set; }
        public string UserID { get; set; }
        public bool IsConnected { get; set; }
        public List<FriendsModel> Friends { get; set; }

        public UserModel()
        {
            Friends = new List<FriendsModel>();
        }

    }
}
