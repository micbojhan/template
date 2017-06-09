using Company.WebApplication1.Infrastructure.DataAccess;
using GeekLearning.Testavior.Configuration.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
#if (SeedMethod == "OOSeed")
using Company.WebApplication1.Infrastructure.DataAccess.Data.Seed;
#elseif (SeedMethod == "CSVSeed")
using System.Linq;
using Company.WebApplication1.Infrastructure.DataAccess.CsvSeeder;
#endif
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;

namespace Company.WebApplication1.Application.MVC
{
    public class StartupConfigurationService : DefaultStartupConfigurationService
    {
        public override void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration)
        {
            base.ConfigureServices(services, configuration);

            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Company.WebApplication1.Infrastructure.DataAccess")));
        }

        public override void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            using(var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
#if (SeedMethod == "OOSeed")
                context.MigrateAndSeedData(app.ApplicationServices);
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
