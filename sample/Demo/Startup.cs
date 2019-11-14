using System.Collections.Generic;
using Es.Throttle;
using Es.Throttle.Mvc;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Demo
{
    public class Startup
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
              .ConfigureWebHostDefaults(webBuilder =>
              {
                  webBuilder.UseStartup<Startup>();
              })
              .Build()
              .Run();
        }


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMvc();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            services.AddThrottle(Configuration.GetSection("Throttle"));

            //services.AddThrottle(
            //    new ThrottleOptions
            //    {
            //        EnableGlobalFilter = false,
            //        Policy = new ThrottlePolicy
            //        {
            //            EnableHttpMethod = true,
            //            //EnableIP = true,
            //            //EnableRequestPath = true,
            //            //EnableUserAgent = true,
            //            //IpWhitelist = new List<string> {
            //            //     "192.168.0.0 - 192.168.255.255",
            //            //},
            //            //UserAgentWhitelist = new List<string>{
            //            //        "Chrome",
            //            //},
            //            //RequestPathWhitelist = new List<string>{
            //            //        "/home/index"
            //            //},
            //            //UserAgentRules = new Dictionary<string, RateQuota>{
            //            //     { "Chrome", new RateQuota(50,50, RateLimitPeriod.Sec)}
            //            //},
            //            //IPRules = new Dictionary<string, RateQuota>{
            //            //     { "127.0.0.1", new RateQuota(50,50, RateLimitPeriod.Sec)}
            //            //},
            //            //RequestPathRules = new Dictionary<string, RateQuota>{
            //            //     {  "/home/xxxx", new RateQuota(1,1, RateLimitPeriod.Sec)},
            //            //     {  "/home/index", new RateQuota(50,50, RateLimitPeriod.Sec)}
            //            //},
            //        }
            //    }
            //   );
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}