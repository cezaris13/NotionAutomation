using System.ComponentModel;
using NotionTaskAutomation.Objects;

namespace NotionTaskAutomation.Extensions;

public static class EnumExtension
{
    public static string ToDescriptionString(this States state)
    {
        var attributes = (DescriptionAttribute[]) state
            .GetType()
            .GetField(state.ToString())
            ?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes is {Length: > 0} ? attributes[0].Description : string.Empty;
    }
}