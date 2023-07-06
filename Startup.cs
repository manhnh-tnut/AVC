using Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AVC.Models;
using AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore.Authorization;
using AVC.Policy;
using AVC.Hubs;
using Microsoft.Extensions.Options;
using AVC.Collections;
using AVC.Interfaces;
using AVC.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile($"dbsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<DatabaseSettings>(Configuration.GetSection(nameof(DatabaseSettings)));
            services.AddIdentityMongoDbProvider<AVC.IdentityModels.IdentityUser, AVC.IdentityModels.IdentityRole>(identityOptions =>
            {
                identityOptions.Password.RequiredLength = 6;
                identityOptions.Password.RequireLowercase = false;
                identityOptions.Password.RequireUppercase = false;
                identityOptions.Password.RequireNonAlphanumeric = false;
                identityOptions.Password.RequireDigit = false;
            }, mongoIdentityOptions =>
            {
                mongoIdentityOptions.ConnectionString = Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>().ConnectionString + "/" + Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>().DatabaseName;
            });

            services.AddSingleton<IDatabaseSettings>(sp =>
            {
                return sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
            });
            services.AddTransient<IMongoContext, MongoContext>();
            services.AddTransient<ILogCollection, LogCollection>();
            services.AddTransient<ILogService, LogService>();
            services.AddTransient<IMachineCollection, MachineCollection>();
            services.AddTransient<IMachineService, MachineService>();
            services.AddTransient<ISummaryCollection, SummaryCollection>();
            services.AddTransient<ISummaryService, SummaryService>();

            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, HasClaimHandler>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSignalR((signalR =>
            {
                signalR.EnableDetailedErrors = true;
            }));
            services.AddHostedService<HubServiceWorker>();
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

            app.UseSerilogRequestLogging(options =>
            {
                // Customize the message template
                options.MessageTemplate = "Handled {RequestPath}";

                // Emit debug-level events instead of the defaults
                options.GetLevel = (httpContext, elapsed, ex) => Serilog.Events.LogEventLevel.Debug;

                // Attach additional properties to the request completion event
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                };
            });
            app.UseHttpsRedirection();
            app.UseFileServer();

            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapHub<HubService>("/hubs/live");
            });
        }
    }
}
