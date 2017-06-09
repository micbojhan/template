using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using GeekLearning.Testavior.Environment;
using Company.WebApplication1.Core.Entities;
using Company.WebApplication1.Application.MVC;
using Company.WebApplication1.Infrastructure.DataAccess;
using Company.WebApplication1.ViewModels.AccountViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace Company.WebApplication1.IntegrationTests
{
    public class AccountControllerTests
    {
        private readonly TestEnvironment<Startup, TestStartupConfigurationService<ApplicationDbContext>> _testEnvironment;

        public AccountControllerTests()
        {
            _testEnvironment = new TestEnvironment<Startup, TestStartupConfigurationService<ApplicationDbContext>>(Path.Combine(System.AppContext.BaseDirectory, @"..\..\..\..\..\src\Company.WebApplication1.Application.MVC"));
        }

        [Fact]
        public async Task Login_RouteWorks_ReturnsSuccessfully()
        {
            // Act
            var response = await _testEnvironment.Client.GetAsync("/Account/Login");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Login_UserLoginWithLocalReturnUrl_ReturnsRedirectToReturnUrl()
        {
            // Arrange
            var returnUrl = "/testurl";
            var password = "Password@123";
            var email = "test@test.uk";
            var viewModel = new LoginViewModel { Email = email, Password = password };

            using (var serviceScope = _testEnvironment.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var userToInsert = new ApplicationUser { Email = email, UserName = email };
                var userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                var result = await userManager.CreateAsync(userToInsert, password);
            }

            // Act
            var response = await _testEnvironment.Client.PostAsJsonAntiForgeryAsync("/Account/Login?returnUrl=" + returnUrl, viewModel);

            // Assert
            Assert.Equal(returnUrl, response.Headers.Location.ToString());
        }
    }
}
