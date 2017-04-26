using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Company.WebApplication1.Infrastructure.DataAccess;

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


        public BaseCommand(ILogger<BaseCommand> logger)
        {
            _logger = logger;
        }

        public BaseCommand(ILogger<BaseCommand> logger, ApplicationDbContext dbContext)
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