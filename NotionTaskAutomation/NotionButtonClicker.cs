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
using NotionTaskAutomation.Db;
using NotionTaskAutomation.Objects;

namespace NotionTaskAutomation;

public class NotionButtonClicker : INotionButtonClicker
{
    private readonly IHttpClientFactory m_httpClientFactory;
    private readonly string m_bearerToken;
    private readonly NotionDbContext m_notionDbContext;

    public NotionButtonClicker(IHttpClientFactory httpClientFactory,
        NotionDbContext notionDbContext,
        IConfiguration configuration)
    {
        m_httpClientFactory = httpClientFactory;
        m_notionDbContext = notionDbContext;

        m_bearerToken = configuration.GetValue<string>("bearerToken");
        if (string.IsNullOrEmpty(m_bearerToken))
            m_bearerToken = Environment.GetEnvironmentVariable("bearerToken");
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
            httpRequestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json");

        httpRequestMessage.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", m_bearerToken);
        httpRequestMessage.Headers.Add("Notion-Version", "2022-06-28");
        var response = await httpClient.SendAsync(httpRequestMessage);
        response.EnsureSuccessStatusCode(); // fix this
        var responseAsObject =
            await JsonSerializer.DeserializeAsync<T>(
                await response.Content.ReadAsStreamAsync());
        if (responseAsObject == null)
            throw new Exception("failed to deserialize");

        return responseAsObject;
    }

    public async Task<List<Guid>> GetSharedDatabases()
    {
        var searchFilter = new SearchFilter
        {
            Filter = new()
            {
                Value = "database",
                Property = "object"
            }
        };

        var responseAsObject = await GetResponseAsync<QueryObject>(
            $"https://api.notion.com/v1/search", HttpMethod.Post, JsonSerializer.Serialize(searchFilter));

        return responseAsObject.Results.Select(p => p.Id).ToList();
    }

    public async Task<List<string>> GetStates(Guid notionPageId)
    {
        var responseAsObject = await GetResponseAsync<StatesObject>(
            $"https://api.notion.com/v1/databases/{notionPageId}", HttpMethod.Get);

        return responseAsObject.Properties.Status.Select.Options.Select(p => p.Name).ToList();
    }

    public async Task<List<TaskObject>> GetTasks(Guid notionPageId)
    {
        Guid? continuationToken = null;
        List<TaskObject> tasks = new();
        var notionPageRules = GetNotionPageRules(notionPageId);
        var filter = ConstructFilter(notionPageRules);

        do
        {
            var response = await GetResponseAsync<QueryObject>(
                $"https://api.notion.com/v1/databases/{notionPageId}/query",
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

    public async Task UpdateTasks(Guid notionPageId)
    {
        var tasks = await GetTasks(notionPageId);
        var notionPageRules = GetNotionPageRules(notionPageId);

        foreach (var task in tasks)
        {
            var nextState = notionPageRules.Find(p => p.StartingState == task.Properties.Status.Select.Name).EndingState;
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

    public List<NotionPageRule> GetNotionPageRules(Guid notionPageId)
    {
        return m_notionDbContext.NotionPageRules.Where(p => p.PageId == notionPageId).ToList();
    }

    private Filter ConstructFilter(List<NotionPageRule> notionPageRules)
    {
        return new Filter()
        {
            Or = notionPageRules.Select(p =>
            {
                var date = DateTime.Today.AddDays(p.DayOffset).Date.ToString("yyyy-MM-dd");

                return new Or
                {
                    And = new List<And>
                    {
                        new() { Property = "Status", Select = new FilterSelect { Equals = p.StartingState } },
                        new()
                        {
                            Property = "Date", Date = new FilterDateObject
                            {
                                OnOrBefore = p.OnDay == "OnOrBefore" ? date : null,
                                Before = p.OnDay != "OnOrBefore" ? date : null
                            }
                        },
                    }
                };
            }).ToList()
        };
    }
}
