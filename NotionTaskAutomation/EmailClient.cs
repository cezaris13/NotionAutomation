using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace NotionTaskAutomation;

public class EmailClient : IEmailClient
{
    private readonly IConfiguration m_configuration;

    public EmailClient(IConfiguration configuration)
    {
        m_configuration = configuration;
    }
    public async Task SendEmail()
    {
        var sendGridApiKey = m_configuration.GetValue<string>("SendGridApiKey");
        Console.WriteLine($"Email has been sent {sendGridApiKey}");
    }
}