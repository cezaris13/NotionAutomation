using NotionAutomation.Attributes;

namespace NotionAutomationTests.Attributes;

[TestClass]
public class AuthorizationAttributeTests {
    [TestMethod]
    public void Is_Attribute_Multiple_False()
    {
        var attributes = (IList<AttributeUsageAttribute>)typeof(Authorization).GetCustomAttributes(typeof(AttributeUsageAttribute), false);
        Assert.AreEqual(1, attributes.Count);

        var attribute = attributes[0];
        Assert.IsFalse(attribute.AllowMultiple);
    }

}