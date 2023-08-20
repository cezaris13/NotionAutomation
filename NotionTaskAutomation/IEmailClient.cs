using System.Threading.Tasks;

namespace NotionTaskAutomation;

public interface IEmailClient
{
    Task SendEmail();
}