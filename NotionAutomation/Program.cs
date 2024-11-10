using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using NotionAutomation;
using NotionAutomation.Db;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NotionAutomation", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddMvcCore();
builder.Services.AddMvc();
builder.Services.AddHttpClient();

builder.Services.AddScoped<INotionApiService, NotionApiService>();
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false);

IConfiguration configuration = configurationBuilder.Build();
var connectionString = configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString)) {
    builder.Services.AddDbContext<NotionDbContext>(options => {
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var dbPath = Path.Join(path, "notionrules.db");
        options.UseSqlite($"Data Source={dbPath}");
    });
}
else {
    builder.Services.AddDbContext<NotionDbContext>(options => {
        options.UseNpgsql(connectionString);
    });
}

var host = builder.Build();
host.UseSwagger();
host.UseSwaggerUI(options => { options.SwaggerEndpoint("v1/swagger.json", "v1"); });

host.MapControllers();

await host.RunAsync();

[ExcludeFromCodeCoverage]
public partial class Program { }