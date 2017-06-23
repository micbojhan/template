using System;
using Company.WebApplication1.Infrastructure.DataAccess;
using Microsoft.Extensions.Logging;

namespace Company.WebApplication1.Core.Command
{
    public abstract class BaseCommand
    {
        protected ILogger _logger;
        protected ApplicationDbContext _dbContext;

        /// <summary>
        /// This should contain the actual implementation of a command.
        /// </summary>
        protected abstract void RunCommand();

        public BaseCommand(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BaseCommand>();
        }

        public BaseCommand(ILoggerFactory loggerFactory, ApplicationDbContext dbContext)
        {
            _logger = loggerFactory.CreateLogger<BaseCommand>();
            _dbContext = dbContext;
        }

        public void Run()
        {
            _logger.LogInformation("Running command {@Command}", this);
            RunCommand();
        }
    }
}
