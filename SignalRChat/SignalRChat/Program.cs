using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SignalRChat.Database;
using SignalRChat.Hubs;
using SignalRChat.Models;

namespace SignalRChat
{

    public class Program
    {
        static List<UserModel> userlist = new List<UserModel>();
        static List<GroupModel> groups = new List<GroupModel>();
        public UserContext users = new UserContext()
        {
            Users = userlist,
            Groups = groups


        };

        public static void Main(string[] args)
        {
            DbStartUp db = new DbStartUp();
            var result = db.GetGroups();
            groups = result;
            CreateWebHostBuilder(args).Build().Run();

           
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                //.UseUrls("https://*:5566")
                //.UseContentRoot(Directory.GetCurrentDirectory())
                //.UseIISIntegration()
                .UseStartup<Startup>();

        public string test()
        {
            return "--------------------------------------";
        }
    }
}
