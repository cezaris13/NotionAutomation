using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NotionTaskAutomation;
using NotionTaskAutomation.Db;

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
        using var scope = _serviceProvider.CreateScope();
        
        // Arrange
        var mockNotionButtonClicker = new Mock<INotionButtonClicker>();

        var sharedDatabases = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        mockNotionButtonClicker.Setup(n => n.GetSharedDatabases()).ReturnsAsync(sharedDatabases);

        var mockDbContext = scope.ServiceProvider.GetRequiredService<NotionDbContext>();
        var sut = new NotionController(mockNotionButtonClicker.Object, mockDbContext);
        // Act
        var result = await sut.GetSharedDatabases();

        // Assert
        Assert.IsInstanceOfType(result, typeof(ActionResult<List<Guid>>));
        Assert.AreEqual(sharedDatabases.Count, result.Value.Count);
        Assert.AreSame(sharedDatabases, result.Value);
    }
}