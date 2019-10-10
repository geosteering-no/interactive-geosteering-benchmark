using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace GameServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {

                    String dir = Directory.GetCurrentDirectory();
                    String www = dir + "/wwwroot";
                    webBuilder.UseStartup<Startup>()
                    .UseContentRoot(dir)
                    .UseWebRoot(www)
                    .UseStaticWebAssets();
                });

    }
}
