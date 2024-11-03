using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using NotionTaskAutomation;
using NotionTaskAutomation.Db;
using NotionTaskAutomation.Objects;

namespace NotionAutomationTests;

[TestClass]
public class NotionButtonClickerTests
{
    private ServiceProvider m_serviceProvider;
    
    private Mock<HttpMessageHandler> m_mockHttpMessageHandler;
    private Mock<IHttpClientFactory> m_mockHttpClientFactory;
    private Mock<IHttpContextAccessor> m_mockHttpContextAccessor;
    private Mock<HttpContext> m_mockHttpContext;
    private Mock<HttpRequest> m_mockHttpRequest;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddDbContext<NotionDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));

        m_serviceProvider = services.BuildServiceProvider();
        
        m_mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        
        var httpClient = new HttpClient(m_mockHttpMessageHandler.Object);
        httpClient.BaseAddress = new Uri("https://api.example.com/");

        m_mockHttpClientFactory = new Mock<IHttpClientFactory>();
        m_mockHttpClientFactory
            .Setup(p => p.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);
        
        
        m_mockHttpRequest = new Mock<HttpRequest>();
        
        m_mockHttpContext = new Mock<HttpContext>();
        m_mockHttpContext
            .Setup(p => p.Request)
            .Returns(m_mockHttpRequest.Object);
        
        m_mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        m_mockHttpContextAccessor
            .Setup(p=> p.HttpContext)
            .Returns(m_mockHttpContext.Object);
    }
    
    [TestMethod]
    public async Task GetSharedDatabases_ReturnsListOfGuids()
    {
        // Arrange
        var queryObject = new QueryObject
        {
            Results = [
                new TaskObject
                {
                    Id = Guid.NewGuid()
                },
                new TaskObject
                {
                    Id = Guid.NewGuid()
                }, 
            ]
        };
        
        m_mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(queryObject), System.Text.Encoding.UTF8, "application/json")
            });

        var headerDictionary = new HeaderDictionary { { "Authorization", "Bearer token123" } };
        
        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);
        
        var sut = new NotionButtonClicker(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);
        
        // Act
        var result = await sut.GetSharedDatabases();
        
        // Assert
        Assert.AreEqual(2, result.Count);
        CollectionAssert.AreEquivalent(queryObject.Results.Select(p => p.Id).ToList(), result);
    }
}