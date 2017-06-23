using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Company.WebApplication1.Application.MVC.Services;
using Company.WebApplication1.Core.Command;
using Company.WebApplication1.Core.Query;
using Company.WebApplication1.Infrastructure.DataAccess;
using Module = Autofac.Module;

namespace Company.WebApplication1.UnitTests
{
    public class BaseTest
    {
        protected InMemoryApplicationDbContext DbContext { get; set; }
        protected IContainer Container { get; set; }

        public BaseTest()
        {
            var databaseName = Guid.NewGuid().ToString();
            DbContext = new InMemoryApplicationDbContext(databaseName);
            DbContext.Seed();
            DbContext.SaveChanges();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<DefaultModule>();
            containerBuilder.Register(c => new InMemoryApplicationDbContext(databaseName)).As<ApplicationDbContext>();
            Container = containerBuilder.Build();
        }

        public void ReloadDbContext()
        {
            DbContext = (InMemoryApplicationDbContext)Container.Resolve<ApplicationDbContext>();
        }
    }

    public class DefaultModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(new LoggerFactory()).As<ILoggerFactory>();

            var config = new MapperConfiguration(cfg => {
                cfg.AddProfile(new AutoMapperConfig());
            });
            builder.RegisterInstance(config.CreateMapper()).As<IMapper>();

            builder.RegisterType<HostingEnvironment>().As<IHostingEnvironment>();

            builder.RegisterType<QueryDb>();

            var webAssembly = Assembly.Load(new AssemblyName("Company.WebApplication1.Application.MVC"));
            builder.RegisterAssemblyTypes(webAssembly)
                .Where(t => t.Name.EndsWith("Controller"))
                .AsSelf();
        }
    }
}
