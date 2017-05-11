using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Company.WebApplication1.IntegrationTests
{
    public class HomeControllerTests : IClassFixture<TestFixture<Company.WebApplication1.Application.MVC.Startup>>
    {
        private readonly HttpClient _client;

        public HomeControllerTests(TestFixture<Company.WebApplication1.Application.MVC.Startup> fixture)
        {
            _client = fixture.Client;
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
