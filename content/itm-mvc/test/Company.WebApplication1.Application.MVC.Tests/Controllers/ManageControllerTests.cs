using Microsoft.AspNetCore.Identity;
using Xunit;
using NSubstitute;
using Company.WebApplication1.Core.Entities;
using Microsoft.Extensions.Options;
using Company.WebApplication1.Application.MVC.Services;
using Microsoft.Extensions.Logging;
using Company.WebApplication1.Application.MVC.Controllers;
using static Company.WebApplication1.Application.MVC.Controllers.ManageController;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Company.WebApplication1.Models.ManageViewModels;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace Company.WebApplication1.Application.MVC.Tests.Controllers
{
    public class ManageControllerTests
    {
        private readonly UserManager<ApplicationUser> _userManagerMock;
        private readonly ManageController _uut;
        private readonly SignInManager<ApplicationUser> _signInManagerMock;

        public ManageControllerTests()
        {
            //Usermanager mocks
            var userStoreMock = Substitute.For<IUserStore<ApplicationUser>>();
            var optionsMock = Substitute.For<IOptions<IdentityOptions>>();
            var passwordHasherMock = Substitute.For<IPasswordHasher<ApplicationUser>>();
            var userValidatorMock = Substitute.For<IEnumerable<IUserValidator<ApplicationUser>>>();
            var passwordValidatorMock = Substitute.For<IEnumerable<IPasswordValidator<ApplicationUser>>>();
            var lookUpNormalizerMock = Substitute.For<ILookupNormalizer>();
            var identityErrorDescriberMock = Substitute.For<IdentityErrorDescriber>();
            var serviceProviderMock = Substitute.For<IServiceProvider>();
            var loggerMock = Substitute.For<ILogger<UserManager<ApplicationUser>>>();

            _userManagerMock = Substitute.For<UserManager<ApplicationUser>>(
                userStoreMock,
                optionsMock,
                passwordHasherMock,
                userValidatorMock,
                passwordValidatorMock,
                lookUpNormalizerMock,
                identityErrorDescriberMock,
                serviceProviderMock,
                loggerMock);

            //SignInManager mocks
            var contextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var claimsPricipleFactoryMock = Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var loggerMockSM = Substitute.For<ILogger<SignInManager<ApplicationUser>>>();

            _signInManagerMock = Substitute.For<SignInManager<ApplicationUser>>(
                _userManagerMock,
                contextAccessorMock,
                claimsPricipleFactoryMock,
                optionsMock,
                loggerMockSM);

            var cookieOptionsMock = Substitute.For<IOptions<IdentityCookieOptions>>();
            var identityOptionsMock = new IdentityCookieOptions();
            cookieOptionsMock.Value.Returns(identityOptionsMock);

            var emailMock = Substitute.For<IEmailSender>();
            var smsMock = Substitute.For<ISmsSender>();
            var loggerFactoryMock = Substitute.For<ILoggerFactory>();
            var loggerMockCtrl = Substitute.For<ILogger>();
            loggerFactoryMock.CreateLogger("").Returns(loggerMockCtrl);

            _uut = new ManageController(_userManagerMock, _signInManagerMock, cookieOptionsMock, emailMock, smsMock, loggerFactoryMock);
        }

        //Index
        [Theory]
        [InlineData(ManageMessageId.ChangePasswordSuccess, "Your password has been changed.")]
        [InlineData(ManageMessageId.SetPasswordSuccess, "Your password has been set.")]
        [InlineData(ManageMessageId.SetTwoFactorSuccess, "Your two-factor authentication provider has been set.")]
        [InlineData(ManageMessageId.Error, "An error has occurred.")]
        [InlineData(ManageMessageId.AddPhoneSuccess, "Your phone number was added.")]
        [InlineData(ManageMessageId.RemovePhoneSuccess, "Your phone number was removed.")]
        [InlineData(null, "")]
        public void Index_GivenStatusMessage_ViewBagContainsCorrectMessage(ManageMessageId? messageId, string statusMessage)
        {
            //Arrange

            //Act
            var result = _uut.Index(messageId);

            //Assert
            Assert.Equal(statusMessage, _uut.ViewData["StatusMessage"]);
        }

        [Fact]
        public async void Index_UserIsValid_ReturnsDefaultView()
        {
            //Arrange
            var validPrincipal = new ClaimsPrincipal(new[]
            {
                new ClaimsIdentity(
                    new[] {new Claim(ClaimTypes.NameIdentifier, "MyUserId")})
            });
            var httpContext = Substitute.For<HttpContext>();
            httpContext.User.Returns(validPrincipal);

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _userManagerMock.HasPasswordAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(true));
            _userManagerMock.GetPhoneNumberAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(""));
            _userManagerMock.GetTwoFactorEnabledAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(true));
            IList<UserLoginInfo> list = new List<UserLoginInfo>();
            _userManagerMock.GetLoginsAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(list));
            _signInManagerMock.IsTwoFactorClientRememberedAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(true));
            _uut.TempData = Substitute.For<ITempDataDictionary>();

            //Act
            var result = await _uut.Index(null) as ViewResult;

            //Assert
            Assert.Null(result.ViewName); //Returns default view
        }

        [Fact]
        public async void Index_UserIsValid_ReturnsCorrectViewModel()
        {
            //Arrange
            var validPrincipal = new ClaimsPrincipal(new[]
                {
                    new ClaimsIdentity(
                        new[] {new Claim(ClaimTypes.NameIdentifier, "MyUserId")})
                });
            var httpContext = Substitute.For<HttpContext>();
            httpContext.User.Returns(validPrincipal);

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            var viewModel = new IndexViewModel
            {
                HasPassword = true,
                PhoneNumber = "",
                TwoFactor = true,
                Logins = new List<UserLoginInfo>(),
                BrowserRemembered = true
            };

            _userManagerMock.HasPasswordAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(viewModel.HasPassword));
            _userManagerMock.GetPhoneNumberAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(viewModel.PhoneNumber));
            _userManagerMock.GetTwoFactorEnabledAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(viewModel.TwoFactor));
            _userManagerMock.GetLoginsAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(viewModel.Logins));
            _signInManagerMock.IsTwoFactorClientRememberedAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(viewModel.BrowserRemembered));
            _uut.TempData = Substitute.For<ITempDataDictionary>();

            //Act
            var result = await _uut.Index(null) as ViewResult;
            var originalViewModel = JsonConvert.SerializeObject(viewModel);
            var viewModelReturnedToView = JsonConvert.SerializeObject(result.Model);

            //Assert
            Assert.Equal(originalViewModel, viewModelReturnedToView);
        }

        [Fact]
        public async void Index_UnregisteredUser_ReturnsErrorView()
        {
            //Arrange
            var httpContext = Substitute.For<HttpContext>();

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            ApplicationUser nullUser = null;
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult(nullUser));
            _userManagerMock.HasPasswordAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(true));
            _userManagerMock.GetPhoneNumberAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(""));
            _userManagerMock.GetTwoFactorEnabledAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(true));
            IList<UserLoginInfo> list = new List<UserLoginInfo>();
            _userManagerMock.GetLoginsAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(list));
            _signInManagerMock.IsTwoFactorClientRememberedAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(true));
            _uut.TempData = Substitute.For<ITempDataDictionary>();

            //Act
            var result = await _uut.Index(null) as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        //Removelogin
        [Fact]
        public async void Removelogin_UnregisteredUser_CallsRedirectWithErrorMessage()
        {
            //Arrange
            var removeLoginViewModelMock = new RemoveLoginViewModel
            {
                LoginProvider = "",
                ProviderKey = ""
            };
            var httpContext = Substitute.For<HttpContext>();

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            ApplicationUser nullUser = null;
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult(nullUser));
            _uut.Url = Substitute.For<IUrlHelper>();

            //Act
            var result = await _uut.RemoveLogin(removeLoginViewModelMock) as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.Error, result.RouteValues["message"]);
        }

        [Fact]
        public async void Removelogin_RegisteredUserWithRemoveLoginSuccess_CallsRedirectWithRemoveLoginSuccessMessage()
        {
            //Arrange
            var removeLoginViewModelMock = new RemoveLoginViewModel
            {
                LoginProvider = "",
                ProviderKey = ""
            };

            var validPrincipal = new ClaimsPrincipal(new[]
                {
                    new ClaimsIdentity(
                        new[] {new Claim(ClaimTypes.NameIdentifier, "MyUserId")})
                });
            var httpContext = Substitute.For<HttpContext>();
            httpContext.User.Returns(validPrincipal);

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _userManagerMock.RemoveLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Success));

            _uut.Url = Substitute.For<IUrlHelper>();

            //Act
            var result = await _uut.RemoveLogin(removeLoginViewModelMock) as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.RemoveLoginSuccess, result.RouteValues["message"]);
        }

        [Fact]
        public async void Removelogin_RegisteredUserWithRemoveLoginError_CallsRedirectWithErrorMessage()
        {
            //Arrange
            var removeLoginViewModelMock = new RemoveLoginViewModel
            {
                LoginProvider = "",
                ProviderKey = ""
            };

            var validPrincipal = new ClaimsPrincipal(new[]
                {
                    new ClaimsIdentity(
                        new[] {new Claim(ClaimTypes.NameIdentifier, "MyUserId")})
                });
            var httpContext = Substitute.For<HttpContext>();
            httpContext.User.Returns(validPrincipal);

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _userManagerMock.RemoveLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Failed()));

            _uut.Url = Substitute.For<IUrlHelper>();

            //Act
            var result = await _uut.RemoveLogin(removeLoginViewModelMock) as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.Error, result.RouteValues["message"]);
        }

        //AddPhoneNumber
        [Fact]
        public void AddPhoneNumber_ReturnsDefaultViewResult()
        {
            //Arrange

            //Act
            var result = _uut.AddPhoneNumber() as ViewResult;

            //Assert
            Assert.Null(result.ViewName); //Returns default view
        }
    }
}
