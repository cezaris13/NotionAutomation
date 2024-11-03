using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NotionTaskAutomation;
using NotionTaskAutomation.Db;
using NotionTaskAutomation.Objects;

namespace NotionAutomationTests;

[TestClass]
public class NotionControllerTests
{
    private ServiceProvider _serviceProvider;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddDbContext<NotionDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));

        _serviceProvider = services.BuildServiceProvider();
    }
    
    [TestMethod]
    public async Task GetSharedDatabases_ReturnsListOfGuids()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        
        var mockNotionButtonClicker = new Mock<INotionButtonClicker>();
        mockNotionButtonClicker
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        var sut = new NotionController(mockNotionButtonClicker.Object, mockDbContext);
        // Act
        var result = await sut.GetSharedDatabases();

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var response = (result.Result as OkObjectResult)!.Value as IEnumerable<Guid>;
        Assert.AreEqual(sharedDatabases.Count, response.Count());
        Assert.AreSame(sharedDatabases, response);
    }
    
    [TestMethod]
    public async Task GetNotionDatabaseRules_ReturnsListOfRules()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        
        var databaseId = Guid.NewGuid();
        var notionRules = new List<NotionDatabaseRule>
        {
            new()
            {
                RuleId = Guid.NewGuid(),
                DatabaseId = databaseId,
                StartingState = "Pending",
                EndingState = "Approved",
                OnDay = "Monday",
                DayOffset = 2
            },
            new()
            {
                RuleId = Guid.NewGuid(),
                DatabaseId = databaseId,
                StartingState = "InProgress",
                EndingState = "Completed",
                OnDay = "Wednesday",
                DayOffset = 5
            },
            new()
            {
                RuleId = Guid.NewGuid(),
                DatabaseId = databaseId,
                StartingState = "New",
                EndingState = "Archived",
                OnDay = "Friday",
                DayOffset = -3
            }
        };

        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };
        
        var mockNotionButtonClicker = new Mock<INotionButtonClicker>();
        mockNotionButtonClicker
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);
        
        mockNotionButtonClicker
            .Setup(p => p.GetNotionDatabaseRules(databaseId))
            .Returns(
                notionRules
                    .Where(q => q.DatabaseId == databaseId)
                    .ToList());

        var sut = new NotionController(mockNotionButtonClicker.Object, mockDbContext);
        
        // Act
        var result = await sut.GetNotionDatabaseRules(databaseId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var response = (result.Result as OkObjectResult)!.Value as List<NotionDatabaseRule>;
        Assert.AreEqual(notionRules.Count, response.Count());
        CollectionAssert.AreEquivalent(notionRules, response);
    }
    
    [TestMethod]
    public async Task GetNotionDatabaseRules_NoRulesFound_ReturnsEmptyList()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        var notionRules = new List<NotionDatabaseRule> { 
            new()
            {
                RuleId = Guid.NewGuid(),
                DatabaseId = Guid.NewGuid(),
                StartingState = "Pending",
                EndingState = "Approved",
                OnDay = "Monday",
                DayOffset = 2
            }
        };
        await mockDbContext.SaveChangesAsync();
        
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };
        
        var mockNotionButtonClicker = new Mock<INotionButtonClicker>();
        mockNotionButtonClicker
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);
        
        mockNotionButtonClicker
            .Setup(p => p.GetNotionDatabaseRules(databaseId))
            .Returns(
                notionRules
                    .Where(q => q.DatabaseId == databaseId)
                    .ToList());

        var sut = new NotionController(mockNotionButtonClicker.Object, mockDbContext);
        // Act
        var result = await sut.GetNotionDatabaseRules(databaseId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var response = (result.Result as OkObjectResult)!.Value as IEnumerable<NotionDatabaseRule>;
        Assert.AreEqual(0, response.Count());
    }
    
    [TestMethod]
    public async Task GetNotionDatabaseRules_DatabaseDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();

        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        
        var mockNotionButtonClicker = new Mock<INotionButtonClicker>();
        mockNotionButtonClicker.Setup(n => n.GetSharedDatabases()).ReturnsAsync(sharedDatabases);

        var sut = new NotionController(mockNotionButtonClicker.Object, mockDbContext);
        
        // Act
        var result = await sut.GetNotionDatabaseRules(Guid.NewGuid());

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }
    
    [TestMethod]
    public async Task GetNotionDatabaseRule_ReturnsRule()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        
        var ruleId = Guid.NewGuid();
        var databaseId = Guid.NewGuid();
        var notionRules = new List<NotionDatabaseRule>
        {
            new()
            {
                RuleId = ruleId,
                DatabaseId =  databaseId,
                StartingState = "Pending",
                EndingState = "Approved",
                OnDay = "Monday",
                DayOffset = 2
            },
            new()
            {
                RuleId = Guid.NewGuid(),
                DatabaseId =  Guid.NewGuid(),
                StartingState = "InProgress",
                EndingState = "Completed",
                OnDay = "Wednesday",
                DayOffset = 5
            }
        };

        mockDbContext.NotionDatabaseRules.AddRange(notionRules);
        await mockDbContext.SaveChangesAsync();
        
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() ,databaseId };
        
        var mockNotionButtonClicker = new Mock<INotionButtonClicker>();
        mockNotionButtonClicker
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);
        
        var sut = new NotionController(mockNotionButtonClicker.Object, mockDbContext);
        
        // Act
        var result = await sut.GetNotionDatabaseRule(ruleId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var response = (result.Result as OkObjectResult)!.Value as NotionDatabaseRule;
        Assert.AreEqual(notionRules[0], response);
    }
    
    [TestMethod]
    public async Task GetNotionDatabaseRule_NotionRuleIsNotForUserDatabase_ReturnsNotFound()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        
        var ruleId = Guid.NewGuid();
        var notionRules = new List<NotionDatabaseRule>
        {
            new()
            {
                RuleId = ruleId,
                DatabaseId =  Guid.NewGuid(),
                StartingState = "Pending",
                EndingState = "Approved",
                OnDay = "Monday",
                DayOffset = 2
            },
        };

        mockDbContext.NotionDatabaseRules.AddRange(notionRules);
        await mockDbContext.SaveChangesAsync();
        
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid()};
        
        var mockNotionButtonClicker = new Mock<INotionButtonClicker>();
        mockNotionButtonClicker
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);
        
        var sut = new NotionController(mockNotionButtonClicker.Object, mockDbContext);
        
        // Act
        var result = await sut.GetNotionDatabaseRule(ruleId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }
    
    [TestMethod]
    public async Task GetNotionDatabaseRule_NoRuleFound_ReturnsNotFound()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        
        var ruleId = Guid.NewGuid();
        var notionRules = new List<NotionDatabaseRule>
        {
            new()
            {
                RuleId = Guid.NewGuid(),
                DatabaseId =  Guid.NewGuid(),
                StartingState = "InProgress",
                EndingState = "Completed",
                OnDay = "Wednesday",
                DayOffset = 5
            }
        };

        mockDbContext.NotionDatabaseRules.AddRange(notionRules);
        await mockDbContext.SaveChangesAsync();
        
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        
        var mockNotionButtonClicker = new Mock<INotionButtonClicker>();
        mockNotionButtonClicker
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);
        
        var sut = new NotionController(mockNotionButtonClicker.Object, mockDbContext);
        
        // Act
        var result = await sut.GetNotionDatabaseRule(ruleId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    } 
}