using System.ComponentModel;

namespace NotionAutomationButtonAutomation.Extensions
{
    public static class EnumExtension
    {
            public static string ToDescriptionString(this States val)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])val
                    .GetType()
                    .GetField(val.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false);
                return attributes.Length > 0 ? attributes[0].Description : string.Empty;
            }
    }
}