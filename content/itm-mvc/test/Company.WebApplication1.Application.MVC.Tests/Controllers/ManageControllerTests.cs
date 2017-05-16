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
using Microsoft.AspNetCore.Http.Authentication;
using System.Linq;

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

        //DisableTwoFactorAuthentication
        [Fact]
        public async void DisableTwoFactorAuthentication_UnregisteredUser_UserManagerTwoFactorEnableIsNotCalled()
        {
            //Arrange
            var httpContext = Substitute.For<HttpContext>();

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            ApplicationUser nullUser = null;
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult(nullUser));
            _uut.Url = Substitute.For<IUrlHelper>();

            //Act
            var result = await _uut.DisableTwoFactorAuthentication();

            //Assert
            await _userManagerMock.DidNotReceive().SetTwoFactorEnabledAsync(Arg.Any<ApplicationUser>(),Arg.Any<bool>());
        }

        [Fact]
        public async void DisableTwoFactorAuthentication_ValidUser_UserManagerTwoFactorEnableIsCalled()
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
            _uut.Url = Substitute.For<IUrlHelper>();

            //Act
            var result = await _uut.DisableTwoFactorAuthentication();

            //Assert
            await _userManagerMock.Received().SetTwoFactorEnabledAsync(Arg.Any<ApplicationUser>(),false);
        }

        //VerifyPhoneNumber(string)
        [Fact]
        public async void VerifyPhoneNumber_UnregisteredUser_ReturnsErrorView()
        {
            //Arrange
            var httpContext = Substitute.For<HttpContext>();

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            ApplicationUser nullUser = null;
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult(nullUser));
            _uut.TempData = Substitute.For<ITempDataDictionary>();

            //Act
            var result = await _uut.VerifyPhoneNumber("12345678") as ViewResult;

            //Assert
            Assert.Equal("Error",result.ViewName);
        }

        [Fact]
        public async void VerifyPhoneNumber_ValidUserPhoneNumberNull_ReturnsErrorView()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();

            //Act
            var result = await _uut.VerifyPhoneNumber(null as string) as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public async void VerifyPhoneNumber_ValidUserPhoneNumberValid_ReturnsDefaultView()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();

            //Act
            var result = await _uut.VerifyPhoneNumber("12345678") as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        //VerifyPhoneNumber(model)
        [Fact]
        public async void VerifyPhoneNumber_ModelStateNotValid_DoesntChangePhoneNumber()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            _uut.ModelState.AddModelError("Error","Error");
            var verifyPhoneNumberViewModel = new VerifyPhoneNumberViewModel
            {
                PhoneNumber = "12345678",
                Code = "1234"
            };

            //Act
            var result = await _uut.VerifyPhoneNumber(verifyPhoneNumberViewModel) as ViewResult;

            //Assert
            await _userManagerMock.DidNotReceive().ChangePhoneNumberAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>(),Arg.Any<string>());
        }

        [Fact]
        public async void VerifyPhoneNumber_ModelStateNotValid_DoesntSignUserIn()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            _uut.ModelState.AddModelError("Error","Error");
            var verifyPhoneNumberViewModel = new VerifyPhoneNumberViewModel
            {
                PhoneNumber = "12345678",
                Code = "1234"
            };

            //Act
            var result = await _uut.VerifyPhoneNumber(verifyPhoneNumberViewModel) as ViewResult;

            //Assert
            await _signInManagerMock.DidNotReceive().SignInAsync(Arg.Any<ApplicationUser>(),Arg.Any<bool>(),Arg.Any<string>());
        }

        [Fact]
        public async void VerifyPhoneNumber_ModelStateValidUserValidChangePhoneNumberSuccess_RedirectResultReturnedWithPhoneChangeSuccessMessage()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            var verifyPhoneNumberViewModel = new VerifyPhoneNumberViewModel
            {
                PhoneNumber = "12345678",
                Code = "1234"
            };
            _userManagerMock.ChangePhoneNumberAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>(),Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Success));
            _uut.Url = Substitute.For<IUrlHelper>();

            //Act
            var result = await _uut.VerifyPhoneNumber(verifyPhoneNumberViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.AddPhoneSuccess,result.RouteValues["message"]);
        }

        [Fact]
        public async void VerifyPhoneNumber_ModelStateValidUnregisteredUser_ModelErrorAdded()
        {
            //Arrange
            var httpContext = Substitute.For<HttpContext>();

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            ApplicationUser nullUser = null;
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult(nullUser));
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            var verifyPhoneNumberViewModel = new VerifyPhoneNumberViewModel
            {
                PhoneNumber = "12345678",
                Code = "1234"
            };

            //Act
            var result = await _uut.VerifyPhoneNumber(verifyPhoneNumberViewModel) as ViewResult;

            //Assert
            Assert.Equal("Failed to verify phone number",result.ViewData.ModelState.Root.Errors.FirstOrDefault().ErrorMessage);
        }

        [Fact]
        public async void VerifyPhoneNumber_ModelStateValidUserValidChangePhoneNumberError_ModelErrorAdded()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            var verifyPhoneNumberViewModel = new VerifyPhoneNumberViewModel
            {
                PhoneNumber = "12345678",
                Code = "1234"
            };
            _userManagerMock.ChangePhoneNumberAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>(),Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Failed()));
            _uut.Url = Substitute.For<IUrlHelper>();

            //Act
            var result = await _uut.VerifyPhoneNumber(verifyPhoneNumberViewModel) as ViewResult;

            //Assert
            Assert.Equal("Failed to verify phone number",result.ViewData.ModelState.Root.Errors.FirstOrDefault().ErrorMessage);
        }

        [Fact]
        public async void VerifyPhoneNumber_ModelStateValidUserValidChangePhoneNumberError_SignInNotCalled()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            var verifyPhoneNumberViewModel = new VerifyPhoneNumberViewModel
            {
                PhoneNumber = "12345678",
                Code = "1234"
            };
            _userManagerMock.ChangePhoneNumberAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>(),Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Failed()));
            _uut.Url = Substitute.For<IUrlHelper>();

            //Act
            var result = await _uut.VerifyPhoneNumber(verifyPhoneNumberViewModel) as ViewResult;

            //Assert
            await _signInManagerMock.DidNotReceive().SignInAsync(Arg.Any<ApplicationUser>(),Arg.Any<bool>());
        }

        //RemovePhoneNumber
        [Fact]
        public async void RemovePhoneNumber_UnregisteredUser_ReturnsRedirectWithErrorMessage()
        {
            //Arrange
            var httpContext = Substitute.For<HttpContext>();

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            ApplicationUser nullUser = null;
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult(nullUser));
            _uut.Url = Substitute.For<IUrlHelper>();

            //Act
            var result = await _uut.RemovePhoneNumber() as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.Error,result.RouteValues["message"]);
        }

        [Fact]
        public async void RemovePhoneNumber_ValidUserSetPhoneNumberError_ReturnsRedirectWithErrorMessage()
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
            _uut.Url = Substitute.For<IUrlHelper>();
            _userManagerMock.SetPhoneNumberAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Failed()));

            //Act
            var result = await _uut.RemovePhoneNumber() as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.Error,result.RouteValues["message"]);
        }

        [Fact]
        public async void RemovePhoneNumber_ValidUserSetPhoneNumberSuccess_ReturnsRedirectWithSuccessMessage()
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
            _uut.Url = Substitute.For<IUrlHelper>();
            _userManagerMock.SetPhoneNumberAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Success));

            //Act
            var result = await _uut.RemovePhoneNumber() as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.RemovePhoneSuccess,result.RouteValues["message"]);
        }

        //ChangePassword
        [Fact]
        public void ChangePassword_ReturnsDefaultView()
        {
            //Arrange

            //Act
            var result = _uut.ChangePassword() as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        //ChangePassword(model)
        [Fact]
        public async void ChangePassword_ModelStateNotValid_UserManagerChangePasswordNotCalled()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            var changePasswordViewModel = new ChangePasswordViewModel
            {
                OldPassword = "",
                NewPassword = "",
                ConfirmPassword = ""
            };
            _uut.ModelState.AddModelError("Error","Error");

            //Act
            var result = await _uut.ChangePassword(changePasswordViewModel) as ViewResult;

            //Assert
            await _userManagerMock.DidNotReceive().ChangePasswordAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>(),Arg.Any<string>());
        }

        [Fact]
        public async void ChangePassword_ModelStateNotValid_ReturnsDefaultView()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            var changePasswordViewModel = new ChangePasswordViewModel
            {
                OldPassword = "",
                NewPassword = "",
                ConfirmPassword = ""
            };
            _uut.ModelState.AddModelError("Error","Error");

            //Act
            var result = await _uut.ChangePassword(changePasswordViewModel) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void ChangePassword_ModelStateValidUnregisteredUser_ReturnsRedirectResultWithErrorMessage()
        {
            //Arrange
            var httpContext = Substitute.For<HttpContext>();

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            ApplicationUser nullUser = null;
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult(nullUser));
            _uut.Url = Substitute.For<IUrlHelper>();
            var changePasswordViewModel = new ChangePasswordViewModel
            {
                OldPassword = "",
                NewPassword = "",
                ConfirmPassword = ""
            };

            //Act
            var result = await _uut.ChangePassword(changePasswordViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.Error,result.RouteValues["message"]);
        }

        [Fact]
        public async void ChangePassword_ModelStateValidUserValidChangePasswordError_ModelStateErrorsAdded()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            var changePasswordViewModel = new ChangePasswordViewModel
            {
                OldPassword = "",
                NewPassword = "",
                ConfirmPassword = ""
            };
            _userManagerMock.ChangePasswordAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>(),Arg.Any<string>())
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError{Description = "ChangePasswordErrorForTest"})));

            //Act
            var result = await _uut.ChangePassword(changePasswordViewModel) as ViewResult;

            //Assert
            Assert.Equal("ChangePasswordErrorForTest",result.ViewData.ModelState.Root.Errors.FirstOrDefault().ErrorMessage);
        }

        [Fact]
        public async void ChangePassword_ModelStateValidUserValidChangePasswordError_DefaultViewReturned()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            var changePasswordViewModel = new ChangePasswordViewModel
            {
                OldPassword = "",
                NewPassword = "",
                ConfirmPassword = ""
            };
            _userManagerMock.ChangePasswordAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>(),Arg.Any<string>())
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError{Description = "ChangePasswordErrorForTest"})));

            //Act
            var result = await _uut.ChangePassword(changePasswordViewModel) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void ChangePassword_ModelStateValidUserValidChangePasswordSuccess_SignInManagerSignInCalled()
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
            _uut.Url = Substitute.For<IUrlHelper>();
            var changePasswordViewModel = new ChangePasswordViewModel
            {
                OldPassword = "",
                NewPassword = "",
                ConfirmPassword = ""
            };
            _userManagerMock.ChangePasswordAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>(),Arg.Any<string>())
                .Returns(Task.FromResult(IdentityResult.Success));

            //Act
            var result = await _uut.ChangePassword(changePasswordViewModel) as RedirectToActionResult;

            //Assert
            await _signInManagerMock.Received().SignInAsync(Arg.Any<ApplicationUser>(),Arg.Any<bool>());
        }

        [Fact]
        public async void ChangePassword_ModelStateValidUserValidChangePasswordSuccess_RedirectWithChangePasswordSuccessReturned()
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
            _uut.Url = Substitute.For<IUrlHelper>();
            var changePasswordViewModel = new ChangePasswordViewModel
            {
                OldPassword = "",
                NewPassword = "",
                ConfirmPassword = ""
            };
            _userManagerMock.ChangePasswordAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>(),Arg.Any<string>())
                .Returns(Task.FromResult(IdentityResult.Success));

            //Act
            var result = await _uut.ChangePassword(changePasswordViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.ChangePasswordSuccess,result.RouteValues["message"]);
        }

        //SetPassword
        [Fact]
        public void SetPassword_ReturnsDefaultView()
        {
            //Arrange

            //Act
            var result = _uut.SetPassword() as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        //SetPassword(model)
        [Fact]
        public async void SetPassword_ModelStateNotValid_UserManagerAddPasswordNotCalled()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            var setPasswordViewModel = new SetPasswordViewModel
            {
                NewPassword = "",
                ConfirmPassword = ""
            };
            _uut.ModelState.AddModelError("Error","Error");

            //Act
            var result = await _uut.SetPassword(setPasswordViewModel) as ViewResult;

            //Assert
            await _userManagerMock.DidNotReceive().AddPasswordAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>());
        }

        [Fact]
        public async void SetPassword_ModelStateNotValid_DefaultViewReturned()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            var setPasswordViewModel = new SetPasswordViewModel
            {
                NewPassword = "",
                ConfirmPassword = ""
            };
            _uut.ModelState.AddModelError("Error","Error");

            //Act
            var result = await _uut.SetPassword(setPasswordViewModel) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void SetPassword_ModelStateValidUnregisteredUser_RedirectResultWithErrorReturned()
        {
            //Arrange
            var httpContext = Substitute.For<HttpContext>();

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            ApplicationUser nullUser = null;
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult(nullUser));
            var setPasswordViewModel = new SetPasswordViewModel
            {
                NewPassword = "",
                ConfirmPassword = ""
            };
            _uut.Url = Substitute.For<IUrlHelper>();

            //Act
            var result = await _uut.SetPassword(setPasswordViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.Error,result.RouteValues["message"]);
        }

        [Fact]
        public async void SetPassword_ModelStateValidUserValidAddPasswordError_ModelStateErrorsAdded()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            var setPasswordViewModel = new SetPasswordViewModel
            {
                NewPassword = "",
                ConfirmPassword = ""
            };
            _userManagerMock.AddPasswordAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>())
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError{Description = "AddPasswordErrorForTest"})));

            //Act
            var result = await _uut.SetPassword(setPasswordViewModel) as ViewResult;

            //Assert
            Assert.Equal("AddPasswordErrorForTest",result.ViewData.ModelState.Root.Errors.FirstOrDefault().ErrorMessage);
        }

        [Fact]
        public async void SetPassword_ModelStateValidUserValidAddPasswordError_DefaultViewReturned()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            var setPasswordViewModel = new SetPasswordViewModel
            {
                NewPassword = "",
                ConfirmPassword = ""
            };
            _userManagerMock.AddPasswordAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>())
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError{Description = "AddPasswordErrorForTest"})));

            //Act
            var result = await _uut.SetPassword(setPasswordViewModel) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void SetPassword_ModelStateValidUserValidAddPasswordSuccess_SignInManagerSignInCalled()
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
            _uut.Url = Substitute.For<IUrlHelper>();
            var setPasswordViewModel = new SetPasswordViewModel
            {
                NewPassword = "",
                ConfirmPassword = ""
            };
            _userManagerMock.AddPasswordAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>())
                .Returns(Task.FromResult(IdentityResult.Success));

            //Act
            var result = await _uut.SetPassword(setPasswordViewModel) as RedirectToActionResult;

            //Assert
            await _signInManagerMock.Received().SignInAsync(Arg.Any<ApplicationUser>(),Arg.Any<bool>());
        }

        [Fact]
        public async void SetPassword_ModelStateValidUserValidAddPasswordSuccess_RedirectResultWithSetPasswordSuccessReturned()
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
            _uut.Url = Substitute.For<IUrlHelper>();
            var setPasswordViewModel = new SetPasswordViewModel
            {
                NewPassword = "",
                ConfirmPassword = ""
            };
            _userManagerMock.AddPasswordAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>())
                .Returns(Task.FromResult(IdentityResult.Success));

            //Act
            var result = await _uut.SetPassword(setPasswordViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.SetPasswordSuccess,result.RouteValues["message"]);
        }

        //ManageLogins
        [Theory]
        [InlineData(ManageMessageId.RemoveLoginSuccess, "The external login was removed.")]
        [InlineData(ManageMessageId.AddLoginSuccess, "The external login was added.")]
        [InlineData(ManageMessageId.Error, "An error has occurred.")]
        [InlineData(null, "")]
        public async void ManageLogins_GivenMessage_ViewBagContainsCorrectMessage(ManageMessageId? messageId, string message)
        {
            //Arrange
            var httpContext = Substitute.For<HttpContext>();

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            ApplicationUser nullUser = null;
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult(nullUser));
            _uut.TempData = Substitute.For<ITempDataDictionary>();

            //Act
            var result = await _uut.ManageLogins(messageId);

            //Assert
            Assert.Equal(message, _uut.ViewData["StatusMessage"]);
        }

        [Fact]
        public async void ManageLogins_UnregisteredUser_ErrorViewReturned()
        {
            //Arrange
            var httpContext = Substitute.For<HttpContext>();

            _uut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            ApplicationUser nullUser = null;
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult(nullUser));
            _uut.TempData = Substitute.For<ITempDataDictionary>();

            //Act
            var result = await _uut.ManageLogins() as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public async void ManageLogins_ValidUser_DefaultViewReturned()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            IList<UserLoginInfo> list = new List<UserLoginInfo>();
            _userManagerMock.GetLoginsAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(list));
            IEnumerable<AuthenticationDescription> authList = new List<AuthenticationDescription>();
            _signInManagerMock.GetExternalAuthenticationSchemes().Returns(authList);

            //Act
            var result = await _uut.ManageLogins() as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void ManageLogins_ValidUserUserLoginCountOverOneUserPasswordHashNotNull_ShowRemoveButtonIsTrue()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            IList<UserLoginInfo> list = new List<UserLoginInfo>
            {
                new UserLoginInfo("","",""),
                new UserLoginInfo("","","")
            };
            _userManagerMock.GetLoginsAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(list));
            IEnumerable<AuthenticationDescription> authList = new List<AuthenticationDescription>();
            _signInManagerMock.GetExternalAuthenticationSchemes().Returns(authList);
            ApplicationUser user = new ApplicationUser
            {
                PasswordHash = "123",
                Id = "123",
                Email = "123"
            };
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult(user));

            //Act
            var result = await _uut.ManageLogins() as ViewResult;

            //Assert
            Assert.True((bool)result.ViewData["ShowRemoveButton"]);
        }

        [Fact]
        public async void ManageLogins_ValidUserUserLoginCountIsOneUserPassWordHashNull_ShowRemoveButtonIsFalse()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            IList<UserLoginInfo> list = new List<UserLoginInfo>
            {
                new UserLoginInfo("","","")
            };
            _userManagerMock.GetLoginsAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(list));
            IEnumerable<AuthenticationDescription> authList = new List<AuthenticationDescription>();
            _signInManagerMock.GetExternalAuthenticationSchemes().Returns(authList);
            ApplicationUser user = new ApplicationUser
            {
                PasswordHash = null,
                Id = "123",
                Email = "123"
            };
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult(user));

            //Act
            var result = await _uut.ManageLogins() as ViewResult;

            //Assert
            Assert.False((bool)result.ViewData["ShowRemoveButton"]);
        }

        [Fact]
        public async void ManageLogins_ValidUserUserLoginCountIsOverOneUserPassWordHashNull_ShowRemoveButtonIsTrue()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            IList<UserLoginInfo> list = new List<UserLoginInfo>
            {
                new UserLoginInfo("","",""),
                new UserLoginInfo("","","")
            };
            _userManagerMock.GetLoginsAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(list));
            IEnumerable<AuthenticationDescription> authList = new List<AuthenticationDescription>();
            _signInManagerMock.GetExternalAuthenticationSchemes().Returns(authList);
            ApplicationUser user = new ApplicationUser
            {
                PasswordHash = null,
                Id = "123",
                Email = "123"
            };
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult(user));

            //Act
            var result = await _uut.ManageLogins() as ViewResult;

            //Assert
            Assert.True((bool)result.ViewData["ShowRemoveButton"]);
        }

        [Fact]
        public async void ManageLogins_ValidUserUserLoginCountIsOneUserPassWordHashNotNull_ShowRemoveButtonIsTrue()
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
            _uut.TempData = Substitute.For<ITempDataDictionary>();
            IList<UserLoginInfo> list = new List<UserLoginInfo>
            {
                new UserLoginInfo("","","")
            };
            _userManagerMock.GetLoginsAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult(list));
            IEnumerable<AuthenticationDescription> authList = new List<AuthenticationDescription>();
            _signInManagerMock.GetExternalAuthenticationSchemes().Returns(authList);
            ApplicationUser user = new ApplicationUser
            {
                PasswordHash = "123",
                Id = "123",
                Email = "123"
            };
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult(user));

            //Act
            var result = await _uut.ManageLogins() as ViewResult;

            //Assert
            Assert.True((bool)result.ViewData["ShowRemoveButton"]);
        }
    }
}
