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
using Company.WebApplication1.Application.MVC.ViewModels.ManageViewModels;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Authentication;
using System.Linq;
using Company.WebApplication1.Application.MVC.Tests.Helpers;

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
            var loggerMockCtrl = Substitute.For<ILogger<ManageController>>();

            _uut = new ManageController(_userManagerMock, _signInManagerMock, cookieOptionsMock, emailMock, smsMock, loggerMockCtrl).WithDefaultMocks();
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
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());

            _userManagerMock.HasPasswordAsync(Arg.Any<ApplicationUser>()).Returns(true);
            _userManagerMock.GetPhoneNumberAsync(Arg.Any<ApplicationUser>()).Returns("");
            _userManagerMock.GetTwoFactorEnabledAsync(Arg.Any<ApplicationUser>()).Returns(true);
            IList<UserLoginInfo> list = new List<UserLoginInfo>();
            _userManagerMock.GetLoginsAsync(Arg.Any<ApplicationUser>()).Returns(list);
            _signInManagerMock.IsTwoFactorClientRememberedAsync(Arg.Any<ApplicationUser>()).Returns(true);

            //Act
            var result = await _uut.Index(null) as ViewResult;

            //Assert
            Assert.Null(result.ViewName); //Returns default view
        }

        [Fact]
        public async void Index_UserIsValid_ReturnsCorrectViewModel()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            var viewModel = new IndexViewModel
            {
                HasPassword = true,
                PhoneNumber = "",
                TwoFactor = true,
                Logins = new List<UserLoginInfo>(),
                BrowserRemembered = true
            };

            _userManagerMock.HasPasswordAsync(Arg.Any<ApplicationUser>()).Returns(viewModel.HasPassword);
            _userManagerMock.GetPhoneNumberAsync(Arg.Any<ApplicationUser>()).Returns(viewModel.PhoneNumber);
            _userManagerMock.GetTwoFactorEnabledAsync(Arg.Any<ApplicationUser>()).Returns(viewModel.TwoFactor);
            _userManagerMock.GetLoginsAsync(Arg.Any<ApplicationUser>()).Returns(viewModel.Logins);
            _signInManagerMock.IsTwoFactorClientRememberedAsync(Arg.Any<ApplicationUser>()).Returns(viewModel.BrowserRemembered);

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
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(default(ApplicationUser));

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
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(default(ApplicationUser));

            //Act
            var result = await _uut.RemoveLogin(null) as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.Error, result.RouteValues["message"]);
        }

        [Fact]
        public async void Removelogin_RegisteredUserWithRemoveLoginSuccess_CallsRedirectWithRemoveLoginSuccessMessage()
        {
            //Arrange
            var removeLoginViewModel = new RemoveLoginViewModel
            {
                LoginProvider = "",
                ProviderKey = ""
            };
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            _userManagerMock.RemoveLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>()).Returns(IdentityResult.Success);

            //Act
            var result = await _uut.RemoveLogin(removeLoginViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.RemoveLoginSuccess, result.RouteValues["message"]);
        }

        [Fact]
        public async void Removelogin_RegisteredUserWithRemoveLoginError_CallsRedirectWithErrorMessage()
        {
            //Arrange
            var removeLoginViewModel = new RemoveLoginViewModel
            {
                LoginProvider = "",
                ProviderKey = ""
            };
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            _userManagerMock.RemoveLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>()).Returns(IdentityResult.Failed());

            //Act
            var result = await _uut.RemoveLogin(removeLoginViewModel) as RedirectToActionResult;

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

        //AddPhoneNumber(model)
        [Fact]
        public async void AddPhoneNumber_ModelStateNotValid_ReturnsDefaultView()
        {
            //Arrange
            var addPhoneNumberViewModel = new AddPhoneNumberViewModel();
            _uut.ModelState.AddModelError("Error","Error");

            //Act
            var result = await _uut.AddPhoneNumber(addPhoneNumberViewModel) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void AddPhoneNumber_ModelStateNotValid_ReturnsViewWithSameModel()
        {
            //Arrange
            var addPhoneNumberViewModel = new AddPhoneNumberViewModel();
            _uut.ModelState.AddModelError("Error","Error");

            //Act
            var result = await _uut.AddPhoneNumber(addPhoneNumberViewModel) as ViewResult;
            var originalViewModel = JsonConvert.SerializeObject(addPhoneNumberViewModel);
            var viewModelReturnedToView = JsonConvert.SerializeObject(result.Model);

            //Assert
            Assert.Equal(originalViewModel, viewModelReturnedToView);
        }

        [Fact]
        public async void AddPhoneNumber_ModelStateValidUnregisteredUser_ReturnsErrorView()
        {
            //Arrange
            var addPhoneNumberViewModel = new AddPhoneNumberViewModel();
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(default(ApplicationUser));

            //Act
            var result = await _uut.AddPhoneNumber(addPhoneNumberViewModel) as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public async void AddPhoneNumber_ModelStateValidValidUser_ReturnsRedirectResultWithPhoneNumber()
        {
            //Arrange
            var addPhoneNumberViewModel = new AddPhoneNumberViewModel();
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());

            _userManagerMock.GenerateChangePhoneNumberTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns("");

            //Act
            var result = await _uut.AddPhoneNumber(addPhoneNumberViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal(addPhoneNumberViewModel.PhoneNumber, result.RouteValues["PhoneNumber"]);
        }

        [Fact]
        public async void AddPhoneNumber_ModelStateValidValidUser_ReturnsRedirectResultToVerifyPhoneNumber()
        {
            //Arrange
            var addPhoneNumberViewModel = new AddPhoneNumberViewModel();
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());

            _userManagerMock.GenerateChangePhoneNumberTokenAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns("");

            //Act
            var result = await _uut.AddPhoneNumber(addPhoneNumberViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal("VerifyPhoneNumber", result.ActionName);
        }

        //EnableTwoFactorAuthentication
        [Fact]
        public async void EnableTwoFactorAuthentication_UnregisteredUser_UserManagerTwoFactorEnableIsNotCalled()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(default(ApplicationUser));

            //Act
            var result = await _uut.EnableTwoFactorAuthentication();

            //Assert
            await _userManagerMock.DidNotReceive().SetTwoFactorEnabledAsync(Arg.Any<ApplicationUser>(), Arg.Any<bool>());
        }

        [Fact]
        public async void EnableTwoFactorAuthentication_ValidUser_UserManagerTwoFactorEnableIsCalled()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());

            //Act
            var result = await _uut.EnableTwoFactorAuthentication();

            //Assert
            await _userManagerMock.Received().SetTwoFactorEnabledAsync(Arg.Any<ApplicationUser>(), true);
        }

        //DisableTwoFactorAuthentication
        [Fact]
        public async void DisableTwoFactorAuthentication_UnregisteredUser_UserManagerTwoFactorEnableIsNotCalled()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(default(ApplicationUser));

            //Act
            var result = await _uut.DisableTwoFactorAuthentication();

            //Assert
            await _userManagerMock.DidNotReceive().SetTwoFactorEnabledAsync(Arg.Any<ApplicationUser>(), Arg.Any<bool>());
        }

        [Fact]
        public async void DisableTwoFactorAuthentication_ValidUser_UserManagerTwoFactorEnableIsCalled()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());

            //Act
            var result = await _uut.DisableTwoFactorAuthentication();

            //Assert
            await _userManagerMock.Received().SetTwoFactorEnabledAsync(Arg.Any<ApplicationUser>(), false);
        }

        //VerifyPhoneNumber(string)
        [Fact]
        public async void VerifyPhoneNumber_UnregisteredUser_ReturnsErrorView()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(default(ApplicationUser));

            //Act
            var result = await _uut.VerifyPhoneNumber("12345678") as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public async void VerifyPhoneNumber_ValidUserAndPhoneNumberNull_ReturnsErrorView()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());

            //Act
            var result = await _uut.VerifyPhoneNumber(null as string) as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public async void VerifyPhoneNumber_ValidUserAndPhoneNumberValid_ReturnsDefaultView()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());

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
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.VerifyPhoneNumber(default(VerifyPhoneNumberViewModel)) as ViewResult;

            //Assert
            await _userManagerMock.DidNotReceive().ChangePhoneNumberAsync(Arg.Any<ApplicationUser>(),Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async void VerifyPhoneNumber_ModelStateNotValid_DoesntSignUserIn()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.VerifyPhoneNumber(default(VerifyPhoneNumberViewModel)) as ViewResult;

            //Assert
            await _signInManagerMock.DidNotReceive().SignInAsync(Arg.Any<ApplicationUser>(),Arg.Any<bool>(), Arg.Any<string>());
        }

        [Fact]
        public async void VerifyPhoneNumber_ValidUserAndChangePhoneNumberSuccess_RedirectResultReturnedWithPhoneChangeSuccessMessage()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());

            var verifyPhoneNumberViewModel = new VerifyPhoneNumberViewModel
            {
                PhoneNumber = "12345678",
                Code = "1234"
            };
            _userManagerMock.ChangePhoneNumberAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>()).Returns(IdentityResult.Success);

            //Act
            var result = await _uut.VerifyPhoneNumber(verifyPhoneNumberViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.AddPhoneSuccess, result.RouteValues["message"]);
        }

        [Fact]
        public async void VerifyPhoneNumber_UnregisteredUser_ModelErrorAdded()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(default(ApplicationUser));

            //Act
            var result = await _uut.VerifyPhoneNumber(default(VerifyPhoneNumberViewModel)) as ViewResult;

            //Assert
            Assert.Equal("Failed to verify phone number", result.ViewData.ModelState.Root.Errors.FirstOrDefault().ErrorMessage);
        }

        [Fact]
        public async void VerifyPhoneNumber_ValidUserAndChangePhoneNumberError_ModelErrorAdded()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());

            var verifyPhoneNumberViewModel = new VerifyPhoneNumberViewModel
            {
                PhoneNumber = "12345678",
                Code = "1234"
            };
            _userManagerMock.ChangePhoneNumberAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>()).Returns(IdentityResult.Failed());

            //Act
            var result = await _uut.VerifyPhoneNumber(verifyPhoneNumberViewModel) as ViewResult;

            //Assert
            Assert.Equal("Failed to verify phone number", result.ViewData.ModelState.Root.Errors.FirstOrDefault().ErrorMessage);
        }

        [Fact]
        public async void VerifyPhoneNumber_ValidUserAndChangePhoneNumberError_SignInNotCalled()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());

            var verifyPhoneNumberViewModel = new VerifyPhoneNumberViewModel
            {
                PhoneNumber = "12345678",
                Code = "1234"
            };
            _userManagerMock.ChangePhoneNumberAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>()).Returns(IdentityResult.Failed());

            //Act
            var result = await _uut.VerifyPhoneNumber(verifyPhoneNumberViewModel) as ViewResult;

            //Assert
            await _signInManagerMock.DidNotReceive().SignInAsync(Arg.Any<ApplicationUser>(), Arg.Any<bool>());
        }

        //RemovePhoneNumber
        [Fact]
        public async void RemovePhoneNumber_UnregisteredUser_ReturnsRedirectWithErrorMessage()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(default(ApplicationUser));

            //Act
            var result = await _uut.RemovePhoneNumber() as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.Error, result.RouteValues["message"]);
        }

        [Fact]
        public async void RemovePhoneNumber_ValidUserAndSetPhoneNumberError_ReturnsRedirectWithErrorMessage()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            _userManagerMock.SetPhoneNumberAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Failed());

            //Act
            var result = await _uut.RemovePhoneNumber() as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.Error, result.RouteValues["message"]);
        }

        [Fact]
        public async void RemovePhoneNumber_ValidUserAndSetPhoneNumberSuccess_ReturnsRedirectWithSuccessMessage()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            _userManagerMock.SetPhoneNumberAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);

            //Act
            var result = await _uut.RemovePhoneNumber() as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.RemovePhoneSuccess, result.RouteValues["message"]);
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
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.ChangePassword(default(ChangePasswordViewModel)) as ViewResult;

            //Assert
            await _userManagerMock.DidNotReceive().ChangePasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async void ChangePassword_ModelStateNotValid_ReturnsDefaultView()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.ChangePassword(default(ChangePasswordViewModel)) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void ChangePassword_UnregisteredUser_ReturnsRedirectResultWithErrorMessage()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(default(ApplicationUser));

            //Act
            var result = await _uut.ChangePassword(default(ChangePasswordViewModel)) as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.Error, result.RouteValues["message"]);
        }

        [Fact]
        public async void ChangePassword_ValidUserAndChangePasswordError_ModelStateErrorsAdded()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            var changePasswordViewModel = new ChangePasswordViewModel
            {
                OldPassword = "",
                NewPassword = "",
                ConfirmPassword = ""
            };
            _userManagerMock.ChangePasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed(new IdentityError{Description = "ChangePasswordErrorForTest"}));

            //Act
            var result = await _uut.ChangePassword(changePasswordViewModel) as ViewResult;

            //Assert
            Assert.Equal("ChangePasswordErrorForTest", result.ViewData.ModelState.Root.Errors.FirstOrDefault().ErrorMessage);
        }

        [Fact]
        public async void ChangePassword_ValidUserAndChangePasswordError_DefaultViewReturned()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            var changePasswordViewModel = new ChangePasswordViewModel
            {
                OldPassword = "",
                NewPassword = "",
                ConfirmPassword = ""
            };
            _userManagerMock.ChangePasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed(new IdentityError{Description = "ChangePasswordErrorForTest"}));

            //Act
            var result = await _uut.ChangePassword(changePasswordViewModel) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void ChangePassword_ValidUserAndChangePasswordSuccess_SignInManagerSignInCalled()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            var changePasswordViewModel = new ChangePasswordViewModel
            {
                OldPassword = "",
                NewPassword = "",
                ConfirmPassword = ""
            };
            _userManagerMock.ChangePasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(IdentityResult.Success);

            //Act
            var result = await _uut.ChangePassword(changePasswordViewModel) as RedirectToActionResult;

            //Assert
            await _signInManagerMock.Received().SignInAsync(Arg.Any<ApplicationUser>(), Arg.Any<bool>());
        }

        [Fact]
        public async void ChangePassword_ValidUserAndChangePasswordSuccess_RedirectWithChangePasswordSuccessReturned()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            var changePasswordViewModel = new ChangePasswordViewModel
            {
                OldPassword = "",
                NewPassword = "",
                ConfirmPassword = ""
            };
            _userManagerMock.ChangePasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(IdentityResult.Success);

            //Act
            var result = await _uut.ChangePassword(changePasswordViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.ChangePasswordSuccess, result.RouteValues["message"]);
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
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.SetPassword(default(SetPasswordViewModel)) as ViewResult;

            //Assert
            await _userManagerMock.DidNotReceive().AddPasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
        }

        [Fact]
        public async void SetPassword_ModelStateNotValid_DefaultViewReturned()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            _uut.ModelState.AddModelError("Error", "Error");

            //Act
            var result = await _uut.SetPassword(default(SetPasswordViewModel)) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void SetPassword_UnregisteredUser_RedirectResultWithErrorReturned()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(default(ApplicationUser));

            //Act
            var result = await _uut.SetPassword(default(SetPasswordViewModel)) as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.Error, result.RouteValues["message"]);
        }

        [Fact]
        public async void SetPassword_ValidUserAndAddPasswordError_ModelStateErrorsAdded()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            var setPasswordViewModel = new SetPasswordViewModel
            {
                NewPassword = "",
                ConfirmPassword = ""
            };
            _userManagerMock.AddPasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed(new IdentityError{Description = "AddPasswordErrorForTest"}));

            //Act
            var result = await _uut.SetPassword(setPasswordViewModel) as ViewResult;

            //Assert
            Assert.Equal("AddPasswordErrorForTest", result.ViewData.ModelState.Root.Errors.FirstOrDefault().ErrorMessage);
        }

        [Fact]
        public async void SetPassword_ValidUserAndAddPasswordError_DefaultViewReturned()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            var setPasswordViewModel = new SetPasswordViewModel
            {
                NewPassword = "",
                ConfirmPassword = ""
            };
            _userManagerMock.AddPasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed(new IdentityError{Description = "AddPasswordErrorForTest"}));

            //Act
            var result = await _uut.SetPassword(setPasswordViewModel) as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void SetPassword_ValidUserAndAddPasswordSuccess_SignInManagerSignInCalled()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            var setPasswordViewModel = new SetPasswordViewModel
            {
                NewPassword = "",
                ConfirmPassword = ""
            };
            _userManagerMock.AddPasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Success);

            //Act
            var result = await _uut.SetPassword(setPasswordViewModel) as RedirectToActionResult;

            //Assert
            await _signInManagerMock.Received().SignInAsync(Arg.Any<ApplicationUser>(), Arg.Any<bool>());
        }

        [Fact]
        public async void SetPassword_ValidUserAndAddPasswordSuccess_RedirectResultWithSetPasswordSuccessReturned()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            var setPasswordViewModel = new SetPasswordViewModel
            {
                NewPassword = "",
                ConfirmPassword = ""
            };
            _userManagerMock.AddPasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Success);

            //Act
            var result = await _uut.SetPassword(setPasswordViewModel) as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.SetPasswordSuccess, result.RouteValues["message"]);
        }

        //ManageLogins
        [Theory]
        [InlineData(ManageMessageId.RemoveLoginSuccess, "The external login was removed.")]
        [InlineData(ManageMessageId.AddLoginSuccess, "The external login was added.")]
        [InlineData(ManageMessageId.Error, "An error has occurred.")]
        [InlineData(null, "")]
        public async void ManageLogins_UnregisteredUserAndGivenMessage_ViewBagContainsCorrectMessage(ManageMessageId? messageId, string message)
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(default(ApplicationUser));

            //Act
            var result = await _uut.ManageLogins(messageId);

            //Assert
            Assert.Equal(message, _uut.ViewData["StatusMessage"]);
        }

        [Fact]
        public async void ManageLogins_UnregisteredUser_ErrorViewReturned()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(default(ApplicationUser));

            //Act
            var result = await _uut.ManageLogins() as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public async void ManageLogins_ValidUser_DefaultViewReturned()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());

            IList<UserLoginInfo> list = new List<UserLoginInfo>();
            _userManagerMock.GetLoginsAsync(Arg.Any<ApplicationUser>()).Returns(list);
            IEnumerable<AuthenticationDescription> authList = new List<AuthenticationDescription>();
            _signInManagerMock.GetExternalAuthenticationSchemes().Returns(authList);

            //Act
            var result = await _uut.ManageLogins() as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public async void ManageLogins_ValidUserAndUserLoginCountOverOneAndUserPasswordHashNotNull_ShowRemoveButtonIsTrue()
        {
            //Arrange
            IList<UserLoginInfo> list = new List<UserLoginInfo>
            {
                new UserLoginInfo("","",""),
                new UserLoginInfo("","","")
            };
            _userManagerMock.GetLoginsAsync(Arg.Any<ApplicationUser>()).Returns(list);
            IEnumerable<AuthenticationDescription> authList = new List<AuthenticationDescription>();
            _signInManagerMock.GetExternalAuthenticationSchemes().Returns(authList);
            ApplicationUser user = new ApplicationUser
            {
                PasswordHash = "123"
            };
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);

            //Act
            var result = await _uut.ManageLogins() as ViewResult;

            //Assert
            Assert.True((bool)result.ViewData["ShowRemoveButton"]);
        }

        [Fact]
        public async void ManageLogins_ValidUserAndUserLoginCountIsOneAndUserPasswordHashNull_ShowRemoveButtonIsFalse()
        {
            //Arrange
            IList<UserLoginInfo> list = new List<UserLoginInfo>
            {
                new UserLoginInfo("", "", "")
            };
            _userManagerMock.GetLoginsAsync(Arg.Any<ApplicationUser>()).Returns(list);
            IEnumerable<AuthenticationDescription> authList = new List<AuthenticationDescription>();
            _signInManagerMock.GetExternalAuthenticationSchemes().Returns(authList);
            ApplicationUser user = new ApplicationUser
            {
                PasswordHash = null
            };
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);

            //Act
            var result = await _uut.ManageLogins() as ViewResult;

            //Assert
            Assert.False((bool)result.ViewData["ShowRemoveButton"]);
        }

        [Fact]
        public async void ManageLogins_ValidUserAndUserLoginCountIsOverOneAndUserPasswordHashNull_ShowRemoveButtonIsTrue()
        {
            //Arrange
            IList<UserLoginInfo> list = new List<UserLoginInfo>
            {
                new UserLoginInfo("", "", ""),
                new UserLoginInfo("", "", "")
            };
            _userManagerMock.GetLoginsAsync(Arg.Any<ApplicationUser>()).Returns(list);
            IEnumerable<AuthenticationDescription> authList = new List<AuthenticationDescription>();
            _signInManagerMock.GetExternalAuthenticationSchemes().Returns(authList);
            ApplicationUser user = new ApplicationUser
            {
                PasswordHash = null
            };
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);

            //Act
            var result = await _uut.ManageLogins() as ViewResult;

            //Assert
            Assert.True((bool)result.ViewData["ShowRemoveButton"]);
        }

        [Fact]
        public async void ManageLogins_ValidUserAndUserLoginCountIsOneAndUserPasswordHashNotNull_ShowRemoveButtonIsTrue()
        {
            //Arrange
            IList<UserLoginInfo> list = new List<UserLoginInfo>
            {
                new UserLoginInfo("", "", "")
            };
            _userManagerMock.GetLoginsAsync(Arg.Any<ApplicationUser>()).Returns(list);
            IEnumerable<AuthenticationDescription> authList = new List<AuthenticationDescription>();
            _signInManagerMock.GetExternalAuthenticationSchemes().Returns(authList);
            ApplicationUser user = new ApplicationUser
            {
                PasswordHash = "123"
            };
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);

            //Act
            var result = await _uut.ManageLogins() as ViewResult;

            //Assert
            Assert.True((bool)result.ViewData["ShowRemoveButton"]);
        }

        //LinkLogin
        [Fact]
        public async void LinkLogin_CallsSignOutOnHttpContext()
        {
            //Arrange

            //Act
            var result = await _uut.LinkLogin(Arg.Any<string>());

            //Assert
            await _uut.ControllerContext.HttpContext.Received().Authentication.SignOutAsync(Arg.Any<string>());
        }

        [Fact]
        public async void LinkLogin_CallsSignInManagerConfigureExternalAuthenticationProperties()
        {
            //Arrange

            //Act
            var result = await _uut.LinkLogin(Arg.Any<string>());

            //Assert
            _signInManagerMock.Received().ConfigureExternalAuthenticationProperties(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async void LinkLogin_ReturnsChallengeResult()
        {
            //Arrange

            //Act
            var result = await _uut.LinkLogin(Arg.Any<string>());

            //Assert
            Assert.IsType<ChallengeResult>(result);
        }

        //LinkLoginCallback
        [Fact]
        public async void LinkLoginCallback_UnregisteredUser_ReturnsErrorView()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(default(ApplicationUser));

            //Act
            var result = await _uut.LinkLoginCallback() as ViewResult;

            //Assert
            Assert.Equal("Error", result.ViewName);
        }

        [Fact]
        public async void LinkLoginCallback_UserIsValidAndExternalLoginInfoNull_ReturnsRedirectWithErrorMessage()
        {
            //Arrange
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());
            _signInManagerMock.GetExternalLoginInfoAsync(Arg.Any<string>()).Returns(default(ExternalLoginInfo));

            //Act
            var result = await _uut.LinkLoginCallback() as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.Error, result.RouteValues["message"]);
        }

        [Fact]
        public async void LinkLoginCallback_UserIsValidAndExternalLoginInfoNotNullAndAddLoginFailed_UserManagerAddLoginCalled()
        {
            //Arrange
            var validPrincipal = new ClaimsPrincipal(new[]
                {
                    new ClaimsIdentity(
                        new[] {new Claim(ClaimTypes.NameIdentifier, "MyUserId")})
                });

            _signInManagerMock.GetExternalLoginInfoAsync(Arg.Any<string>()).Returns(new ExternalLoginInfo(validPrincipal, "", "", ""));
            _userManagerMock.AddLoginAsync(Arg.Any<ApplicationUser>(),Arg.Any<UserLoginInfo>()).Returns(IdentityResult.Failed());
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());

            //Act
            var result = await _uut.LinkLoginCallback() as RedirectToActionResult;

            //Assert
            await _userManagerMock.Received().AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>());
        }

        [Fact]
        public async void LinkLoginCallback_UserIsValidAndExternalLoginInfoNotNullAndAddLoginFailed_ReturnsRedirectWithErrorMessage()
        {
            //Arrange
            var validPrincipal = new ClaimsPrincipal(new[]
                {
                    new ClaimsIdentity(
                        new[] {new Claim(ClaimTypes.NameIdentifier, "MyUserId")})
                });

            _signInManagerMock.GetExternalLoginInfoAsync(Arg.Any<string>()).Returns(new ExternalLoginInfo(validPrincipal, "", "", ""));
            _userManagerMock.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>()).Returns(IdentityResult.Failed());
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());

            //Act
            var result = await _uut.LinkLoginCallback() as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.Error, result.RouteValues["message"]);
        }

        [Fact]
        public async void LinkLoginCallback_UserIsValidAndExternalLoginInfoNotNullAndAddLoginSucceded_CallsSignOutOnHttpContext()
        {
            //Arrange
            var validPrincipal = new ClaimsPrincipal(new[]
                {
                    new ClaimsIdentity(
                        new[] {new Claim(ClaimTypes.NameIdentifier, "MyUserId")})
                });

            _signInManagerMock.GetExternalLoginInfoAsync(Arg.Any<string>()).Returns(new ExternalLoginInfo(validPrincipal, "", "", ""));
            _userManagerMock.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>()).Returns(IdentityResult.Success);
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());

            //Act
            var result = await _uut.LinkLoginCallback() as RedirectToActionResult;

            //Assert
            await _uut.ControllerContext.HttpContext.Received().Authentication.SignOutAsync(Arg.Any<string>());
        }

        [Fact]
        public async void LinkLoginCallback_UserIsValidAndExternalLoginInfoNotNullAndAddLoginSucceded_ReturnsRedirectWithSuccessMessage()
        {
            //Arrange
            var validPrincipal = new ClaimsPrincipal(new[]
                {
                    new ClaimsIdentity(
                        new[] {new Claim(ClaimTypes.NameIdentifier, "MyUserId")})
                });

            _signInManagerMock.GetExternalLoginInfoAsync(Arg.Any<string>()).Returns(new ExternalLoginInfo(validPrincipal, "", "", ""));
            _userManagerMock.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>()).Returns(IdentityResult.Success);
            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(new ApplicationUser());

            //Act
            var result = await _uut.LinkLoginCallback() as RedirectToActionResult;

            //Assert
            Assert.Equal(ManageMessageId.AddLoginSuccess, result.RouteValues["message"]);
        }
    }
}
