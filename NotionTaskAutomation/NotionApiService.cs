using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NotionTaskAutomation.Db;
using NotionTaskAutomation.Objects;
using LanguageExt;
using Microsoft.AspNetCore.WebUtilities;
using static System.String;

namespace NotionTaskAutomation;

public class NotionApiService(
    IHttpClientFactory httpClientFactory,
    NotionDbContext notionDbContext,
    IHttpContextAccessor httpContextAccessor)
    : INotionApiService
{
    private readonly JsonSerializerOptions m_jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<List<Guid>> GetSharedDatabases()
    {
        var searchFilter = new SearchFilter
        {
            Filter = new SearchFilterObject
            {
                Value = "database",
                Property = "object"
            }
        };

        var responseAsObject = await GetResponseAsync<QueryObject>(
            "https://api.notion.com/v1/search", HttpMethod.Post, JsonSerializer.Serialize(searchFilter));

        return responseAsObject.Results.Select(p => p.Id).ToList();
    }

    public async Task<List<string>> GetStates(Guid notionDatabaseId)
    {
        var responseAsObject = await GetResponseAsync<StatesObject>(
            $"https://api.notion.com/v1/databases/{notionDatabaseId}", HttpMethod.Get);

        return responseAsObject.Properties.Status.Select.Options.Select(p => p.Name).ToList();
    }

    public async Task<List<TaskObject>> GetTasks(Guid notionDatabaseId)
    {
        Guid? continuationToken = null;
        List<TaskObject> tasks = [];
        var notionPageRules = GetNotionDatabaseRules(notionDatabaseId);
        var filter = ConstructFilter(notionPageRules);

        do
        {
            var response = await GetResponseAsync<QueryObject>(
                $"https://api.notion.com/v1/databases/{notionDatabaseId}/query",
                HttpMethod.Post,
                JsonSerializer.Serialize(
                    new TasksFilter
                    {
                        Filter = filter,
                        StartCursor = continuationToken
                    },
                    m_jsonOptions)
            );
            continuationToken = response.NextCursor;
            tasks = tasks.Concat(response.Results).ToList();
        } while (continuationToken != null);

        return tasks;
    }

    public async Task UpdateTasks(Guid notionDatabaseId)
    {
        var tasks = await GetTasks(notionDatabaseId);
        var notionPageRules = GetNotionDatabaseRules(notionDatabaseId);

        foreach (var task in tasks)
        {
            var nextState = notionPageRules.Find(p => p.StartingState == task.Properties.Status.Select.Name)
                .EndingState;
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
                JsonSerializer.Serialize(filter, m_jsonOptions)
            );
        }
    }

    public List<NotionDatabaseRule> GetNotionDatabaseRules(Guid notionDatabaseId) => notionDbContext.NotionDatabaseRules.Where(p => p.DatabaseId == notionDatabaseId).ToList();

    private string GetBearerToken() => httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

    private async Task<T> GetResponseAsync<T>(string requestUri, HttpMethod httpMethod, string body = null)
    {
        var httpClient = httpClientFactory.CreateClient();
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = httpMethod,
            RequestUri = new Uri(requestUri)
        };
        if (body != null)
            httpRequestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json");

        var bearerToken = GetBearerToken();
        if(IsNullOrEmpty(bearerToken))
            throw new UnauthorizedAccessException("Bearer token is required");

        httpRequestMessage.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", bearerToken);
        httpRequestMessage.Headers.Add("Notion-Version", "2022-06-28");
        var response = await httpClient.SendAsync(httpRequestMessage);
        if (!response.IsSuccessStatusCode)
        {
            var statusCode = response.StatusCode;
            throw new BadHttpRequestException($"Response status code does not indicate success: {(int)statusCode} ({ReasonPhrases.GetReasonPhrase((int)statusCode)}).", (int)response.StatusCode);
        }

        var responseAsObject =
            await JsonSerializer.DeserializeAsync<T>(
                await response.Content.ReadAsStreamAsync());
        if (responseAsObject == null)
            throw new Exception("failed to deserialize");

        return responseAsObject;
    }

    private Filter ConstructFilter(List<NotionDatabaseRule> notionPageRules)
    {
        return new Filter
        {
            Or = notionPageRules.Select(p =>
            {
                var date = DateTime.Today.AddDays(p.DayOffset).Date.ToString("yyyy-MM-dd");

                return new Or
                {
                    And =
                    [
                        new And { Property = "Status", Select = new FilterSelect { Equals = p.StartingState } },
                        new And
                        {
                            Property = "Date", Date = new FilterDateObject
                            {
                                OnOrBefore = p.OnDay == "OnOrBefore" ? date : null,
                                Before = p.OnDay != "OnOrBefore" ? date : null
                            }
                        }
                    ]
                };
            }).ToList()
        };
    }
}