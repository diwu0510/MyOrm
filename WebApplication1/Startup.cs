using AutoMapper;
using HZC.MyOrm;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin;
using Senparc.Weixin.Entities;
using Senparc.Weixin.RegisterServices;
using Senparc.Weixin.Work;
using WebApplication1.Models;

namespace WebApplication1
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
            MyDbConfiguration.Init(Configuration.GetConnectionString("DefaultConnectionString"));

            services.AddAutoMapper(options => { options.AddProfile<CustomerProfile>(); });

            services.AddDistributedRedisCache(options => { options.Configuration = "127.0.0.1:6379"; });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.Name = "Zodo.Approval";
                    options.AccessDeniedPath = "/Error/Deny";
                    options.LoginPath = "/Login";
                    //options.ExpireTimeSpan = TimeSpan.FromDays(2);
                    options.SlidingExpiration = false;
                });

            services.AddSenparcGlobalServices(Configuration)//Senparc.CO2NET 全局注册
                .AddSenparcWeixinServices(Configuration);//Senparc.Weixin 注册

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            IOptions<SenparcSetting> senparcSetting,
            IOptions<SenparcWeixinSetting> senparcWeixinSetting)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            var register = RegisterService.Start(env, senparcSetting.Value)
                .UseSenparcGlobal();

            register.UseSenparcWeixin(senparcWeixinSetting.Value, senparcSetting.Value)
                .RegisterWorkAccount(senparcWeixinSetting.Value);
        }
    }
}
