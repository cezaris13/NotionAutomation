using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NotionTaskAutomation.Db;

namespace NotionTaskAutomation;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "NotionAutomation", Version = "v1" });
        });
        builder.Services.AddMvcCore();
        builder.Services.AddMvc();
        builder.Services.AddHttpClient();
        
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
        IConfiguration configuration = configurationBuilder.Build();
        
        builder.Services.AddSingleton(_ => configuration);
        builder.Services.AddSingleton<INotionButtonClicker, NotionButtonClicker>();
        builder.Services.AddDbContext<NotionDbContext>();
        
        var host = builder.Build();
        host.UseSwagger();
        host.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("v1/swagger.json", "v1");
        });
        
        host.MapControllers();

        await host.RunAsync();
    }
}