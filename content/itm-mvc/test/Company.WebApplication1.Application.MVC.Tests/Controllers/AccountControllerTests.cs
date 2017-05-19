using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;
using Company.WebApplication1.Application.MVC.Controllers;
using Company.WebApplication1.Application.MVC.Services;
using Company.WebApplication1.Core.Entities;
using Company.WebApplication1.Models.AccountViewModels;
using ITM.MVC.TestExtensions;
using Xunit;

namespace Company.WebApplication1.Application.MVC.Tests.Controllers
{
    using SignInResult = Microsoft.AspNetCore.Identity.SignInResult; //Resolves ambigious classes for SignInResult
    public class AccountControllerTests
    {
        private readonly UserManager<ApplicationUser> _userManagerMock;
        private readonly AccountController _uut;
        private readonly SignInManager<ApplicationUser> _signInManagerMock;

        public AccountControllerTests()
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

            _uut = new AccountController(_userManagerMock, _signInManagerMock, cookieOptionsMock, emailMock, smsMock, loggerFactoryMock).WithDefaultMocks();
        }

        //Login(string)
        [Fact]
        public async void Login_HttpContextSignOutCalled()
        {
            //Arrange

            //Act
            var result = await _uut.Login() as ViewResult;

            //Assert
            await _uut.ControllerContext.HttpContext.Received().Authentication.SignOutAsync(Arg.Any<string>());
        }

        [Fact]
        public async void Login_CalledWithNoReturnUrl_ViewDataContainsNullInReturnUrl()
        {
            //Arrange

            //Act
            var result = await _uut.Login() as ViewResult;

            //Assert
            Assert.Null(result.ViewData["ReturnUrl"]);
        }

        [Fact]
        public async void Login_CalledWithReturnUrl_ViewDataContainsSameReturnUrl()
        {
            //Arrange
            var returnUrl = "123";

            //Act
            var result = await _uut.Login(returnUrl) as ViewResult;

            //Assert
            Assert.Equal(returnUrl, result.ViewData["ReturnUrl"]);
        }

        [Fact]
        public async void Login_ReturnsDefaultView()
        {
            //Arrange

            //Act
            var result = await _uut.Login(null) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        //Login(model, string)
        [Fact]
        public async void Login_CalledWithNoReturnUrlAndModelStateInvalid_ViewDataContainsNullInReturnUrl()
        {
            //Arrange
            _uut.ModelState.AddModelError("Error","Error");

            //Act
            var result = await _uut.Login(default(LoginViewModel)) as ViewResult;

            //Assert
            Assert.Null(result.ViewData["ReturnUrl"]);
        }

        [Fact]
        public async void Login_CalledWithReturnUrlAndModelStateInvalid_ViewDataContainsSameReturnUrl()
        {
            //Arrange
            var returnUrl = "123";
            _uut.ModelState.AddModelError("Error","Error");

            //Act
            var result = await _uut.Login(null, returnUrl) as ViewResult;

            //Assert
            Assert.Equal(returnUrl, result.ViewData["ReturnUrl"]);
        }

        [Fact]
        public async void Login_CalledWithNoReturnUrlAndModelStateValid_ViewDataContainsNullInReturnUrl()
        {
            //Arrange
            var loginViewModel = new LoginViewModel
            {
                Email = "",
                Password = "",
                RememberMe = true
            };
            _signInManagerMock.PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns(SignInResult.Failed);

            //Act
            var result = await _uut.Login(loginViewModel) as ViewResult;

            //Assert
            Assert.Null(result.ViewData["ReturnUrl"]);
        }

        [Fact]
        public async void Login_CalledWithReturnUrlAndModelStateValid_ViewDataContainsSameReturnUrl()
        {
            //Arrange
            var returnUrl = "123";
            var loginViewModel = new LoginViewModel
            {
                Email = "",
                Password = "",
                RememberMe = true
            };
            _signInManagerMock.PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns(SignInResult.Failed);

            //Act
            var result = await _uut.Login(loginViewModel, returnUrl) as ViewResult;

            //Assert
            Assert.Equal(returnUrl, result.ViewData["ReturnUrl"]);
        }

        [Fact]
        public async void Login_ModelStateNotValid_DefaultViewReturned()
        {
            //Arrange
            _uut.ModelState.AddModelError("Error","Error");

            //Act
            var result = await _uut.Login(default(LoginViewModel)) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void Login_ModelStateNotValid_ViewWithSameModelReturned()
        {
            //Arrange
            _uut.ModelState.AddModelError("Error","Error");
            var loginViewModel = new LoginViewModel
            {
                Email = "",
                Password = "",
                RememberMe = true
            };

            //Act
            var result = await _uut.Login(loginViewModel) as ViewResult;
            var originalViewModel = JsonConvert.SerializeObject(loginViewModel);
            var viewModelReturnedToView = JsonConvert.SerializeObject(result.Model);

            //Assert
            Assert.Equal(originalViewModel, viewModelReturnedToView);
        }

        [Fact]
        public async void Login_ModelStateValid_SignInManagerPasswordSignInCalled()
        {
            //Arrange
            var loginViewModel = new LoginViewModel
            {
                Email = "",
                Password = "",
                RememberMe = true
            };
            _signInManagerMock.PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns(SignInResult.Failed);

            //Act
            var result = await _uut.Login(loginViewModel) as ViewResult;

            //Assert
            await _signInManagerMock.Received().PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>());
        }

        [Fact]
        public async void Login_ModelStateNotValid_SignInManagerPasswordSignInNotCalled()
        {
            //Arrange
            _uut.ModelState.AddModelError("Error","Error");

            //Act
            var result = await _uut.Login(default(LoginViewModel)) as ViewResult;

            //Assert
            await _signInManagerMock.DidNotReceive().PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>());
        }

        [Fact]
        public async void Login_SignInManagerPasswordSignInSuccessAndReturnUrlNotLocal_ReturnsRedirectToHomeController()
        {
            //Arrange
            var loginViewModel = new LoginViewModel
            {
                Email = "",
                Password = "",
                RememberMe = true
            };
            _signInManagerMock.PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns(SignInResult.Success);
            _uut.Url.IsLocalUrl(Arg.Any<string>()).Returns(false);

            //Act
            var result = await _uut.Login(loginViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal("Home", result.ControllerName);
        }

        [Fact]
        public async void Login_SignInManagerPasswordSignInSuccessAndReturnUrlNorLocal_ReturnsRedirectToIndex()
        {
            //Arrange
            var loginViewModel = new LoginViewModel
            {
                Email = "",
                Password = "",
                RememberMe = true
            };
            _signInManagerMock.PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns(SignInResult.Success);
            _uut.Url.IsLocalUrl(Arg.Any<string>()).Returns(false);

            //Act
            var result = await _uut.Login(loginViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public async void Login_SignInManagerPasswordSignInSuccessAndReturnUrlIsLocal_ReturnsRedirectResult()
        {
            //Arrange
            var loginViewModel = new LoginViewModel
            {
                Email = "",
                Password = "",
                RememberMe = true
            };
            _signInManagerMock.PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns(SignInResult.Success);
            _uut.Url.IsLocalUrl(Arg.Any<string>()).Returns(true);

            //Act
            var result = await _uut.Login(loginViewModel,"123");

            //Assert
            Assert.IsType<RedirectResult>(result);
        }

        [Fact]
        public async void Login_SignInManagerPasswordSignInRequiresTwoFactor_ReturnsRedirectToActionSendCode()
        {
            //Arrange
            var loginViewModel = new LoginViewModel
            {
                Email = "",
                Password = "",
                RememberMe = true
            };
            _signInManagerMock.PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns(SignInResult.TwoFactorRequired);

            //Act
            var result = await _uut.Login(loginViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal("SendCode", result.ActionName);
        }

        [Fact]
        public async void Login_SignInManagerPasswordSignInRequiresTwoFactor_ReturnsRedirectToActionWithCorrectReturnUrlParameter()
        {
            //Arrange
            var returnUrl = "123";
            var loginViewModel = new LoginViewModel
            {
                Email = "",
                Password = "",
                RememberMe = true
            };
            _signInManagerMock.PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns(SignInResult.TwoFactorRequired);

            //Act
            var result = await _uut.Login(loginViewModel, returnUrl) as RedirectToActionResult;

            //Assert
            Assert.Equal(returnUrl, result.RouteValues["ReturnUrl"]);
        }

        [Fact]
        public async void Login_SignInManagerPasswordSignInRequiresTwoFactor_ReturnsRedirectToActionWithCorrectRemmeberMeParameter()
        {
            //Arrange
            var loginViewModel = new LoginViewModel
            {
                Email = "",
                Password = "",
                RememberMe = true
            };
            _signInManagerMock.PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns(SignInResult.TwoFactorRequired);

            //Act
            var result = await _uut.Login(loginViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal(loginViewModel.RememberMe, result.RouteValues["RememberMe"]);
        }

        [Fact]
        public async void Login_SignInManagerPasswordSignInLockedOut_ReturnsLockOutView()
        {
            //Arrange
            var loginViewModel = new LoginViewModel
            {
                Email = "",
                Password = "",
                RememberMe = true
            };
            _signInManagerMock.PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns(SignInResult.LockedOut);

            //Act
            var result = await _uut.Login(loginViewModel) as ViewResult;

            //Assert
            Assert.Equal("Lockout", result.ViewName);
        }

        [Fact]
        public async void Login_SignInManagerPasswordSignInFailed_ModelErrorAdded()
        {
            //Arrange
            var loginViewModel = new LoginViewModel
            {
                Email = "",
                Password = "",
                RememberMe = true
            };
            _signInManagerMock.PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns(SignInResult.Failed);

            //Act
            var result = await _uut.Login(loginViewModel) as ViewResult;

            //Assert
            Assert.Equal("Invalid login attempt.", result.ViewData.ModelState.Root.Errors.FirstOrDefault().ErrorMessage);
        }

        [Fact]
        public async void Login_SignInManagerPasswordSignInFailed_DefaultViewReturned()
        {
            //Arrange
            var loginViewModel = new LoginViewModel
            {
                Email = "",
                Password = "",
                RememberMe = true
            };
            _signInManagerMock.PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns(SignInResult.Failed);

            //Act
            var result = await _uut.Login(loginViewModel) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void Login_SignInManagerPasswordSignInFailed_ViewWithSameModelReturned()
        {
            //Arrange
            var loginViewModel = new LoginViewModel
            {
                Email = "",
                Password = "",
                RememberMe = true
            };
            _signInManagerMock.PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns(SignInResult.Failed);

            //Act
            var result = await _uut.Login(loginViewModel) as ViewResult;
            var originalViewModel = JsonConvert.SerializeObject(loginViewModel);
            var viewModelReturnedToView = JsonConvert.SerializeObject(result.Model);

            //Assert
            Assert.Equal(originalViewModel, viewModelReturnedToView);
        }
    }
}
