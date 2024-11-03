using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using NotionTaskAutomation;
using NotionTaskAutomation.Db;
using NotionTaskAutomation.Objects;

namespace NotionAutomationTests;

[TestClass]
public class NotionApiServiceTests
{
    private Mock<IHttpClientFactory> m_mockHttpClientFactory;
    private Mock<HttpContext> m_mockHttpContext;
    private Mock<IHttpContextAccessor> m_mockHttpContextAccessor;

    private Mock<HttpMessageHandler> m_mockHttpMessageHandler;
    private Mock<HttpRequest> m_mockHttpRequest;
    private ServiceProvider m_serviceProvider;

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
            .Setup(p => p.HttpContext)
            .Returns(m_mockHttpContext.Object);
    }

    [TestMethod]
    public async Task GetSharedDatabases_ReturnsListOfGuids()
    {
        // Arrange
        var taskObjects = new List<TaskObject>();
        for (var i = 0; i < 2; i++)
            taskObjects.Add(new TaskObject
            {
                Id = Guid.NewGuid()
            });

        var queryObject = new QueryObject
        {
            Results = taskObjects
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
                Content = new StringContent(JsonSerializer.Serialize(queryObject), Encoding.UTF8, "application/json")
            });

        var headerDictionary = new HeaderDictionary { { "Authorization", "Bearer token123" } };

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);

        // Act
        var result = await sut.GetSharedDatabases();

        // Assert
        Assert.AreEqual(2, result.Count);
        CollectionAssert.AreEquivalent(queryObject.Results.Select(p => p.Id).ToList(), result);
    }

    [TestMethod]
    public async Task GetSharedDatabases_NoBearerToken_ThrowsUnauthorizedException()
    {
        var headerDictionary = new HeaderDictionary();

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);

        // Act
        var exception = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => sut.GetSharedDatabases());

        // Assert
        Assert.AreEqual("Bearer token is required", exception.Message);
    }

    [TestMethod]
    public async Task GetSharedDatabases_NotOkResult_ThrowsHttpRequestException()
    {
        // Arrange
        var statusCode = HttpStatusCode.NotFound;

        m_mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            });

        var headerDictionary = new HeaderDictionary { { "Authorization", "Bearer token123" } };

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);

        // Act
        var exception = await Assert.ThrowsExceptionAsync<BadHttpRequestException>(
            () => sut.GetSharedDatabases());

        // Assert
        Assert.AreEqual(
            $"Response status code does not indicate success: {(int)statusCode} ({ReasonPhrases.GetReasonPhrase((int)statusCode)}).",
            exception.Message);
    }

    [TestMethod]
    public async Task GetSharedDatabases_FailsToDeserialize_ThrowsException()
    {
        // Arrange
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
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });

        var headerDictionary = new HeaderDictionary { { "Authorization", "Bearer token123" } };

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);

        // Act
        var exception = await Assert.ThrowsExceptionAsync<Exception>(
            () => sut.GetSharedDatabases());

        // Assert
        Assert.AreEqual("failed to deserialize", exception.Message);
    }

    [TestMethod]
    public async Task GetStates_ReturnsListOfStates()
    {
        // Arrange
        List<string> states = ["states1", "states2"];
        var statesObject = CreateStatesObject(states);

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
                Content = new StringContent(JsonSerializer.Serialize(statesObject), Encoding.UTF8, "application/json")
            });

        var headerDictionary = new HeaderDictionary { { "Authorization", "Bearer token123" } };

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);

        // Act
        var result = await sut.GetStates(Guid.NewGuid());

        // Assert
        Assert.AreEqual(2, result.Count);
        CollectionAssert.AreEquivalent(states, result);
    }

    [TestMethod]
    public async Task GetStates_NoBearerToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var headerDictionary = new HeaderDictionary();

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);

        // Act
        var exception = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => sut.GetStates(Guid.NewGuid()));

        // Assert
        Assert.AreEqual("Bearer token is required", exception.Message);
    }

    [TestMethod]
    public async Task GetStates_NotOkResult_ThrowsHttpRequestException()
    {
        // Arrange
        var statusCode = HttpStatusCode.NotFound;

        m_mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            });

        var headerDictionary = new HeaderDictionary { { "Authorization", "Bearer token123" } };

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);

        // Act
        var exception = await Assert.ThrowsExceptionAsync<BadHttpRequestException>(
            () => sut.GetStates(Guid.NewGuid()));

        // Assert
        Assert.AreEqual(
            $"Response status code does not indicate success: {(int)statusCode} ({ReasonPhrases.GetReasonPhrase((int)statusCode)}).",
            exception.Message);
    }

    [TestMethod]
    public async Task GetStates_FailsToDeserialize_ThrowsException()
    {
        // Arrange
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
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });

        var headerDictionary = new HeaderDictionary { { "Authorization", "Bearer token123" } };

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, null, m_mockHttpContextAccessor.Object);

        // Act
        var exception = await Assert.ThrowsExceptionAsync<Exception>(
            () => sut.GetStates(Guid.NewGuid()));

        // Assert
        Assert.AreEqual("failed to deserialize", exception.Message);
    }

    [TestMethod]
    public async Task GetTasks_ReturnListOfTasks()
    {
        // Assign
        using var scope = m_serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        var ruleId = Guid.NewGuid();
        var databaseId = Guid.NewGuid();
        var notionRules = CreateNotionDatabaseRules(2, ruleId, databaseId);

        var queryObjects = CreateQueryObjects(2);
        mockDbContext.NotionDatabaseRules.AddRange(notionRules);
        await mockDbContext.SaveChangesAsync();

        m_mockHttpMessageHandler
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(queryObjects[0]), Encoding.UTF8,
                    "application/json")
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(queryObjects[1]), Encoding.UTF8,
                    "application/json")
            });

        var headerDictionary = new HeaderDictionary { { "Authorization", "Bearer token123" } };

        m_mockHttpRequest
            .Setup(request => request.Headers)
            .Returns(headerDictionary);

        var sut = new NotionApiService(m_mockHttpClientFactory.Object, mockDbContext,
            m_mockHttpContextAccessor.Object);
        // Act
        var result = await sut.GetTasks(databaseId);

        // Assert
        Assert.AreEqual(4, result.Count);

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

        CollectionAssert.AreEquivalent(tasks, result);
    }


    [TestMethod]
    public async Task UpdateTasks_UpdatesTasks()
    {
        // Assign
        using var scope = m_serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        var ruleId = Guid.NewGuid();
        var databaseId = Guid.NewGuid();
        var notionRules = CreateNotionDatabaseRules(2, ruleId, databaseId);

        var queryObjects = CreateQueryObjects(2);
        mockDbContext.NotionDatabaseRules.AddRange(notionRules);
        await mockDbContext.SaveChangesAsync();
        var statesResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                JsonSerializer.Serialize(CreateStatesObject(["state"])), Encoding.UTF8,
                "application/json")
        };
        m_mockHttpMessageHandler
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(queryObjects[0]), Encoding.UTF8,
                    "application/json")
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(queryObjects[1]), Encoding.UTF8,
                    "application/json")
            })
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
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    private StatesObject CreateStatesObject(List<string> states)
    {
        var options = states.Select(p => new Options { Name = p }).ToList();

        return new StatesObject
        {
            Properties = new Properties
            {
                Status = new StatusObject
                {
                    Select = new SelectObject
                    {
                        Options = options
                    }
                }
            }
        };
    }

    private List<NotionDatabaseRule> CreateNotionDatabaseRules(int size, Guid ruleId = new(), Guid databaseId = new())
    {
        List<NotionDatabaseRule> rules = [];

        for (var i = 0; i < size; i++)
        {
            var tempRuleId = i == 0 ? ruleId : Guid.NewGuid();
            var tempDatabaseId = i == 0 ? databaseId : Guid.NewGuid();
            rules.Add(new NotionDatabaseRule
            {
                RuleId = tempRuleId,
                DatabaseId = tempDatabaseId,
                StartingState = "InProgress",
                EndingState = "Completed",
                OnDay = "Wednesday",
                DayOffset = 5
            });
        }

        return rules;
    }

    private List<QueryObject> CreateQueryObjects(int size)
    {
        List<QueryObject> queryObjects = [];

        for (var i = 0; i < size; i++)
        {
            Guid? nextCursor = null;
            if (i < size - 1)
                nextCursor = Guid.NewGuid();

            queryObjects.Add(
                new QueryObject
                {
                    Results = Enumerable.Range(0, 2).Select(p =>
                            new TaskObject
                            {
                                Id = Guid.NewGuid(),
                                Properties = new PropertyObject
                                {
                                    Status = new Status
                                    {
                                        Select = new Select
                                        {
                                            Name = "InProgress"
                                        }
                                    }
                                }
                            })
                        .ToList(),
                    NextCursor = nextCursor
                }
            );
        }

        return queryObjects;
    }
}