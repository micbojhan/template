using Microsoft.AspNetCore.Mvc;
using Company.WebApplication1.Application.MVC.Controllers;
using Company.WebApplication1.Application.MVC.Tests.Helpers;
using Xunit;

namespace Company.WebApplication1.Application.MVC.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly HomeController _uut;

        public HomeControllerTests()
        {
            _uut = new HomeController().WithDefaultMocks();
        }

        //index
        [Fact]
        public void Index_ReturnsDefaultView()
        {
            //Arrange

            //Act
            var result = _uut.Index() as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        //About
        [Fact]
        public void About_ReturnsDefaultView()
        {
            //Arrange

            //Act
            var result = _uut.About() as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public void About_ViewDataContainsCorrectMessage()
        {
            //Arrange

            //Act
            var result = _uut.About() as ViewResult;

            //Assert
            Assert.Equal("Your application description page.", result.ViewData["Message"]);
        }

        //Contact
        [Fact]
        public void Contact_ReturnsDefaultView()
        {
            //Arrange

            //Act
            var result = _uut.Contact() as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }

        [Fact]
        public void Contact_ViewDataContainsCorrectMessage()
        {
            //Arrange

            //Act
            var result = _uut.Contact() as ViewResult;

            //Assert
            Assert.Equal("Your contact page.", result.ViewData["Message"]);
        }

        //Error
        [Fact]
        public void Error_ReturnsDefaultView()
        {
            //Arrange

            //Act
            var result = _uut.Error() as ViewResult;

            //Assert
            Assert.Null(result.ViewName);
        }
    }
}
