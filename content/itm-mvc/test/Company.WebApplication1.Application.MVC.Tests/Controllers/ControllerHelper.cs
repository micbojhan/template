using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;

namespace test3.Application.MVC.Tests.Controllers.TestExtensions
{
    public static class ControllerHelper
    {
        public static T WithDefaultMocks<T>(this T ctrl) where T : Controller
        {
            //Setup default mocks for a controller
            var httpContext = Substitute.For<HttpContext>();
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            ctrl.TempData = Substitute.For<ITempDataDictionary>();
            ctrl.Url = Substitute.For<IUrlHelper>();
            return ctrl;
        }
    }
}
