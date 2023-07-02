using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
                var configurationBuilder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false);

                IConfiguration configuration = configurationBuilder.Build();
                services.AddHttpClient();
                services.AddSingleton<IConfiguration>(provider => configuration);

                services.AddSingleton<INotionButtonClicker, NotionButtonClicker>();
                services.AddSingleton<IFilterFactory, FilterFactory>();
            });

            var host = builder.Build();

            await ExecuteTask(host.Services);

            // await host.RunAsync();
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