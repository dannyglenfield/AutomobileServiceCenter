using AutomobileServiceCenter.Tests.TestUtilities;
using AutomobileServiceCenter.Utilities;
using AutomobileServiceCenter.Web.Configuration;
using AutomobileServiceCenter.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AutomobileServiceCenter.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<IOptions<ApplicationSettings>> optionsMock;
        private readonly Mock<HttpContext> mockHttpContext;

        public HomeControllerTests()
        {
            // Create an instance of Mock IOptions
            optionsMock = new Mock<IOptions<ApplicationSettings>>();

            // Set IOptions<> Values property to return ApplicationSettings object
            optionsMock.Setup(ap => ap.Value).Returns(new ApplicationSettings { ApplicationTitle = "ASC" });

            mockHttpContext = new Mock<HttpContext>();
            // Set FakeSession to HttpContext Session
            mockHttpContext.Setup(p => p.Session).Returns(new FakeSession());
        }

        [Fact]
        public void HomeController_Index_NoModel_Test()
        {
            var controller = new HomeController(optionsMock.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            Assert.Null((controller.Index() as ViewResult).ViewData.Model);
        }

        [Fact]
        public void HomeController_Index_Session_Test()
        {
            var controller = new HomeController(optionsMock.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            controller.Index();

            // Session value with key "Test" should not be null.
            Assert.NotNull(controller.HttpContext.Session.GetSession<ApplicationSettings>("Test"));
        }

        [Fact]
        public void HomeController_Index_Validation_Test()
        {
            var controller = new HomeController(optionsMock.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            Assert.Equal(0, (controller.Index() as ViewResult).ViewData.ModelState.ErrorCount);
        }

        [Fact]
        public void HomeController_Index_View_Test()
        {
            var controller = new HomeController(optionsMock.Object);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            Assert.NotNull((controller.Index() as ViewResult));
        }
    }
}
