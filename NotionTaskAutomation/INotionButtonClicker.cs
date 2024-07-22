using System.Threading.Tasks;

namespace NotionTaskAutomation;

public interface INotionButtonClicker
{
    Task<string> ExecuteClickAsync();
}