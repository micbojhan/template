using Company.WebApplication1.Application.MVC;
using Company.WebApplication1.Infrastructure.DataAccess;
using GeekLearning.Testavior.Environment;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Company.WebApplication1.IntegrationTests
{
    public class HomeControllerTests
    {
        private readonly TestEnvironment<Startup, TestStartupConfigurationService<ApplicationDbContext>> _testEnvironment;

        public HomeControllerTests()
        {
            _testEnvironment = new TestEnvironment<Startup, TestStartupConfigurationService<ApplicationDbContext>>(Path.Combine(System.AppContext.BaseDirectory, @"..\..\..\..\..\src\Company.WebApplication1.Application.MVC"));
        }

        [Fact]
        public async Task Index_RouteWorks_ReturnsSuccessfully()
        {
            // Act
            var response = await _testEnvironment.Client.GetAsync("/Home/Index");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task About_RouteWorks_ReturnsSuccessfully()
        {
            // Act
            var response = await _testEnvironment.Client.GetAsync("/Home/About");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Contact_RouteWorks_ReturnsSuccessfully()
        {
            // Act
            var response = await _testEnvironment.Client.GetAsync("/Home/Contact");

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}
