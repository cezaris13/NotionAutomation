using NotionAutomation.DataTypes;

namespace NotionAutomationTests.DataTypes;

[TestClass]
public class UnitTypeTests {
    [TestMethod]
    public void ToString_ReturnsEmptyBraces() {
        // Arrange
        var sut = Unit.Value;

        // Act
        var result = sut.ToString();

        // Assert
        Assert.AreEqual("()", result);
    }
}