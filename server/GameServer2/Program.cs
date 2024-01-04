using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GameServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var hostUrl = "http://0.0.0.0:80";
                    String dir = Directory.GetCurrentDirectory();
                    String www = dir + "/wwwroot";
                    webBuilder.UseStartup<Startup>()
                    .UseUrls(hostUrl)
                    .UseContentRoot(dir)
                    .UseWebRoot(www)
                    .UseStaticWebAssets();
                });
        }
    }
}
