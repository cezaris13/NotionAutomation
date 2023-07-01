using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NotionAutomationButtonAutomation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);
            builder.ConfigureServices(services =>
            {
                services.AddHttpClient();
                services.AddSingleton<INotionButtonClicker, NotionButtonClicker>();
            });

            var host = builder.Build();
            
            await ExecuteTask(host.Services);

            await host.RunAsync();
        }

        static async Task ExecuteTask(IServiceProvider hostProvider)
        {
            var serviceScope = hostProvider.CreateScope();
            var serviceProvider = serviceScope.ServiceProvider;
            var notionButtonClicker = serviceProvider.GetService<INotionButtonClicker>();
            await notionButtonClicker.ExecuteClickAsync();
        }
    }
}