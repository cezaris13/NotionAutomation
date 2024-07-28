using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NotionTaskAutomation.Objects;

namespace NotionTaskAutomation;

public class NotionButtonClicker : INotionButtonClicker
{
    private readonly IHttpClientFactory m_httpClientFactory;
    private readonly string m_bearerToken;
    private readonly Guid m_databaseId;

    public NotionButtonClicker(IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        m_httpClientFactory = httpClientFactory;
        
        m_bearerToken = configuration.GetValue<string>("bearerToken");
        if (string.IsNullOrEmpty(m_bearerToken))
        {
            m_bearerToken = Environment.GetEnvironmentVariable("bearerToken");
            m_databaseId = Guid.Parse(Environment.GetEnvironmentVariable("databaseId") ?? "");
        }
        m_databaseId = configuration.GetValue<Guid>("databaseId");
    }

    private async Task<T> GetResponseAsync<T>(string requestUri, HttpMethod httpMethod, string body = null)
    {
        var httpClient = m_httpClientFactory.CreateClient();
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = httpMethod,
            RequestUri = new Uri(requestUri),
        };
        if (body != null)
        {
            httpRequestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        httpRequestMessage.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", m_bearerToken);
        httpRequestMessage.Headers.Add("Notion-Version", "2022-06-28");
        var response = await httpClient.SendAsync(httpRequestMessage);
        response.EnsureSuccessStatusCode();
        var responseAsObject =
            await JsonSerializer.DeserializeAsync<T>(
                await response.Content.ReadAsStreamAsync());
        if (responseAsObject == null)
        {
            throw new Exception("failed to deserialize");
        }

        return responseAsObject;
    }

    public async Task<List<string>> GetStates()
    {
        var responseAsObject = await GetResponseAsync<StatesObject>(
            $"https://api.notion.com/v1/databases/{m_databaseId}", HttpMethod.Get);
        return responseAsObject.Properties.Status.Select.Options.Select(p => p.Name).ToList();
    }

    public async Task<List<TaskObject>> GetTasks()
    {
        Guid? continuationToken = null;
        List<TaskObject> tasks = new();
        var filter = ConstructFilter();

        do
        {
            var response = await GetResponseAsync<QueryObject>(
                $"https://api.notion.com/v1/databases/{m_databaseId}/query",
                HttpMethod.Post,
                JsonSerializer.Serialize(
                    new TasksFilter
                    {
                        Filter = filter,
                        StartCursor = continuationToken
                    },
                    new JsonSerializerOptions()
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    })    
                );
            continuationToken = response.NextCursor;
            tasks = tasks.Concat(response.Results).ToList();
        } while (continuationToken != null);

        return tasks;
    }

    public async Task UpdateTasks()
    {
        var tasks = await GetTasks();
        var states = await GetStates();

        foreach (var task in tasks)
        {
            var nextState = states[states.IndexOf(task.Properties.Status.Select.Name) + 1];
            var filter = new UpdateTaskObject
            {
                Properties = new PropertyObject
                {
                    Status = new Status
                    {
                        Select = new Select
                        {
                            Name = nextState
                        }
                    }
                }
            };
            
            await GetResponseAsync<StatesObject>(
                $"https://api.notion.com/v1/pages/{task.Id}",
                HttpMethod.Patch,
                JsonSerializer.Serialize(
                    filter,
                    new JsonSerializerOptions()
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    })     
                );
        }
    }

    private Filter ConstructFilter()
    {
        return new Filter()
        {
            Or = new List<Or>
            {
                new()
                {
                    And = new List<And>
                    {
                        new() { Property = "Status", Select = new FilterSelect() { Equals = "TODO tomorrow" } },
                        new() { Property = "Date", Date = new FilterDateObject() { OnOrBefore = DateTime.Today.Date.ToString("yyyy-MM-dd") } },
                    }
                },
                new()
                {
                    And = new List<And>
                    {
                        new() { Property = "Status", Select = new FilterSelect() { Equals = "Event" } },
                        new() { Property = "Date", Date = new FilterDateObject() { Before = DateTime.Today.Date.ToString("yyyy-MM-dd") } },
                    }
                },
                new()
                {
                    And = new List<And>
                    {
                        new() { Property = "Status", Select = new FilterSelect() { Equals = "To Do" } },
                        new() { Property = "Date", Date = new FilterDateObject() { OnOrBefore = DateTime.Today.AddDays(1).Date.ToString("yyyy-MM-dd") } },
                    }
                }
            }
        };
    }
}
