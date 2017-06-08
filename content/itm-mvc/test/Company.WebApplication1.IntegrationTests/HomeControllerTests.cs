using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Company.WebApplication1.IntegrationTests
{
    public class HomeControllerTests
    {
        private readonly HttpClient _client;
        private readonly TestStartup<Company.WebApplication1.Application.MVC.Startup> _fixture;

        public HomeControllerTests()
        {
            _fixture = new TestStartup<Company.WebApplication1.Application.MVC.Startup>();
            _client = _fixture.Client;
        }

        [Fact]
        public async Task Index_RouteWorks_ReturnsSuccessfully()
        {
            // Act
            var response = await _client.GetAsync("/Home/Index");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task About_RouteWorks_ReturnsSuccessfully()
        {
            // Act
            var response = await _client.GetAsync("/Home/About");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Contact_RouteWorks_ReturnsSuccessfully()
        {
            // Act
            var response = await _client.GetAsync("/Home/Contact");

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}
