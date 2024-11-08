using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NotionAutomation;
using NotionAutomation.DataTypes;
using NotionAutomation.Db;
using NotionAutomation.Objects;

namespace NotionAutomationTests;

[TestClass]
public class NotionControllerTests {
    private ServiceProvider m_serviceProvider;

    [TestInitialize]
    public void Setup() {
        var services = new ServiceCollection();

        services.AddDbContext<NotionDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));

        m_serviceProvider = services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task GetSharedDatabases_ReturnsListOfGuids() {
        // Arrange
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.GetSharedDatabases();

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var response = (result.Result as OkObjectResult)!.Value as List<Guid>;
        Assert.IsNotNull(response);
        Assert.AreEqual(sharedDatabases.Count, response.Count);
        Assert.AreSame(sharedDatabases, response);
    }

    [TestMethod]
    public async Task GetSharedDatabases_ApiServiceFails_ReturnsNotFound() {
        // Arrange
        var unauthorizedObject = new ObjectResult("unauthorized!") {
            StatusCode = 401
        };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(Result<List<Guid>, ActionResult>.Err(unauthorizedObject));

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.GetSharedDatabases();

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
        Assert.AreEqual(unauthorizedObject.Value, (result.Result as ObjectResult)!.Value);
        Assert.AreEqual(unauthorizedObject.StatusCode, (result.Result as ObjectResult)!.StatusCode);
    }

    [TestMethod]
    public async Task GetNotionDatabaseRules_ReturnsListOfRules() {
        // Arrange
        var databaseId = Guid.NewGuid();
        var notionRules = ObjectFactory.CreateNotionDatabaseRules(3, databaseId: databaseId);
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        mockNotionApiService
            .Setup(p => p.GetNotionDatabaseRules(databaseId))
            .Returns(
                notionRules
                    .Where(q => q.DatabaseId == databaseId)
                    .ToList());

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.GetNotionDatabaseRules(databaseId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var response = (result.Result as OkObjectResult)!.Value as List<NotionDatabaseRule>;
        Assert.IsNotNull(response);
        Assert.AreEqual(1, response.Count);
        CollectionAssert.AreEquivalent(notionRules.Where(p => p.DatabaseId == databaseId).ToList(), response);
    }

    [TestMethod]
    public async Task GetNotionDatabaseRules_NoRulesFound_ReturnsEmptyList() {
        // Arrange
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };
        var notionRules = ObjectFactory.CreateNotionDatabaseRules(1);

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        mockNotionApiService
            .Setup(p => p.GetNotionDatabaseRules(databaseId))
            .Returns(
                notionRules
                    .Where(q => q.DatabaseId == databaseId)
                    .ToList());

        var sut = new NotionController(mockNotionApiService.Object, null);
        // Act
        var result = await sut.GetNotionDatabaseRules(databaseId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var response = (result.Result as OkObjectResult)!.Value as List<NotionDatabaseRule>;
        Assert.IsNotNull(response);
        Assert.AreEqual(0, response.Count);
    }

    [TestMethod]
    public async Task GetNotionDatabaseRules_GetSharedDatabasesFails_ReturnsSharedDatabasesError() {
        // Arrange
        var unauthorizedObject = new ObjectResult("unauthorized!") {
            StatusCode = 401
        };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(Result<List<Guid>, ActionResult>.Err(unauthorizedObject));

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.GetNotionDatabaseRules(Guid.NewGuid());

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
        Assert.AreEqual(unauthorizedObject.Value, (result.Result as ObjectResult)!.Value);
        Assert.AreEqual(unauthorizedObject.StatusCode, (result.Result as ObjectResult)!.StatusCode);
    }

    [TestMethod]
    public async Task GetNotionDatabaseRules_DatabaseDoesNotExist_ReturnsNotFound() {
        // Arrange
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.GetNotionDatabaseRules(Guid.NewGuid());

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));

        Assert.AreEqual("Notion database not found", (result.Result as NotFoundObjectResult)!.Value);
    }
    
    [TestMethod]
    public async Task GetNotionDatabaseRule_ReturnsRule() {
        // Arrange
        var ruleId = Guid.NewGuid();
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };
        var notionRules = ObjectFactory.CreateNotionDatabaseRules(2, ruleId, databaseId);
        
        using var scope = m_serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        mockDbContext.NotionDatabaseRules.AddRange(notionRules);
        await mockDbContext.SaveChangesAsync();

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        var sut = new NotionController(mockNotionApiService.Object, mockDbContext);

        // Act
        var result = await sut.GetNotionDatabaseRule(ruleId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var response = (result.Result as OkObjectResult)!.Value as NotionDatabaseRule;
        Assert.AreEqual(notionRules[0], response);
    }
    
    [TestMethod]
    public async Task GetNotionDatabaseRule_GetSharedDatabasesFails_ReturnsSharedDatabasesError() {
        // Arrange
        var unauthorizedObject = new ObjectResult("unauthorized!") {
            StatusCode = 401
        };
        
        var ruleId = Guid.NewGuid();
        
        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(Result<List<Guid>, ActionResult>.Err(unauthorizedObject));

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.GetNotionDatabaseRule(ruleId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
        Assert.AreEqual(unauthorizedObject.Value, (result.Result as ObjectResult)!.Value);
        Assert.AreEqual(unauthorizedObject.StatusCode, (result.Result as ObjectResult)!.StatusCode);
    }
    
    [TestMethod]
    public async Task GetNotionDatabaseRule_NotionRuleIsNotForUserDatabase_ReturnsNotFound() {
        // Arrange
      
        var ruleId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var notionRules = ObjectFactory.CreateNotionDatabaseRules(1, ruleId);
        
        using var scope = m_serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        mockDbContext.NotionDatabaseRules.AddRange(notionRules);
        await mockDbContext.SaveChangesAsync();

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        var sut = new NotionController(mockNotionApiService.Object, mockDbContext);

        // Act
        var result = await sut.GetNotionDatabaseRule(ruleId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        Assert.AreEqual("Notion database not found", (result.Result as NotFoundObjectResult)!.Value);
    }

    [TestMethod]
    public async Task GetNotionDatabaseRule_NoRuleFound_ReturnsNotFound() {
        // Arrange
        var ruleId = Guid.NewGuid();
        var notionRules = ObjectFactory.CreateNotionDatabaseRules(1);
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        
        using var scope = m_serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        mockDbContext.NotionDatabaseRules.AddRange(notionRules);
        await mockDbContext.SaveChangesAsync();

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        var sut = new NotionController(mockNotionApiService.Object, mockDbContext);

        // Act
        var result = await sut.GetNotionDatabaseRule(ruleId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        Assert.AreEqual("Notion page rule not found", (result.Result as NotFoundObjectResult)!.Value);
    }

    [TestMethod]
    public async Task ModifyNotionDatabaseRule_RuleIsModified() {
        // Arrange
        var ruleId = Guid.NewGuid();
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };
        var notionRules = ObjectFactory.CreateNotionDatabaseRules(1, ruleId, databaseId);
        var modifiedNotionDatabaseRule = ObjectFactory.CreateNotionDatabaseRules(1, ruleId)[0];
        
        using var scope = m_serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        mockDbContext.NotionDatabaseRules.AddRange(notionRules);
        await mockDbContext.SaveChangesAsync();

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        mockNotionApiService
            .Setup(p => p.GetStates(It.IsAny<Guid>()))
            .ReturnsAsync(Result<List<string>, ActionResult>.Ok(["InProgress", "Completed"]));

        var sut = new NotionController(mockNotionApiService.Object, mockDbContext);

        // Act
        var result = await sut.ModifyNotionDatabaseRule(databaseId, modifiedNotionDatabaseRule);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkResult));
        var modifiedRule = mockDbContext.NotionDatabaseRules.FirstOrDefault(p => p.RuleId == ruleId);
        Assert.IsNotNull(modifiedRule);
        Assert.AreEqual(modifiedNotionDatabaseRule.OnDay, modifiedRule.OnDay);
        Assert.AreEqual(modifiedNotionDatabaseRule.StartingState, modifiedRule.StartingState);
        Assert.AreEqual(modifiedNotionDatabaseRule.EndingState, modifiedRule.EndingState);
    }

    [TestMethod]
    public async Task ModifyNotionDatabaseRule_GetSharedDatabasesFails_ReturnsSharedDatabasesError() {
        // Arrange
        var unauthorizedObject = new ObjectResult("unauthorized!") {
            StatusCode = 401
        };
        
        var modifiedNotionDatabaseRule = ObjectFactory.CreateNotionDatabaseRules(1)[0];
        
        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(Result<List<Guid>, ActionResult>.Err(unauthorizedObject));

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.ModifyNotionDatabaseRule(Guid.NewGuid(), modifiedNotionDatabaseRule);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        Assert.AreEqual(unauthorizedObject.Value, (result as ObjectResult)!.Value);
        Assert.AreEqual(unauthorizedObject.StatusCode, (result as ObjectResult)!.StatusCode);
    }
    
    [TestMethod]
    public async Task ModifyNotionDatabaseRule_GetStatesFails_ReturnsStatesError() {
        // Arrange
        var unauthorizedObject = new ObjectResult("unauthorized!") {
            StatusCode = 401
        };

        var ruleId = Guid.NewGuid();
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };
        var modifiedNotionDatabaseRule = ObjectFactory.CreateNotionDatabaseRules(1, ruleId)[0];
        
        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        mockNotionApiService
            .Setup(p => p.GetStates(It.IsAny<Guid>()))
            .ReturnsAsync(Result<List<string>, ActionResult>.Err(unauthorizedObject));

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.ModifyNotionDatabaseRule(databaseId, modifiedNotionDatabaseRule);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        Assert.AreEqual(unauthorizedObject.Value, (result as ObjectResult)!.Value);
        Assert.AreEqual(unauthorizedObject.StatusCode, (result as ObjectResult)!.StatusCode);
    }
    
    [TestMethod]
    public async Task ModifyNotionDatabaseRule_DatabaseIdIsNotIncluded_NotFound() {
        // Arrange
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.ModifyNotionDatabaseRule(databaseId, null);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        Assert.AreEqual("Notion database not found", (result as NotFoundObjectResult)!.Value);
    }

    [TestMethod]
    public async Task ModifyNotionDatabaseRule_StatesAreNotInStateList_BadRequest() {
        // Arrange
        var ruleId = Guid.NewGuid();
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };
        var modifiedNotionDatabaseRule = ObjectFactory.CreateNotionDatabaseRules(1, ruleId)[0];

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        mockNotionApiService
            .Setup(p => p.GetStates(It.IsAny<Guid>()))
            .ReturnsAsync(Result<List<string>, ActionResult>.Ok(["InProgressss", "Completedddd"]));

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.ModifyNotionDatabaseRule(databaseId, modifiedNotionDatabaseRule);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        Assert.AreEqual("Notion page start or end state is invalid", (result as BadRequestObjectResult)!.Value);
    }

    [TestMethod]
    public async Task ModifyNotionDatabaseRule_RuleIsNotFound_NotFoundResult() {
        // Arrange
        var ruleId = Guid.NewGuid();
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };
        var notionRules = ObjectFactory.CreateNotionDatabaseRules(1, databaseId: databaseId);
        var modifiedNotionDatabaseRule = ObjectFactory.CreateNotionDatabaseRules(1, ruleId)[0];

        using var scope = m_serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        mockDbContext.NotionDatabaseRules.AddRange(notionRules);
        await mockDbContext.SaveChangesAsync();

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        mockNotionApiService
            .Setup(p => p.GetStates(It.IsAny<Guid>()))
            .ReturnsAsync(Result<List<string>, ActionResult>.Ok(["InProgress", "Completed"]));

        var sut = new NotionController(mockNotionApiService.Object, mockDbContext);

        // Act
        var result = await sut.ModifyNotionDatabaseRule(databaseId, modifiedNotionDatabaseRule);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        Assert.AreEqual("Notion page rule not found", (result as NotFoundObjectResult)!.Value);
    }

    [TestMethod]
    public async Task AddNotionDatabaseRule_RuleIsAdded() {
        // Arrange
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };
        var notionRules = ObjectFactory.CreateNotionDatabaseRules(0);
        var notionDatabaseRule = ObjectFactory.CreateNotionDatabaseRuleObject();

        using var scope = m_serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        mockDbContext.NotionDatabaseRules.AddRange(notionRules);
        await mockDbContext.SaveChangesAsync();

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        mockNotionApiService
            .Setup(p => p.GetStates(It.IsAny<Guid>()))
            .ReturnsAsync(Result<List<string>, ActionResult>.Ok(["InProgress", "Completed"]));

        var sut = new NotionController(mockNotionApiService.Object, mockDbContext);

        var notionRuleSize = mockDbContext.NotionDatabaseRules.Count();
        // Act
        var result = await sut.AddNotionDatabaseRule(databaseId, notionDatabaseRule);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.AreEqual(notionRuleSize + 1, mockDbContext.NotionDatabaseRules.Count());
        var addedNotionDatabaseRule = mockDbContext.NotionDatabaseRules.First(p => p.DatabaseId == databaseId);
        Assert.AreEqual(notionDatabaseRule.OnDay, addedNotionDatabaseRule.OnDay);
        Assert.AreEqual(notionDatabaseRule.StartingState, addedNotionDatabaseRule.StartingState);
        Assert.AreEqual(notionDatabaseRule.EndingState, addedNotionDatabaseRule.EndingState);
        Assert.AreEqual(databaseId, addedNotionDatabaseRule.DatabaseId);
    }

    [TestMethod]
    public async Task AddNotionDatabaseRule_DatabaseIdIsNotIncluded_NotFound() {
        // Arrange
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.AddNotionDatabaseRule(databaseId, null);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        Assert.AreEqual("Notion database not found", (result as NotFoundObjectResult)!.Value);
    }

    [TestMethod]
    public async Task AddNotionDatabaseRule_StatesAreNotInStateList_BadRequest() {
        // Arrange
        var databaseId = Guid.NewGuid();

        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        mockNotionApiService
            .Setup(p => p.GetStates(It.IsAny<Guid>()))
            .ReturnsAsync(Result<List<string>, ActionResult>.Ok(["InProgress", "Completed"]));

        var sut = new NotionController(mockNotionApiService.Object, null);

        var notionDatabaseRule = new NotionDatabaseRuleObject {
            StartingState = "RandomState",
            EndingState = "RandomState"
        };

        // Act
        var result = await sut.AddNotionDatabaseRule(databaseId, notionDatabaseRule);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        Assert.AreEqual("Notion page start or end state is invalid", (result as BadRequestObjectResult)!.Value);
    }

    [TestMethod]
    public async Task RemoveNotionDatabaseRule_RuleIsRemoved() {
        // Arrange
        using var scope = m_serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();

        var ruleId = Guid.NewGuid();
        var databaseId = Guid.NewGuid();
        var notionRules = ObjectFactory.CreateNotionDatabaseRules(1, ruleId, databaseId);
        mockDbContext.NotionDatabaseRules.AddRange(notionRules);
        await mockDbContext.SaveChangesAsync();

        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        var sut = new NotionController(mockNotionApiService.Object, mockDbContext);


        var notionRuleSize = mockDbContext.NotionDatabaseRules.Count();
        // Act
        var result = await sut.DeleteNotionDatabaseRule(ruleId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.AreEqual(notionRuleSize - 1, mockDbContext.NotionDatabaseRules.Count());
        var removedNotionDatabaseRule = mockDbContext.NotionDatabaseRules.FirstOrDefault(p => p.RuleId == ruleId);
        Assert.IsNull(removedNotionDatabaseRule);
    }

    [TestMethod]
    public async Task RemoveNotionDatabaseRule_NoRuleFound_NotFoundResult() {
        // Arrange
        var ruleId = Guid.NewGuid();
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };
        var notionRules = new List<NotionDatabaseRule>();
        
        using var scope = m_serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        mockDbContext.NotionDatabaseRules.AddRange(notionRules);
        await mockDbContext.SaveChangesAsync();

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        var sut = new NotionController(mockNotionApiService.Object, mockDbContext);

        // Act
        var result = await sut.DeleteNotionDatabaseRule(ruleId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        Assert.AreEqual("Notion rule for database not found", (result as NotFoundObjectResult)!.Value);
    }

    [TestMethod]
    public async Task RemoveNotionDatabaseRule_NotInSharedDatabaseList_NotFoundResult() {
        // Arrange
        var ruleId = Guid.NewGuid();
        var databaseId = Guid.NewGuid();
        var notionRules = ObjectFactory.CreateNotionDatabaseRules(1, ruleId, databaseId);
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        
        using var scope = m_serviceProvider.CreateScope();
        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        mockDbContext.NotionDatabaseRules.AddRange(notionRules);
        await mockDbContext.SaveChangesAsync();

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        var sut = new NotionController(mockNotionApiService.Object, mockDbContext);

        // Act
        var result = await sut.DeleteNotionDatabaseRule(ruleId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        Assert.AreEqual("Notion database not found", (result as NotFoundObjectResult)!.Value);
    }

    [TestMethod]
    public async Task GetStates_DoesNotContainDatabaseId_NotFoundResult() {
        // Arrange
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.GetStates(databaseId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        Assert.AreEqual("Notion database not found", (result.Result as NotFoundObjectResult)!.Value);
    }

    [TestMethod]
    public async Task GetStates_EmptyListOfStates_ReturnsEmptyList() {
        // Arrange
        List<string> states = [];

        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        mockNotionApiService
            .Setup(p => p.GetStates(It.IsAny<Guid>()))
            .ReturnsAsync(states);

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.GetStates(databaseId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var response = (result.Result as OkObjectResult)!.Value as List<string>;
        Assert.IsNotNull(response);
        Assert.AreEqual(0, response.Count);
    }

    [TestMethod]
    public async Task GetStates_ReturnsListOfStates() {
        // Arrange
        List<string> states = ["State1", "State2", "State3"];
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        mockNotionApiService
            .Setup(p => p.GetStates(It.IsAny<Guid>()))
            .ReturnsAsync(states);

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.GetStates(databaseId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var response = (result.Result as OkObjectResult)!.Value as List<string>;
        Assert.IsNotNull(response);
        Assert.AreEqual(states.Count, response.Count);
        CollectionAssert.AreEquivalent(states, response);
    }

    [TestMethod]
    public async Task GetTasks_DoesNotContainDatabaseId_NotFoundResult() {
        // Arrange
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.GetTasks(databaseId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        Assert.AreEqual("Notion database not found", (result.Result as NotFoundObjectResult)!.Value);
    }

    [TestMethod]
    public async Task GetTasks_EmptyListOfStates_ReturnsEmptyList() {
        // Arrange
        List<TaskObject> states = [];
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        mockNotionApiService
            .Setup(p => p.GetTasks(It.IsAny<Guid>()))
            .ReturnsAsync(states);

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.GetTasks(databaseId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var response = (result.Result as OkObjectResult)!.Value as List<TaskObject>;
        Assert.IsNotNull(response);
        Assert.AreEqual(0, response.Count);
    }

    [TestMethod]
    public async Task GetTasks_ReturnsListOfStates() {
        // Arrange
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };
        var states = ObjectFactory.CreateTaskObject(1);

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        mockNotionApiService
            .Setup(p => p.GetTasks(It.IsAny<Guid>()))
            .ReturnsAsync(states);

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.GetTasks(databaseId);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var response = (result.Result as OkObjectResult)!.Value as List<TaskObject>;
        Assert.IsNotNull(response);
        Assert.AreEqual(states.Count, response.Count);
        CollectionAssert.AreEquivalent(states, response);
    }

    [TestMethod]
    public async Task UpdateTasksForDatabase_DoesNotContainDatabaseId_NotFoundResult() {
        // Arrange
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.UpdateTasksForDatabase(databaseId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        Assert.AreEqual("Notion database not found", (result as NotFoundObjectResult)!.Value);
    }

    [TestMethod]
    public async Task UpdateTasksForDatabase_ReturnsOk() {
        // Arrange
        var databaseId = Guid.NewGuid();
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), databaseId };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        mockNotionApiService
            .Setup(p => p.UpdateTasks(It.IsAny<Guid>()))
            .ReturnsAsync(Result<Unit, ActionResult>.Ok(Unit.Value));

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.UpdateTasksForDatabase(databaseId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public async Task UpdateTasksForDatabases_ReturnsOk() {
        // Arrange
        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var mockNotionApiService = new Mock<INotionApiService>();
        mockNotionApiService
            .Setup(n => n.GetSharedDatabases())
            .ReturnsAsync(sharedDatabases);

        mockNotionApiService
            .Setup(p => p.UpdateTasks(It.IsAny<Guid>()))
            .ReturnsAsync(Result<Unit, ActionResult>.Ok(Unit.Value));

        var sut = new NotionController(mockNotionApiService.Object, null);

        // Act
        var result = await sut.UpdateTasksForDatabases();

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkResult));

        mockNotionApiService.Verify(n => n.UpdateTasks(It.IsAny<Guid>()), Times.Exactly(3));
        mockNotionApiService.Verify(n => n.GetSharedDatabases(), Times.Once);
    }
}