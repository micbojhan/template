using Autofac;
using Company.WebApplication1.Application.MVC.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Company.WebApplication1.Application.MVC.Tests.Controllers
{
    public class HomeControllerTests : BaseTest
    {
        private HomeController controller;

        public HomeControllerTests()
        {
            controller = Container.Resolve<HomeController>();
        }

        [Fact]
        public void HomeController_Index_Returns()
        {
            // Arrange

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
        }
    }
}
