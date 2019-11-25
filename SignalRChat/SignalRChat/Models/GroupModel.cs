using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRChat.Models
{
    public class GroupModel
    {
        public string GroupName { get; set; }
        public string GroupID { get; set; }
        public List<UserModel> Users { get; set; }
    }
}
