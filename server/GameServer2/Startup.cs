using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ServerDataStructures;
using ServerStateInterfaces;
using TrajectoryInterfaces;
using UserState;
using Microsoft.AspNetCore.ResponseCompression;


namespace GameServer
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression();
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                // options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                //options.Cookie.Expiration = TimeSpan.FromDays(1);
                // Make the session cookie essential
                options.Cookie.IsEssential = true;
            });
            services.AddSingleton<
                IFullServerStateGeocontroller<WellPoint, UserData, UserEvaluation, LevelDescription<WellPoint, RealizationData, TrueModelState>>,
                FullServerState
                //ServerStateMock
            >();

            services.AddControllers();

        }

        public static Task RedirectTask(HttpContext context)
        {
            QueryString qs = context.Request.QueryString;
            context.Response.Redirect("/geo/redirect"+qs.ToString());
            return context.Response.StartAsync();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();
            app.UseStaticFiles();
            app.UseSession();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            

            app.UseEndpoints(endpoints =>
            {
                endpoints.Map("default", context => RedirectTask(context));
                endpoints.Map("/", context => RedirectTask(context));
                endpoints.MapControllers();
            });

        }
    }
}
