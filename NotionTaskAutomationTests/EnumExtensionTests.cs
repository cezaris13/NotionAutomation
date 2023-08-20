using NotionTaskAutomation.Extensions;
using NotionTaskAutomation.Objects;

namespace TestProject1;

public class EnumExtensionTests
{
    [Theory]
    [InlineData(States.Doing, "Doing")]
    [InlineData(States.TodoTomorrow, "TODO tomorrow")]
    [InlineData(States.Event, "Event")]
    [InlineData(States.Todo, "To Do")]
    [InlineData(States.EventDone, "Event done")]
    public void CorrectStateIsPassedReturnsString(States state, string expectedResult)
    {
        var result = state.ToDescriptionString();
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void IncorrectStateIsPassedReturnsNull()
    {
        var state = (States) (-1);
        var result = state.ToDescriptionString();
        Assert.Empty(result);
    }
}