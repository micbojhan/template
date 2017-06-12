using Company.WebApplication1.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Company.WebApplication1.Application.MVC.Services;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
#if (OrganizationalAuth)
using Microsoft.AspNetCore.Authentication.Cookies;
#endif
#if (MultiOrgAuth)
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
#endif
#if (IndividualAuth)
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
#endif
#if (OrganizationalAuth && OrgReadAccess)
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
#endif
#if (MultiOrgAuth)
using Microsoft.IdentityModel.Tokens;
#endif
#if (IndividualAuth)
using Company.WebApplication1.Core.Entities;
#endif
using GeekLearning.Testavior.Configuration.Startup;

namespace Company.WebApplication1.Application.MVC
{
    public class Startup
    {
        private readonly IStartupConfigurationService _externalStartupConfiguration;

        public Startup(IHostingEnvironment env, IStartupConfigurationService externalStartupConfiguration = null)
        {
            if (externalStartupConfiguration == null) // needed when running dotnet ef
                externalStartupConfiguration = new StartupConfigurationService();

            _externalStartupConfiguration = externalStartupConfiguration;
            _externalStartupConfiguration.ConfigureEnvironment(env);

#if (IndividualAuth || OrganizationalAuth)
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }
#else
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
#endif
#if (IndividualAuth || MultiOrgAuth || SingleOrgAuth)

            builder.AddEnvironmentVariables();
#endif
            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
              .Enrich.FromLogContext()
              .ReadFrom.Configuration(Configuration)
              .CreateLogger();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
#if (IndividualAuth)
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
#endif
            services.AddMvc();
            services.AddAutoMapper();
#if (IndividualAuth)

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddScoped<ApplicationDbContext, ApplicationDbContext>();
#elseif (OrganizationalAuth)

            services.AddAuthentication(
                SharedOptions => SharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
#endif

            _externalStartupConfiguration.ConfigureServices(services, Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _externalStartupConfiguration.Configure(app, env, loggerFactory);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
#if (IndividualAuth)
                app.UseDatabaseErrorPage();
#endif
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

#if (IndividualAuth)
            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

#elseif (OrganizationalAuth)
            app.UseCookieAuthentication();

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                ClientId = Configuration["Authentication:AzureAd:ClientId"],
    #if (OrgReadAccess)
                ClientSecret = Configuration["Authentication:AzureAd:ClientSecret"],
    #endif
    #if (MultiOrgAuth)
                Authority = Configuration["Authentication:AzureAd:AADInstance"] + "Common",
    #elseif (SingleOrgAuth)
                Authority = Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"],
    #endif
#endif
#if (MultiOrgAuth)
                CallbackPath = Configuration["Authentication:AzureAd:CallbackPath"],
    #if (OrgReadAccess)
                ResponseType = OpenIdConnectResponseType.CodeIdToken,
    #endif

                TokenValidationParameters = new TokenValidationParameters
                {
                    // Instead of using the default validation (validating against a single issuer value, as we do in line of business apps),
                    // we inject our own multitenant validation logic
                    ValidateIssuer = false,

                    // If the app is meant to be accessed by entire organizations, add your issuer validation logic here.
                    //IssuerValidator = (issuer, securityToken, validationParameters) => {
                    //    if (myIssuerValidationLogic(issuer)) return issuer;
                    //}
                },
                Events = new OpenIdConnectEvents
                {
                    OnTicketReceived = (context) =>
                    {
                        // If your authentication logic is based on users then add your logic here
                        return Task.FromResult(0);
                    },
                    OnAuthenticationFailed = (context) =>
                    {
                        context.Response.Redirect("/Home/Error");
                        context.HandleResponse(); // Suppress the exception
                        return Task.FromResult(0);
                    },
                    // If your application needs to do authenticate single users, add your user validation below.
                    //OnTokenValidated = (context) =>
                    //{
                    //    return myUserValidationLogic(context.Ticket.Principal);
                    //}
                }
#elseif (SingleOrgAuth)
    #if (OrgReadAccess)
                CallbackPath = Configuration["Authentication:AzureAd:CallbackPath"],
                ResponseType = OpenIdConnectResponseType.CodeIdToken
    #else
                CallbackPath = Configuration["Authentication:AzureAd:CallbackPath"]
    #endif
#endif
#if (OrganizationalAuth)
            });

#endif
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
