using System;
using System.Collections.Generic;
using System.Text;

namespace ChatAppV3.Models
{
    [Serializable]
    class UserModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserID { get; set; }
        
    }
}
