using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRChat.Models
{
    public class UserModel
    {
        public string Name { get; set; }
        public string ConnectionID { get; set; }
        public string UserID { get; set; }

        public UserModel(string name, string conID, string userID)
        {
            Name = name;
            ConnectionID = conID;
            UserID = userID;
        }
    }
}
