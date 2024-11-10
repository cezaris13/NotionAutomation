using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using NotionAutomation.Attributes;

namespace NotionAutomationTests.Attributes;

[TestClass]
public class AuthorizationAttributeTests {
    private ActionExecutingContext m_actionExecutingContext;
    private Mock<HttpRequest> m_mockHttpRequest;

    [TestInitialize]
    public void Setup() {
        var routeValueDictionary = new RouteValueDictionary {
            ["id"] = "someId",
        };

        m_mockHttpRequest = new Mock<HttpRequest>();

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext
            .Setup(p => p.Request)
            .Returns(m_mockHttpRequest.Object);

        var actionContext = new ActionContext {
            HttpContext = mockHttpContext.Object,
            RouteData = new RouteData(routeValueDictionary),
            ActionDescriptor = new ActionDescriptor(),
        };

        m_actionExecutingContext =
            new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), null);
    }

    [TestMethod]
    public void AuthorizationAttribute_AuthorizationExists_NoActionResult() {
        // Arrange
        var headerDictionary = new HeaderDictionary {
            ["Authorization"] = "*"
        };

        m_mockHttpRequest
            .Setup(p => p.Headers)
            .Returns(headerDictionary);

        var sut = new Authorization();

        // Act
        sut.OnActionExecuting(m_actionExecutingContext);

        // Assert
        Assert.IsNull(m_actionExecutingContext.Result);
    }

    [TestMethod]
    public void AuthorizationAttribute_NoAuthorization_SetsContextUnauthorizedResult() {
        // Arrange
        var headerDictionary = new HeaderDictionary();

        m_mockHttpRequest
            .Setup(p => p.Headers)
            .Returns(headerDictionary);

        var sut = new Authorization();

        // Act
        sut.OnActionExecuting(m_actionExecutingContext);

        // Assert
        Assert.IsInstanceOfType(m_actionExecutingContext.Result, typeof(UnauthorizedResult));
    }
}