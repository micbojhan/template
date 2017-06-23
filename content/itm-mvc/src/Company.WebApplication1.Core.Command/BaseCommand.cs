using System;
using Company.WebApplication1.Infrastructure.DataAccess;
using Microsoft.Extensions.Logging;

namespace Company.WebApplication1.Core.Command
{
    public abstract class BaseCommand<T>
    {
        protected ILogger _logger;
        protected ApplicationDbContext _dbContext;

        /// <summary>
        /// This should contain the actual implementation of a command.
        /// </summary>
        protected abstract void RunCommand();

        public BaseCommand(ILogger<T> logger)
        {
            _logger = logger;
        }

        public BaseCommand(ILogger<T> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public void Run()
        {
            _logger.LogInformation("Running command {@Command}", this);
            RunCommand();
        }
    }
}
