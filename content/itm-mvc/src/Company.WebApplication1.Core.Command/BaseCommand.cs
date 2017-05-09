using Microsoft.Extensions.Logging;

namespace Company.WebApplication1.Core.Command
{
    public abstract class BaseCommand
    {
        protected ILogger _logger;

        /// <summary>
        /// This should contain the actual implementation of a command.
        /// </summary>
        protected abstract void RunCommand();

        public BaseCommand(ILogger<BaseCommand> logger)
        {
            _logger = logger;
        }

        public void Run()
        {
            _logger.LogInformation("Running command {@Command}", this);
            RunCommand();
        }
    }
}
