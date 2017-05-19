using System.Linq;
using Company.WebApplication1.Infrastructure.DataAccess;
#if (SeedMethod == "OOSeed")
using Company.WebApplication1.Infrastructure.DataAccess.Data.Seed;
#elseif (SeedMethod == "CSVSeed")
using Company.WebApplication1.Infrastructure.DataAccess.CsvSeeder;
#endif
using Microsoft.EntityFrameworkCore;
using Company.WebApplication1.Application.MVC.Services;
using AutoMapper;
using Serilog;
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
using Company.WebApplication1.Core.DomainServices;
using Company.WebApplication1.Core.Query;
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

namespace Company.WebApplication1.Application.MVC
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
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
            // Add framework services.
#if (IndividualAuth)
            services.AddDbContext<ApplicationDbContext>(options =>
#if (UseLocalDB)
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Company.WebApplication1.Infrastructure.DataAccess")));
#else
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Company.WebApplication1.Infrastructure.DataAccess")));
#endif

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
#endif
            services.AddMvc();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMapperConfig());
            });
            services.AddSingleton<IMapper>(sp => config.CreateMapper());
#if (IndividualAuth)

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddScoped(typeof(QueryDb<>), typeof(QueryDb<>));
            services.AddScoped<ApplicationDbContext, ApplicationDbContext>();
#elseif (OrganizationalAuth)

            services.AddAuthentication(
                SharedOptions => SharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug();
            loggerFactory.AddSerilog();

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

            using(var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
#if (SeedMethod == "OOSeed")
                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                context.MigrateAndSeedData();
#elseif (SeedMethod == "CSVSeed")
                context.Database.Migrate();

                if (context.Users.Any() == false)
                    context.Users.SeedFromFile("SeedData/contacts.csv");
                context.SaveChanges();
#endif
            }
        }
    }
}
