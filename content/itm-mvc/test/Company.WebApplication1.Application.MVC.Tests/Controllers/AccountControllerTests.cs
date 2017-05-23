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
using Company.WebApplication1.Application.MVC.Tests.Helpers;
using Xunit;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult; //Resolves ambigious classes for SignInResult
using Microsoft.AspNetCore.Http.Authentication;
using System.Security.Claims;

namespace Company.WebApplication1.Application.MVC.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly UserManager<ApplicationUser> _userManagerMock;
        private readonly AccountController _uut;
        private readonly SignInManager<ApplicationUser> _signInManagerMock;
        private readonly IEmailSender _emailMock;
        private readonly ISmsSender _smsMock;

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

            _emailMock = Substitute.For<IEmailSender>();
            _smsMock = Substitute.For<ISmsSender>();
            var loggerFactoryMock = Substitute.For<ILoggerFactory>();
            var loggerMockCtrl = Substitute.For<ILogger>();
            loggerFactoryMock.CreateLogger("").Returns(loggerMockCtrl);

            _uut = new AccountController(_userManagerMock, _signInManagerMock, cookieOptionsMock, _emailMock, _smsMock, loggerFactoryMock).WithDefaultMocks();
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
            _uut.ModelState.AddModelError("Error", "Error");

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
            _uut.ModelState.AddModelError("Error", "Error");

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
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.Login(default(LoginViewModel)) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void Login_ModelStateNotValid_ViewWithSameModelReturned()
        {
            //Arrange
            _uut.ModelState.AddModelError("Error", "Error");
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
            _uut.ModelState.AddModelError("Error", "Error");

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
        public async void Login_SignInManagerPasswordSignInSuccessAndReturnUrlNotLocal_ReturnsRedirectToIndex()
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
            var result = await _uut.Login(loginViewModel, "123");

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

        //Register
        [Fact]
        public void Register_CalledWithNoReturnUrl_ViewDataContainsNullInReturnUrl()
        {
            //Arrange

            //Act
            var result = _uut.Register() as ViewResult;

            //Assert
            Assert.Null(result.ViewData["ReturnUrl"]);
        }

        [Fact]
        public void Register_CalledWithReturnUrl_ViewDataContainsSameReturnUrl()
        {
            //Arrange
            var returnUrl = "123";

            //Act
            var result = _uut.Register(returnUrl) as ViewResult;

            //Assert
            Assert.Equal(returnUrl, result.ViewData["ReturnUrl"]);
        }

        [Fact]
        public void Register_ReturnsDefaultView()
        {
            //Arrange

            //Act
            var result = _uut.Register(null) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        //Register(model, string)
        [Fact]
        public async void Register_CalledWithNoReturnUrlAndModelStateInvalid_ViewDataContainsNullInReturnUrl()
        {
            //Arrange
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.Register(default(RegisterViewModel)) as ViewResult;

            //Assert
            Assert.Null(result.ViewData["ReturnUrl"]);
        }

        [Fact]
        public async void Register_CalledWithReturnUrlAndModelStateInvalid_ViewDataContainsSameReturnUrl()
        {
            //Arrange
            var returnUrl = "123";
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.Register(null, returnUrl) as ViewResult;

            //Assert
            Assert.Equal(returnUrl, result.ViewData["ReturnUrl"]);
        }

        [Fact]
        public async void Register_CalledWithNoReturnUrlAndModelStateValid_ViewDataContainsNullInReturnUrl()
        {
            //Arrange
            var registerViewModel = new RegisterViewModel
            {
                Email = "",
                Password = "",
                ConfirmPassword = ""
            };
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Failed());

            //Act
            var result = await _uut.Register(registerViewModel) as ViewResult;

            //Assert
            Assert.Null(result.ViewData["ReturnUrl"]);
        }

        [Fact]
        public async void Register_CalledWithReturnUrlAndModelStateValid_ViewDataContainsSameReturnUrl()
        {
            //Arrange
            var returnUrl = "123";
            var registerViewModel = new RegisterViewModel
            {
                Email = "",
                Password = "",
                ConfirmPassword = ""
            };
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Failed());

            //Act
            var result = await _uut.Register(registerViewModel, returnUrl) as ViewResult;

            //Assert
            Assert.Equal(returnUrl, result.ViewData["ReturnUrl"]);
        }

        [Fact]
        public async void Register_ModelStateNotValid_DefaultViewReturned()
        {
            //Arrange
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.Register(default(RegisterViewModel)) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void Register_ModelStateNotValid_ViewWithSameModelReturned()
        {
            //Arrange
            _uut.ModelState.AddModelError("Error", "Error");
            var registerViewModel = new RegisterViewModel
            {
                Email = "",
                Password = "",
                ConfirmPassword = ""
            };

            //Act
            var result = await _uut.Register(registerViewModel) as ViewResult;
            var originalViewModel = JsonConvert.SerializeObject(registerViewModel);
            var viewModelReturnedToView = JsonConvert.SerializeObject(result.Model);

            //Assert
            Assert.Equal(originalViewModel, viewModelReturnedToView);
        }

        [Fact]
        public async void Register_UserManagerCreateAsyncCalled()
        {
            //Arrange
            var registerViewModel = new RegisterViewModel
            {
                Email = "",
                Password = "",
                ConfirmPassword = ""
            };
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Failed());

            //Act
            var result = await _uut.Register(registerViewModel) as ViewResult;

            //Assert
            await _userManagerMock.Received().CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
        }

        [Fact]
        public async void Register_ModelStateNotValid_UserManagerCreateAsyncNotCalled()
        {
            //Arrange
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.Register(default(RegisterViewModel)) as ViewResult;

            //Assert
            await _userManagerMock.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
        }

        [Fact]
        public async void Register_UserManagerCreateAsyncFails_ModelErrorsAdded()
        {
            //Arrange
            var registerViewModel = new RegisterViewModel
            {
                Email = "",
                Password = "",
                ConfirmPassword = ""
            };
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed(new IdentityError{Description = "CreateAsyncErrorForTest"}));

            //Act
            var result = await _uut.Register(registerViewModel) as ViewResult;

            //Assert
            Assert.Equal("CreateAsyncErrorForTest", result.ViewData.ModelState.Root.Errors.FirstOrDefault().ErrorMessage);
        }

        [Fact]
        public async void Register_UserManagerCreateAsyncFails_DefaultViewReturned()
        {
            //Arrange
            var registerViewModel = new RegisterViewModel
            {
                Email = "",
                Password = "",
                ConfirmPassword = ""
            };
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed());

            //Act
            var result = await _uut.Register(registerViewModel) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void Register_UserManagerCreateAsyncFails_ViewWithSameModelReturned()
        {
            //Arrange
            var registerViewModel = new RegisterViewModel
            {
                Email = "",
                Password = "",
                ConfirmPassword = ""
            };
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed());

            //Act
            var result = await _uut.Register(registerViewModel) as ViewResult;
            var originalViewModel = JsonConvert.SerializeObject(registerViewModel);
            var viewModelReturnedToView = JsonConvert.SerializeObject(result.Model);

            //Assert
            Assert.Equal(originalViewModel, viewModelReturnedToView);
        }

        [Fact]
        public async void Register_UserManagerCreateAsyncFails_SignInManagerSignInAsyncNotCalled()
        {
            //Arrange
            var registerViewModel = new RegisterViewModel
            {
                Email = "",
                Password = "",
                ConfirmPassword = ""
            };
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed());

            //Act
            var result = await _uut.Register(registerViewModel) as ViewResult;

            //Assert
            await _signInManagerMock.DidNotReceive().SignInAsync(Arg.Any<ApplicationUser>(), Arg.Any<bool>());
        }

        [Fact]
        public async void Register_UserManagerCreateAsyncSuccess_SignInManagerSignInAsyncCalled()
        {
            //Arrange
            var registerViewModel = new RegisterViewModel
            {
                Email = "",
                Password = "",
                ConfirmPassword = ""
            };
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Success);

            //Act
            var result = await _uut.Register(registerViewModel) as RedirectToActionResult;

            //Assert
            await _signInManagerMock.Received().SignInAsync(Arg.Any<ApplicationUser>(), Arg.Any<bool>());
        }

        [Fact]
        public async void Register_UserManagerCreateAsyncSuccessReturnUrlIsLocal_RedirectResultReturned()
        {
            //Arrange
            var registerViewModel = new RegisterViewModel
            {
                Email = "",
                Password = "",
                ConfirmPassword = ""
            };
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Success);
            _uut.Url.IsLocalUrl(Arg.Any<string>()).Returns(true);

            //Act
            var result = await _uut.Register(registerViewModel, "123");

            //Assert
            Assert.IsType<RedirectResult>(result);
        }

        [Fact]
        public async void Register_UserManagerCreateAsyncSuccessReturnUrlNotLocal_ReturnsRedirectToHomeController()
        {
            //Arrange
            var registerViewModel = new RegisterViewModel
            {
                Email = "",
                Password = "",
                ConfirmPassword = ""
            };
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Success);
            _uut.Url.IsLocalUrl(Arg.Any<string>()).Returns(false);

            //Act
            var result = await _uut.Register(registerViewModel, "123") as RedirectToActionResult;

            //Assert
            Assert.Equal("Home", result.ControllerName);
        }

        [Fact]
        public async void Register_UserManagerCreateAsyncSuccessReturnUrlNotLocal_ReturnsRedirectToActionIndex()
        {
            //Arrange
            var registerViewModel = new RegisterViewModel
            {
                Email = "",
                Password = "",
                ConfirmPassword = ""
            };
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Success);
            _uut.Url.IsLocalUrl(Arg.Any<string>()).Returns(false);

            //Act
            var result = await _uut.Register(registerViewModel, "123") as RedirectToActionResult;

            //Assert
            Assert.Equal("Index", result.ActionName);
        }

        //Logout
        [Fact]
        public async void Logout_SignInManagerSignOutCalled()
        {
            //Arrange

            //Act
            var result = await _uut.Logout() as RedirectToActionResult;

            //Assert
            await _signInManagerMock.Received().SignOutAsync();
        }

        [Fact]
        public async void Logout_ReturnsRedirectToHomeController()
        {
            //Arrange

            //Act
            var result = await _uut.Logout() as RedirectToActionResult;

            //Assert
            Assert.Equal("Home", result.ControllerName);
        }

        [Fact]
        public async void Logout_ReturnsRedirectToActionIndex()
        {
            //Arrange

            //Act
            var result = await _uut.Logout() as RedirectToActionResult;

            //Assert
            Assert.Equal("Index", result.ActionName);
        }

        //ExternalLogin
        [Fact]
        public void ExternalLogin_SignInManagerConfigureExternalAuthenticationPropertiesCalled()
        {
            //Arrange

            //Act
            var result = _uut.ExternalLogin(null);

            //Assert
            _signInManagerMock.Received().ConfigureExternalAuthenticationProperties(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public void ExternalLogin_ChallengeResultReturned()
        {
            //Arrange

            //Act
            var result = _uut.ExternalLogin(null);

            //Assert
            Assert.IsType<ChallengeResult>(result);
        }

        //ExternalLoginCallback
        [Fact]
        public async void ExternalLoginCallback_RemoteError_ModelErrorAdded()
        {
            //Arrange

            //Act
            var result = await _uut.ExternalLoginCallback(remoteError: "error") as ViewResult;

            //Assert
            Assert.Equal("Error from external provider: error", result.ViewData.ModelState.Root.Errors.FirstOrDefault().ErrorMessage);
        }

        [Fact]
        public async void ExternalLoginCallback_RemoteError_ReturnsLoginView()
        {
            //Arrange

            //Act
            var result = await _uut.ExternalLoginCallback(remoteError: "error") as ViewResult;

            //Assert
            Assert.Equal("Login", result.ViewName);
        }

        [Fact]
        public async void ExternalLoginCallback_NoRemoteError_SignInManagerGetExternalLoginInfoAsyncCalled()
        {
            //Arrange

            //Act
            var result = await _uut.ExternalLoginCallback() as ViewResult;

            //Assert
            await _signInManagerMock.Received().GetExternalLoginInfoAsync();
        }

        [Fact]
        public async void ExternalLoginCallback_NoRemoteErrorSignInManagerGetExternalLoginInfoAsyncNull_ReturnsRedirectToActionLogin()
        {
            //Arrange
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(default(ExternalLoginInfo));

            //Act
            var result = await _uut.ExternalLoginCallback() as RedirectToActionResult;

            //Assert
            Assert.Equal("Login", result.ActionName);
        }

        [Fact]
        public async void ExternalLoginCallback_NoRemoteErrorAndReturnUrlIsLocalAndSignInManagerExternalLoginSignInAsyncSuccess_ReturnsRedirectResult()
        {
            //Arrange
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _signInManagerMock.ExternalLoginSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(SignInResult.Success);
            _uut.Url.IsLocalUrl(Arg.Any<string>()).Returns(true);

            //Act
            var result = await _uut.ExternalLoginCallback("123");

            //Assert
            Assert.IsType<RedirectResult>(result);
        }

        [Fact]
        public async void ExternalLoginCallback_NoRemoteErrorAndReturnUrlNotLocalAndSignInManagerExternalLoginSignInAsyncSuccess_ReturnsRedirectToHomeController()
        {
            //Arrange
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _signInManagerMock.ExternalLoginSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(SignInResult.Success);
            _uut.Url.IsLocalUrl(Arg.Any<string>()).Returns(false);

            //Act
            var result = await _uut.ExternalLoginCallback("123") as RedirectToActionResult;

            //Assert
            Assert.Equal("Home", result.ControllerName);
        }

        [Fact]
        public async void ExternalLoginCallback_NoRemoteErrorAndReturnUrlNotLocalAndSignInManagerExternalLoginSignInAsyncSuccess_ReturnsRedirectToActionIndex()
        {
            //Arrange
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _signInManagerMock.ExternalLoginSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(SignInResult.Success);
            _uut.Url.IsLocalUrl(Arg.Any<string>()).Returns(false);

            //Act
            var result = await _uut.ExternalLoginCallback("123") as RedirectToActionResult;

            //Assert
            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public async void ExternalLoginCallback_NoRemoteErrorAndSignInManagerExternalLoginSignInAsyncRequiresTwoFactor_ReturnsRedirectToActionSendCode()
        {
            //Arrange
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _signInManagerMock.ExternalLoginSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(SignInResult.TwoFactorRequired);

            //Act
            var result = await _uut.ExternalLoginCallback() as RedirectToActionResult;

            //Assert
            Assert.Equal("SendCode", result.ActionName);
        }

        [Fact]
        public async void ExternalLoginCallback_NoRemoteErrorAndSignInManagerExternalLoginSignInAsyncRequiresTwoFactor_ReturnsRedirectToActionWithCorrectRouteParams()
        {
            //Arrange
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _signInManagerMock.ExternalLoginSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(SignInResult.TwoFactorRequired);

            //Act
            var result = await _uut.ExternalLoginCallback("123") as RedirectToActionResult;

            //Assert
            Assert.Equal("123", result.RouteValues["ReturnUrl"]);
        }

        [Fact]
        public async void ExternalLoginCallback_NoRemoteErrorAndSignInManagerExternalLoginSignInAsyncLockout_ReturnsLockoutView()
        {
            //Arrange
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _signInManagerMock.ExternalLoginSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(SignInResult.LockedOut);

            //Act
            var result = await _uut.ExternalLoginCallback() as ViewResult;

            //Assert
            Assert.Equal("Lockout", result.ViewName);
        }

        [Fact]
        public async void ExternalLoginCallback_NoRemoteErrorAndSignInManagerExternalLoginSignInAsyncFailed_ReturnsExternalLoginConfirmationView()
        {
            //Arrange
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _signInManagerMock.ExternalLoginSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(SignInResult.Failed);

            //Act
            var result = await _uut.ExternalLoginCallback() as ViewResult;

            //Assert
            Assert.Equal("ExternalLoginConfirmation", result.ViewName);
        }

        //ExternalLoginConfirmation
        [Fact]
        public async void ExternalLoginConfirmation_ModelStateNotValid_ReturnsDefaultView()
        {
            //Arrange
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.ExternalLoginConfirmation(null) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void ExternalLoginConfirmation_ModelStateNotValid_ReturnsViewWithSameModel()
        {
            //Arrange
            var externalLoginConfirmationViewModel = new ExternalLoginConfirmationViewModel
            {
                Email = "123"
            };
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.ExternalLoginConfirmation(externalLoginConfirmationViewModel) as ViewResult;
            var originalViewModel = JsonConvert.SerializeObject(externalLoginConfirmationViewModel);
            var viewModelReturnedToView = JsonConvert.SerializeObject(result.Model);

            //Assert
            Assert.Equal(originalViewModel, viewModelReturnedToView);
        }

        [Fact]
        public async void ExternalLoginConfirmation_UserManagerCreateAsyncError_ModelErrorsAdded()
        {
            //Arrange
            var externalLoginConfirmationViewModel = new ExternalLoginConfirmationViewModel
            {
                Email = ""
            };
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>())
                .Returns(IdentityResult.Failed(new IdentityError{Description = "CreateAsyncErrorForTest"}));

            //Act
            var result = await _uut.ExternalLoginConfirmation(externalLoginConfirmationViewModel) as ViewResult;

            //Assert
            Assert.Equal("CreateAsyncErrorForTest", result.ViewData.ModelState.Root.Errors.FirstOrDefault().ErrorMessage);
        }

        [Fact]
        public async void ExternalLoginConfirmation_UserManagerCreateAsyncError_DefaultViewReturned()
        {
            //Arrange
            var externalLoginConfirmationViewModel = new ExternalLoginConfirmationViewModel
            {
                Email = ""
            };
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>())
                .Returns(IdentityResult.Failed());

            //Act
            var result = await _uut.ExternalLoginConfirmation(externalLoginConfirmationViewModel) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void ExternalLoginConfirmation_UserManagerCreateAsyncError_SignInManagerSignInNotCalled()
        {
            //Arrange
            var externalLoginConfirmationViewModel = new ExternalLoginConfirmationViewModel
            {
                Email = ""
            };
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>())
                .Returns(IdentityResult.Failed());

            //Act
            var result = await _uut.ExternalLoginConfirmation(externalLoginConfirmationViewModel) as ViewResult;

            //Assert
            await _signInManagerMock.DidNotReceive().SignInAsync(Arg.Any<ApplicationUser>(),Arg.Any<bool>());
        }

        [Fact]
        public async void ExternalLoginConfirmation_UserManagerAddLoginError_ModelErrorsAdded()
        {
            //Arrange
            var externalLoginConfirmationViewModel = new ExternalLoginConfirmationViewModel
            {
                Email = ""
            };
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);
            _userManagerMock.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>())
                .Returns(IdentityResult.Failed(new IdentityError{Description = "AddLoginAsyncErrorForTest"}));

            //Act
            var result = await _uut.ExternalLoginConfirmation(externalLoginConfirmationViewModel) as ViewResult;

            //Assert
            Assert.Equal("AddLoginAsyncErrorForTest", result.ViewData.ModelState.Root.Errors.FirstOrDefault().ErrorMessage);
        }

        [Fact]
        public async void ExternalLoginConfirmation_UserManagerAddLoginError_DefaultViewReturned()
        {
            //Arrange
            var externalLoginConfirmationViewModel = new ExternalLoginConfirmationViewModel
            {
                Email = ""
            };
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);
            _userManagerMock.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>())
                .Returns(IdentityResult.Failed());

            //Act
            var result = await _uut.ExternalLoginConfirmation(externalLoginConfirmationViewModel) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void ExternalLoginConfirmation_UserManagerAddLoginError_SignInManagerSignInNotCalled()
        {
            //Arrange
            var externalLoginConfirmationViewModel = new ExternalLoginConfirmationViewModel
            {
                Email = ""
            };
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);
            _userManagerMock.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>())
                .Returns(IdentityResult.Failed());

            //Act
            var result = await _uut.ExternalLoginConfirmation(externalLoginConfirmationViewModel) as ViewResult;

            //Assert
            await _signInManagerMock.DidNotReceive().SignInAsync(Arg.Any<ApplicationUser>(), Arg.Any<bool>());
        }

        [Fact]
        public async void ExternalLoginConfirmation_NoErrors_SignInManagerSignInCalled()
        {
            //Arrange
            var externalLoginConfirmationViewModel = new ExternalLoginConfirmationViewModel
            {
                Email = ""
            };
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);
            _userManagerMock.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>())
                .Returns(IdentityResult.Success);

            //Act
            var result = await _uut.ExternalLoginConfirmation(externalLoginConfirmationViewModel) as ViewResult;

            //Assert
            await _signInManagerMock.Received().SignInAsync(Arg.Any<ApplicationUser>(),Arg.Any<bool>());
        }

        [Fact]
        public async void ExternalLoginConfirmation_NoErrorsReturnUrlIsLocal_ReturnsRedirectResult()
        {
            //Arrange
            var externalLoginConfirmationViewModel = new ExternalLoginConfirmationViewModel
            {
                Email = ""
            };
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);
            _userManagerMock.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>())
                .Returns(IdentityResult.Success);
            _uut.Url.IsLocalUrl(Arg.Any<string>()).Returns(true);

            //Act
            var result = await _uut.ExternalLoginConfirmation(externalLoginConfirmationViewModel, "123");

            //Assert
            Assert.IsType<RedirectResult>(result);
        }

        [Fact]
        public async void ExternalLoginConfirmation_NoErrorsReturnUrlNotLocal_ReturnsRedirectToHomeController()
        {
            //Arrange
            var externalLoginConfirmationViewModel = new ExternalLoginConfirmationViewModel
            {
                Email = ""
            };
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);
            _userManagerMock.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>())
                .Returns(IdentityResult.Success);
            _uut.Url.IsLocalUrl(Arg.Any<string>()).Returns(false);

            //Act
            var result = await _uut.ExternalLoginConfirmation(externalLoginConfirmationViewModel, "123") as RedirectToActionResult;

            //Assert
            Assert.Equal("Home", result.ControllerName);
        }

        [Fact]
        public async void ExternalLoginConfirmation_NoErrorsReturnUrlNotLocal_ReturnsRedirectToActionIndex()
        {
            //Arrange
            var externalLoginConfirmationViewModel = new ExternalLoginConfirmationViewModel
            {
                Email = ""
            };
            _signInManagerMock.GetExternalLoginInfoAsync().Returns(new ExternalLoginInfo(new ClaimsPrincipal(), "", "", ""));
            _userManagerMock.CreateAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);
            _userManagerMock.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>())
                .Returns(IdentityResult.Success);
            _uut.Url.IsLocalUrl(Arg.Any<string>()).Returns(false);

            //Act
            var result = await _uut.ExternalLoginConfirmation(externalLoginConfirmationViewModel, "123") as RedirectToActionResult;

            //Assert
            Assert.Equal("Index", result.ActionName);
        }

        //ConfirmEmail
        [Fact]
        public async void ConfirmEmail_UserIdNull_ReturnsErrorView()
        {
            //Arrange

            //Act
            var result = await _uut.ConfirmEmail(null, "") as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public async void ConfirmEmail_CodeNull_ReturnsErrorView()
        {
            //Arrange

            //Act
            var result = await _uut.ConfirmEmail("", null) as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public async void ConfirmEmail_CodeNullAndUserIdNull_ReturnsErrorView()
        {
            //Arrange

            //Act
            var result = await _uut.ConfirmEmail(null, null) as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public async void ConfirmEmail_UserIsNull_ReturnsErrorView()
        {
            //Arrange
            _userManagerMock.FindByIdAsync(Arg.Any<string>()).Returns(default(ApplicationUser));

            //Act
            var result = await _uut.ConfirmEmail("", "") as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public async void ConfirmEmail_UserManagerConfirmEmailFails_ReturnsErrorView()
        {
            //Arrange
            _userManagerMock.FindByIdAsync(Arg.Any<string>()).Returns(new ApplicationUser());
            _userManagerMock.ConfirmEmailAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Failed());

            //Act
            var result = await _uut.ConfirmEmail("", "") as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public async void ConfirmEmail_UserManagerConfirmEmailSuccess_ReturnsConfirmEmailView()
        {
            //Arrange
            _userManagerMock.FindByIdAsync(Arg.Any<string>()).Returns(new ApplicationUser());
            _userManagerMock.ConfirmEmailAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);

            //Act
            var result = await _uut.ConfirmEmail("", "") as ViewResult;

            //Assert
            Assert.Equal("ConfirmEmail", result.ViewName);
        }

        //ForgotPassword
        [Fact]
        public void ForgotPassword_ReturnsDefaultView()
        {
            //Arrange

            //Act
            var result = _uut.ForgotPassword() as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        //ForgotPassword(model)
        [Fact]
        public async void ForgotPassword_ModelStateNotValid_ReturnsDefaultView()
        {
            //Arrange
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.ForgotPassword(default(ForgotPasswordViewModel)) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void ForgotPassword_ModelStateNotValid_ReturnsViewWithSameModel()
        {
            //Arrange
            var forgotPasswordViewModel = new ForgotPasswordViewModel
            {
                Email = "123"
            };
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.ForgotPassword(forgotPasswordViewModel) as ViewResult;
            var originalViewModel = JsonConvert.SerializeObject(forgotPasswordViewModel);
            var viewModelReturnedToView = JsonConvert.SerializeObject(result.Model);

            //Assert
            Assert.Equal(originalViewModel, viewModelReturnedToView);
        }

        [Fact]
        public async void ForgotPassword_UnregisteredUser_ReturnsForgotPasswordConfirmationView()
        {
            //Arrange
            var forgotPasswordViewModel = new ForgotPasswordViewModel
            {
                Email = ""
            };
            _userManagerMock.FindByEmailAsync(Arg.Any<string>()).Returns(default(ApplicationUser));

            //Act
            var result = await _uut.ForgotPassword(forgotPasswordViewModel) as ViewResult;

            //Assert
            Assert.Equal("ForgotPasswordConfirmation", result.ViewName);
        }

        [Fact]
        public async void ForgotPassword_EmailNotConfirmed_ReturnsForgotPasswordConfirmationView()
        {
            //Arrange
            var forgotPasswordViewModel = new ForgotPasswordViewModel
            {
                Email = ""
            };
            _userManagerMock.FindByEmailAsync(Arg.Any<string>()).Returns(new ApplicationUser());
            _userManagerMock.IsEmailConfirmedAsync(Arg.Any<ApplicationUser>()).Returns(false);

            //Act
            var result = await _uut.ForgotPassword(forgotPasswordViewModel) as ViewResult;

            //Assert
            Assert.Equal("ForgotPasswordConfirmation", result.ViewName);
        }

        [Fact]
        public async void ForgotPassword_EmailNotConfirmedAndUnregisteredUser_ReturnsForgotPasswordConfirmationView()
        {
            //Arrange
            var forgotPasswordViewModel = new ForgotPasswordViewModel
            {
                Email = ""
            };
            _userManagerMock.FindByEmailAsync(Arg.Any<string>()).Returns(default(ApplicationUser));
            _userManagerMock.IsEmailConfirmedAsync(Arg.Any<ApplicationUser>()).Returns(false);

            //Act
            var result = await _uut.ForgotPassword(forgotPasswordViewModel) as ViewResult;

            //Assert
            Assert.Equal("ForgotPasswordConfirmation", result.ViewName);
        }

        //TODO: If ForgotPassword is activated on application, more tests probably needs to be added

        //ForgotPasswordConfirmation
        [Fact]
        public void ForgotPasswordConfirmation_ReturnsDefaultView()
        {
            //Arrange

            //Act
            var result = _uut.ForgotPasswordConfirmation() as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        //ResetPassword
        [Fact]
        public void ResetPassword_CodeIsNull_ReturnsErrorView()
        {
            //Arrange

            //Act
            var result = _uut.ResetPassword() as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public void ResetPassword_CodeIsNotNull_ReturnsdefaultView()
        {
            //Arrange

            //Act
            var result = _uut.ResetPassword("") as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        //ResetPassword(model)
        [Fact]
        public async void ResetPassword_ModelStateNotValid_ReturnsDefaultView()
        {
            //Arrange
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.ResetPassword(default(ResetPasswordViewModel)) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void ResetPassword_ModelStateNotValid_ReturnsViewWithSameModel()
        {
            //Arrange
            var resetPasswordViewModel = new ResetPasswordViewModel
            {
                Email = "123",
                Password = "123",
                ConfirmPassword = "123",
                Code = "123"
            };
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.ResetPassword(resetPasswordViewModel) as ViewResult;
            var originalViewModel = JsonConvert.SerializeObject(resetPasswordViewModel);
            var viewModelReturnedToView = JsonConvert.SerializeObject(result.Model);

            //Assert
            Assert.Equal(originalViewModel, viewModelReturnedToView);
        }

        [Fact]
        public async void ResetPassword_UnregisteredUser_ReturnsRedirectToResetpasswordConfirmationAction()
        {
            //Arrange
            var resetPasswordViewModel = new ResetPasswordViewModel
            {
                Email = ""
            };
            _userManagerMock.FindByEmailAsync(Arg.Any<string>()).Returns(default(ApplicationUser));

            //Act
            var result = await _uut.ResetPassword(resetPasswordViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal("ResetPasswordConfirmation", result.ActionName);
        }

        [Fact]
        public async void ResetPassword_ResetPasswordFail_ModelErrorsAdded()
        {
            //Arrange
            var resetPasswordViewModel = new ResetPasswordViewModel
            {
                Email = "",
                Code = "",
                Password = ""
            };
            _userManagerMock.FindByEmailAsync(Arg.Any<string>()).Returns(new ApplicationUser());
            _userManagerMock.ResetPasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed(new IdentityError{Description = "ResetPasswordAsyncErrorForTest"}));

            //Act
            var result = await _uut.ResetPassword(resetPasswordViewModel) as ViewResult;

            //Assert
            Assert.Equal("ResetPasswordAsyncErrorForTest", result.ViewData.ModelState.Root.Errors.FirstOrDefault().ErrorMessage);
        }

        [Fact]
        public async void ResetPassword_ResetPasswordFail_DefaultViewReturned()
        {
            //Arrange
            var resetPasswordViewModel = new ResetPasswordViewModel
            {
                Email = "",
                Code = "",
                Password = ""
            };
            _userManagerMock.FindByEmailAsync(Arg.Any<string>()).Returns(new ApplicationUser());
            _userManagerMock.ResetPasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed());

            //Act
            var result = await _uut.ResetPassword(resetPasswordViewModel) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void ResetPassword_NoErrors_ReturnsRedirectToResetpasswordConfirmationAction()
        {
            //Arrange
            var resetPasswordViewModel = new ResetPasswordViewModel
            {
                Email = "",
                Code = "",
                Password = ""
            };
            _userManagerMock.FindByEmailAsync(Arg.Any<string>()).Returns(new ApplicationUser());
            _userManagerMock.ResetPasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(IdentityResult.Success);

            //Act
            var result = await _uut.ResetPassword(resetPasswordViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal("ResetPasswordConfirmation", result.ActionName);
        }

        //ResetPasswordConfirmation
        [Fact]
        public void ResetPasswordConfirmation_ReturnsDefaultView()
        {
            //Arrange

            //Act
            var result = _uut.ResetPasswordConfirmation() as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        //SendCode
        [Fact]
        public async void SendCode_UnregisteredUser_ReturnsErrorView()
        {
            //Arrange
            _signInManagerMock.GetTwoFactorAuthenticationUserAsync().Returns(default(ApplicationUser));

            //Act
            var result = await _uut.SendCode() as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public async void SendCode_ReturnsDefaultView()
        {
            //Arrange
            _signInManagerMock.GetTwoFactorAuthenticationUserAsync().Returns(new ApplicationUser());

            //Act
            var result = await _uut.SendCode() as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void SendCode_ReturnsViewWithSendCodeViewModel()
        {
            //Arrange
            _signInManagerMock.GetTwoFactorAuthenticationUserAsync().Returns(new ApplicationUser());

            //Act
            var result = await _uut.SendCode() as ViewResult;

            //Assert
            Assert.IsType<SendCodeViewModel>(result.Model);
        }

        //SendCode(model)
        [Fact]
        public async void SendCode_ModelStateNotValid_DefaultViewReturned()
        {
            //Arrange
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.SendCode(default(SendCodeViewModel)) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void SendCode_UnregisteredUser_ErrorViewReturned()
        {
            //Arrange
            _signInManagerMock.GetTwoFactorAuthenticationUserAsync().Returns(default(ApplicationUser));

            //Act
            var result = await _uut.SendCode(default(SendCodeViewModel)) as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public async void SendCode_EmptyTwoFactorToken_ErrorViewReturned()
        {
            //Arrange
            var sendCodeViewModel = new SendCodeViewModel
            {
                SelectedProvider = ""
            };
            _signInManagerMock.GetTwoFactorAuthenticationUserAsync().Returns(new ApplicationUser());
            _userManagerMock.GenerateTwoFactorTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns("");

            //Act
            var result = await _uut.SendCode(sendCodeViewModel) as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public async void SendCode_SelectedProviderIsEmail_EmailSenderSendEmailCalled()
        {
            //Arrange
            var sendCodeViewModel = new SendCodeViewModel
            {
                SelectedProvider = "Email"
            };
            _signInManagerMock.GetTwoFactorAuthenticationUserAsync().Returns(new ApplicationUser());
            _userManagerMock.GenerateTwoFactorTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns("1");
            _userManagerMock.GetEmailAsync(Arg.Any<ApplicationUser>()).Returns("");

            //Act
            var result = await _uut.SendCode(sendCodeViewModel) as ViewResult;

            //Assert
            await _emailMock.Received().SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async void SendCode_SelectedProviderIsPhone_SmsSenderSendSmsCalled()
        {
            //Arrange
            var sendCodeViewModel = new SendCodeViewModel
            {
                SelectedProvider = "Phone"
            };
            _signInManagerMock.GetTwoFactorAuthenticationUserAsync().Returns(new ApplicationUser());
            _userManagerMock.GenerateTwoFactorTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns("1");
            _userManagerMock.GetPhoneNumberAsync(Arg.Any<ApplicationUser>()).Returns("");

            //Act
            var result = await _uut.SendCode(sendCodeViewModel) as ViewResult;

            //Assert
            await _smsMock.Received().SendSmsAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async void SendCode_ReturnsRedirectToActionVerifyCode()
        {
            //Arrange
            var sendCodeViewModel = new SendCodeViewModel
            {
                SelectedProvider = "",
                ReturnUrl = "",
                RememberMe = true
            };
            _signInManagerMock.GetTwoFactorAuthenticationUserAsync().Returns(new ApplicationUser());
            _userManagerMock.GenerateTwoFactorTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns("1");

            //Act
            var result = await _uut.SendCode(sendCodeViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal("VerifyCode", result.ActionName);
        }

        //VerifyCode
        [Fact]
        public async void VerifyCode_UnregisteredUser_ReturnsErrorView()
        {
            //Arrange
            _signInManagerMock.GetTwoFactorAuthenticationUserAsync().Returns(default(ApplicationUser));

            //Act
            var result = await _uut.VerifyCode("", true) as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public async void VerifyCode_ReturnsDefaultView()
        {
            //Arrange
            _signInManagerMock.GetTwoFactorAuthenticationUserAsync().Returns(new ApplicationUser());

            //Act
            var result = await _uut.VerifyCode("", true) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void VerifyCode_ReturnsViewWithCorrectViewModel()
        {
            //Arrange
            var verifyCodeViewModel = new VerifyCodeViewModel
            {
                Provider = "123",
                ReturnUrl = "123",
                RememberMe = true
            };
            _signInManagerMock.GetTwoFactorAuthenticationUserAsync().Returns(new ApplicationUser());

            //Act
            var result = await _uut.VerifyCode(verifyCodeViewModel.Provider, verifyCodeViewModel.RememberMe, verifyCodeViewModel.ReturnUrl) as ViewResult;
            var originalViewModel = JsonConvert.SerializeObject(verifyCodeViewModel);
            var viewModelReturnedToView = JsonConvert.SerializeObject(result.Model);

            //Assert
            Assert.Equal(originalViewModel, viewModelReturnedToView);
        }

        //VerifyCode(model)
        [Fact]
        public async void VerifyCode_ModelStateNotValid_ReturnsDefaultView()
        {
            //Arrange
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.VerifyCode(default(VerifyCodeViewModel)) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void VerifyCode_ModelStateNotValid_ReturnsViewWithSameModel()
        {
            //Arrange
            _uut.ModelState.AddModelError("Error", "Error");
            var verifyCodeViewModel = new VerifyCodeViewModel
            {
                Provider = "123",
                ReturnUrl = "123",
                RememberMe = true
            };

            //Act
            var result = await _uut.VerifyCode(verifyCodeViewModel) as ViewResult;
            var originalViewModel = JsonConvert.SerializeObject(verifyCodeViewModel);
            var viewModelReturnedToView = JsonConvert.SerializeObject(result.Model);

            //Assert
            Assert.Equal(originalViewModel, viewModelReturnedToView);
        }

        [Fact]
        public async void VerifyCode_TwoFactorSignInAsyncSuccessAndReturnUrlIsLocal_ReturnsRedirectResult()
        {
            //Arrange
            var verifyCodeViewModel = new VerifyCodeViewModel
            {
                Provider = "",
                ReturnUrl = "123",
                RememberMe = true
            };
            _signInManagerMock.TwoFactorSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>())
                .Returns(SignInResult.Success);
            _uut.Url.IsLocalUrl(Arg.Any<string>()).Returns(true);

            //Act
            var result = await _uut.VerifyCode(verifyCodeViewModel);

            //Assert
            Assert.IsType<RedirectResult>(result);
        }

        [Fact]
        public async void VerifyCode_TwoFactorSignInAsyncSuccessAndReturnUrlNotLocal_ReturnsRedirectToHomeController()
        {
            //Arrange
            var verifyCodeViewModel = new VerifyCodeViewModel
            {
                Provider = "",
                ReturnUrl = "123",
                RememberMe = true
            };
            _signInManagerMock.TwoFactorSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>())
                .Returns(SignInResult.Success);
            _uut.Url.IsLocalUrl(Arg.Any<string>()).Returns(false);

            //Act
            var result = await _uut.VerifyCode(verifyCodeViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal("Home", result.ControllerName);
        }

        [Fact]
        public async void VerifyCode_TwoFactorSignInAsyncSuccessAndReturnUrlNotLocal_ReturnsRedirectToActionIndex()
        {
            //Arrange
            var verifyCodeViewModel = new VerifyCodeViewModel
            {
                Provider = "",
                ReturnUrl = "123",
                RememberMe = true
            };
            _signInManagerMock.TwoFactorSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>())
                .Returns(SignInResult.Success);
            _uut.Url.IsLocalUrl(Arg.Any<string>()).Returns(false);

            //Act
            var result = await _uut.VerifyCode(verifyCodeViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public async void VerifyCode_TwoFactorSignInAsyncLockOut_ReturnsLockedOutView()
        {
            //Arrange
            var verifyCodeViewModel = new VerifyCodeViewModel
            {
                Provider = "",
                ReturnUrl = "",
                RememberMe = true
            };
            _signInManagerMock.TwoFactorSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>())
                .Returns(SignInResult.LockedOut);

            //Act
            var result = await _uut.VerifyCode(verifyCodeViewModel) as ViewResult;

            //Assert
            Assert.Equal("Lockout", result.ViewName);
        }

        [Fact]
        public async void VerifyCode_TwoFactorSignInAsyncFail_ModelErrorsAdded()
        {
            //Arrange
            var verifyCodeViewModel = new VerifyCodeViewModel
            {
                Provider = "",
                ReturnUrl = "",
                RememberMe = true
            };
            _signInManagerMock.TwoFactorSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>())
                .Returns(SignInResult.Failed);

            //Act
            var result = await _uut.VerifyCode(verifyCodeViewModel) as ViewResult;

            //Assert
            Assert.Equal("Invalid code.", result.ViewData.ModelState.Root.Errors.FirstOrDefault().ErrorMessage);
        }

        [Fact]
        public async void VerifyCode_TwoFactorSignInAsyncFail_DefaultViewReturned()
        {
            //Arrange
            var verifyCodeViewModel = new VerifyCodeViewModel
            {
                Provider = "",
                ReturnUrl = "",
                RememberMe = true
            };
            _signInManagerMock.TwoFactorSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>())
                .Returns(SignInResult.Failed);

            //Act
            var result = await _uut.VerifyCode(verifyCodeViewModel) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void VerifyCode_TwoFactorSignInAsyncFail_ViewWithSameModelReturned()
        {
            //Arrange
            var verifyCodeViewModel = new VerifyCodeViewModel
            {
                Provider = "123",
                ReturnUrl = "123",
                RememberMe = true
            };
            _signInManagerMock.TwoFactorSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>())
                .Returns(SignInResult.Failed);

            //Act
            var result = await _uut.VerifyCode(verifyCodeViewModel) as ViewResult;
            var originalViewModel = JsonConvert.SerializeObject(verifyCodeViewModel);
            var viewModelReturnedToView = JsonConvert.SerializeObject(result.Model);

            //Assert
            Assert.Equal(originalViewModel, viewModelReturnedToView);
        }

        //AccessDenied
        [Fact]
        public void AccessDenied_DefaultViewReturned()
        {
            //Arrange

            //Act
            var result = _uut.AccessDenied() as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }
    }
}
