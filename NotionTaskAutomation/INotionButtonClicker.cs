using System.Threading.Tasks;

namespace NotionTaskAutomation;

public interface INotionButtonClicker
{
    Task ExecuteClickAsync();
}