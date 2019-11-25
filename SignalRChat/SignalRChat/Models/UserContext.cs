using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRChat.Models
{
    public class UserContext
    {
        public List<UserModel> Users { get; set; }
        public List<GroupModel> Groups { get; set; }


    }
}
