using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using NotionAutomation;
using NotionAutomation.Db;

namespace NotionAutomationTests;

[TestClass]
public class NotionApiServiceTests {
    private Mock<IHttpClientFactory> m_mockHttpClientFactory;
    private Mock<HttpContext> m_mockHttpContext;
    private Mock<IHttpContextAccessor> m_mockHttpContextAccessor;

    private Mock<HttpMessageHandler> m_mockHttpMessageHandler;
    private Mock<HttpRequest> m_mockHttpRequest;
    private ServiceProvider m_serviceProvider;

    [TestInitialize]
    public void Setup() {
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
            .Setup(p => p.HttpContext)
            .Returns(m_mockHttpContext.Object);
    }

    [TestMethod]
    public async Task GetSharedDatabases_ReturnsListOfGuids() {
        // Arrange
        var queryObject = ObjectFactory.CreateQueryObjects(1)[0];

        m_mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(ObjectFactory.CreateResponse(content: queryObject));

        var headerDictionary = new HeaderDictionary { { "Authorization", "Bearer token123" } };

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);

        // Act
        var result = await sut.GetSharedDatabases();

        // Assert
        Assert.IsTrue(result.IsOk);
        Assert.AreEqual(2, result.Value.Count);
        CollectionAssert.AreEquivalent(queryObject.Results.Select(p => p.Id).ToList(), result.Value);
    }

    [TestMethod]
    public async Task GetSharedDatabases_NoBearerToken_UnauthorizedResult() {
        var headerDictionary = new HeaderDictionary();

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);

        // Act
        var result = await sut.GetSharedDatabases();

        // Assert
        Assert.IsTrue(!result.IsOk);
        Assert.IsInstanceOfType(result.Error, typeof(ObjectResult));
        Assert.AreEqual("Bearer token was not found", (result.Error as ObjectResult)!.Value);
        Assert.AreEqual(401, (result.Error as ObjectResult)!.StatusCode);
    }

    [TestMethod]
    public async Task GetSharedDatabases_NotOkResult_Returns404() {
        // Arrange
        var statusCode = HttpStatusCode.NotFound;

        m_mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(ObjectFactory.CreateResponse(statusCode, string.Empty));

        var headerDictionary = new HeaderDictionary { { "Authorization", "Bearer token123" } };

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);

        // Act
        var result = await sut.GetSharedDatabases();

        // Assert
        Assert.IsTrue(!result.IsOk);
        Assert.IsInstanceOfType(result.Error, typeof(ObjectResult));
        Assert.AreEqual((int)statusCode, (result.Error as ObjectResult)!.StatusCode);
        Assert.AreEqual($"Response status code does not indicate success: {(int)statusCode} ({ReasonPhrases.GetReasonPhrase((int)statusCode)}).", (result.Error as ObjectResult)!.Value);
    }

    [TestMethod]
    public async Task GetSharedDatabases_FailsToDeserialize_Returns500() {
        // Arrange
        m_mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(ObjectFactory.CreateResponse(content: "'{ \"Id\": 1, \"Name\": \"Coke\" }'"));

        var headerDictionary = new HeaderDictionary { { "Authorization", "Bearer token123" } };

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);

        // Act
        var result = await sut.GetSharedDatabases();

        // Assert
        Assert.IsTrue(!result.IsOk);
        Assert.IsInstanceOfType(result.Error, typeof(ObjectResult));
        Assert.AreEqual(500, (result.Error as ObjectResult)!.StatusCode);
    }

    [TestMethod]
    public async Task GetStates_ReturnsListOfStates() {
        // Arrange
        List<string> states = ["states1", "states2"];
        var statesObject = ObjectFactory.CreateStatesObject(states);

        m_mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(ObjectFactory.CreateResponse(content: statesObject));

        var headerDictionary = new HeaderDictionary { { "Authorization", "Bearer token123" } };

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);

        // Act
        var result = await sut.GetStates(Guid.NewGuid());

        // Assert
        Assert.IsTrue(result.IsOk);
        Assert.AreEqual(2, result.Value.Count);
        CollectionAssert.AreEquivalent(states, result.Value);
    }

    [TestMethod]
    public async Task GetStates_NoBearerToken_Returns401() {
        // Arrange
        var headerDictionary = new HeaderDictionary();

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);

        // Act
        var result = await sut.GetSharedDatabases();

        // Assert
        Assert.IsTrue(!result.IsOk);
        Assert.IsInstanceOfType(result.Error, typeof(ObjectResult));
        Assert.AreEqual("Bearer token was not found", (result.Error as ObjectResult)!.Value);
        Assert.AreEqual(401, (result.Error as ObjectResult)!.StatusCode);
    }

    [TestMethod]
    public async Task GetStates_NotOkResult_ReturnsNotFound() {
        // Arrange
        var statusCode = HttpStatusCode.NotFound;

        m_mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(ObjectFactory.CreateResponse(statusCode, string.Empty));

        var headerDictionary = new HeaderDictionary { { "Authorization", "Bearer token123" } };

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);

        // Act
        var result = await sut.GetStates(Guid.NewGuid());

        // Assert
        Assert.IsTrue(!result.IsOk);
        Assert.IsInstanceOfType(result.Error, typeof(ObjectResult));
        Assert.AreEqual((int)statusCode, (result.Error as ObjectResult)!.StatusCode);
        Assert.AreEqual($"Response status code does not indicate success: {(int)statusCode} ({ReasonPhrases.GetReasonPhrase((int)statusCode)}).", (result.Error as ObjectResult)!.Value);
    }

    [TestMethod]
    public async Task GetStates_FailsToDeserialize_Returns500() {
        // Arrange
        m_mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(ObjectFactory.CreateResponse(content: "'{ \"Id\": 1, \"Name\": \"Coke\" }'"));

        var headerDictionary = new HeaderDictionary { { "Authorization", "Bearer token123" } };

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);

        // Act
        var result = await sut.GetStates(Guid.NewGuid());

        // Assert
        Assert.IsTrue(!result.IsOk);
        Assert.IsInstanceOfType(result.Error, typeof(ObjectResult));
        Assert.AreEqual(500, (result.Error as ObjectResult)!.StatusCode);
    }

    [TestMethod]
    public async Task GetTasks_ReturnListOfTasks() {
        // Assign
        using var scope = m_serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        var ruleId = Guid.NewGuid();
        var databaseId = Guid.NewGuid();
        var notionRules = ObjectFactory.CreateNotionDatabaseRules(2, ruleId, databaseId);

        var queryObjects = ObjectFactory.CreateQueryObjects(2);
        mockDbContext.NotionDatabaseRules.AddRange(notionRules);
        await mockDbContext.SaveChangesAsync();

        m_mockHttpMessageHandler
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(ObjectFactory.CreateResponse(content: queryObjects[0]))
            .ReturnsAsync(ObjectFactory.CreateResponse(content: queryObjects[1]));

        var headerDictionary = new HeaderDictionary { { "Authorization", "Bearer token123" } };

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, mockDbContext,
            m_mockHttpContextAccessor.Object);
        // Act
        var result = await sut.GetTasks(databaseId);

        // Assert
        Assert.IsTrue(result.IsOk);
        Assert.AreEqual(4, result.Value.Count);

        m_mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );

        var tasks = queryObjects
            .Select(p => p.Results)
            .SelectMany(p => p)
            .ToList();

        CollectionAssert.AreEquivalent(tasks.Select(p => p.Id).ToList(), result.Value.Select(p => p.Id).ToList());
    }


    [TestMethod]
    public async Task UpdateTasks_UpdatesTasks() {
        // Assign
        using var scope = m_serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        var ruleId = Guid.NewGuid();
        var databaseId = Guid.NewGuid();
        var notionRules = ObjectFactory.CreateNotionDatabaseRules(2, ruleId, databaseId);

        var queryObjects = ObjectFactory.CreateQueryObjects(2);
        mockDbContext.NotionDatabaseRules.AddRange(notionRules);
        await mockDbContext.SaveChangesAsync();
        var statesResponseMessage = ObjectFactory.CreateResponse(content: ObjectFactory.CreateStatesObject(["state"]));
        
        m_mockHttpMessageHandler
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(ObjectFactory.CreateResponse(content: queryObjects[0]))
            .ReturnsAsync(ObjectFactory.CreateResponse(content: queryObjects[1]))
            .ReturnsAsync(statesResponseMessage)
            .ReturnsAsync(statesResponseMessage)
            .ReturnsAsync(statesResponseMessage)
            .ReturnsAsync(statesResponseMessage);

        var headerDictionary = new HeaderDictionary { { "Authorization", "Bearer token123" } };

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, mockDbContext,
            m_mockHttpContextAccessor.Object);
        // Act
        await sut.UpdateTasks(databaseId);

        // Assert
        m_mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(4),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}