using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MimeKit;

namespace Project_EFT
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
            // necessary for accessing sessions
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                // session timeout after 15 minutes of being idle
                options.IdleTimeout = TimeSpan.FromSeconds(900);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404 || context.Response.StatusCode == 405)
                {
                    context.Request.Path = "/Home/Error";
                    await next();
                }
            });
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            // necessary for accessing sessions
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}");
                endpoints.MapControllerRoute(
                    name: "signup",
                    pattern: "{controller=Signup}/{action=Signup}");
                endpoints.MapControllerRoute(
                    name: "recoverInfo",
                    pattern: "{controller=Recovery}/{action=RecoverInfo}");
                endpoints.MapControllerRoute(
                    name: "editInfo",
                    pattern: "{controller=EditInfo}/{action=EditInfo}");
                endpoints.MapControllerRoute(
                    name: "specificUser",
                    pattern: "{controller=Users}/{action=Users}/{username?}");
                endpoints.MapControllerRoute(
                    name: "cipherRoute",
                    pattern: "{controller=Cipher}/{action=Cipher}");
                endpoints.MapControllerRoute(
                    name: "problemRoute",
                    pattern: "{controller=Problem}/{action=Problem}/");
            });
        }
    }
}
