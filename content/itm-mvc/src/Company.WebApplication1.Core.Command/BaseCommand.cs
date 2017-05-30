using System;
using Microsoft.Extensions.Logging;

namespace Company.WebApplication1.Core.Command
{
    public abstract class BaseCommand<T>
    {
        protected ILogger _logger;

        /// <summary>
        /// This should contain the actual implementation of a command.
        /// </summary>
        protected abstract void RunCommand();

        public BaseCommand(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void Run()
        {
            _logger.LogInformation("Running command {@Command}", this);

            try
            {
                RunCommand();
            }
            catch (Exception ex)
            {
                // using magical EvnetId 0 until it's possible to log exceptions without EventId
                // https://github.com/aspnet/Logging/issues/294
                _logger.LogCritical(0, ex, "Failed running {@Commnad}", this);
                throw;
            }
        }
    }
}
