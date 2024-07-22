using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace NotionTaskAutomation;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        
        builder.Services.AddControllers(); 
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "NotionAutomation", Version = "v1" });
        });
        builder.Services.AddMvcCore();
        builder.Services.AddMvc();

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
        IConfiguration configuration = configurationBuilder.Build();
        // services.AddHttpClient();
        builder.Services.AddSingleton(_ => configuration);
        builder.Services.AddSingleton<INotionButtonClicker, NotionButtonClicker>();
        builder.Services.AddSingleton<IFilterFactory, FilterFactory>();
        var host = builder.Build();
        host.UseSwagger();
        host.UseSwaggerUI();
        host.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.RoutePrefix = String.Empty;
        });
        // await ExecuteTask(host.Services);

        await host.RunAsync();
    }

    // private static async Task ExecuteTask(IServiceProvider hostProvider)
    // {
    //     var serviceScope = hostProvider.CreateScope();
    //     var serviceProvider = serviceScope.ServiceProvider;
    //     var notionButtonClicker = serviceProvider.GetService<INotionButtonClicker>();
    //     await notionButtonClicker.ExecuteClickAsync();
    // }
}