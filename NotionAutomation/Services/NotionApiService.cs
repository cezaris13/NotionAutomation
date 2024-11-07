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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using NotionAutomation.DataTypes;
using NotionAutomation.Db;
using NotionAutomation.Objects;
using static System.String;

namespace NotionAutomation;

public class NotionApiService(
    IHttpClientFactory httpClientFactory,
    NotionDbContext notionDbContext,
    IHttpContextAccessor httpContextAccessor)
    : INotionApiService {
    private readonly JsonSerializerOptions m_jsonOptions = new() {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<Result<List<string>, ActionResult>> GetStates(Guid notionDatabaseId) {
        var responseAsObject = await GetResponseAsync<StatesObject>(
            $"https://api.notion.com/v1/databases/{notionDatabaseId}", HttpMethod.Get);

        return responseAsObject.Match<Result<List<string>, ActionResult>>(
            response => response.Properties.Status.Select.Options.Select(p => p.Name).ToList(),
            error => error);
    }

    public List<NotionDatabaseRule> GetNotionDatabaseRules(Guid notionDatabaseId) {
        return notionDbContext.NotionDatabaseRules.Where(p => p.DatabaseId == notionDatabaseId).ToList();
    }

    public async Task<Result<List<TaskObject>, ActionResult>> GetTasks(Guid notionDatabaseId) {
        Guid? continuationToken = null;
        List<TaskObject> tasks = [];
        var notionPageRules = GetNotionDatabaseRules(notionDatabaseId);
        var filter = ConstructFilter(notionPageRules);

        do {
            var response = await GetResponseAsync<QueryObject>(
                $"https://api.notion.com/v1/databases/{notionDatabaseId}/query",
                HttpMethod.Post,
                JsonSerializer.Serialize(
                    new TasksFilter {
                        Filter = filter,
                        StartCursor = continuationToken
                    },
                    m_jsonOptions)
            );

            if (!response.IsOk)
                return response.Error;

            continuationToken = response.Value.NextCursor;
            tasks = tasks.Concat(response.Value.Results).ToList();
        } while (continuationToken != null);

        return tasks;
    }

    public async Task<Result<List<Guid>, ActionResult>> GetSharedDatabases() {
        var searchFilter = new SearchFilter {
            Filter = new SearchFilterObject {
                Value = "database",
                Property = "object"
            }
        };

        var responseAsObject = await GetResponseAsync<QueryObject>(
            "https://api.notion.com/v1/search", HttpMethod.Post, JsonSerializer.Serialize(searchFilter));

        return responseAsObject.Match<Result<List<Guid>, ActionResult>>(
            response => response.Results.Select(p => p.Id).ToList(),
            error => error);
    }

    public async Task<Result<Unit, ActionResult>> UpdateTasks(Guid notionDatabaseId) {
        var tasks = await GetTasks(notionDatabaseId);
        var notionPageRules = GetNotionDatabaseRules(notionDatabaseId);
        if (!tasks.IsOk)
            return tasks.Error;

        foreach (var task in tasks.Value) {
            var nextState = notionPageRules.Find(p => p.StartingState == task.Properties.Status.Select.Name)
                .EndingState;

            var filter = new UpdateTaskObject {
                Properties = new PropertyObject {
                    Status = new Status {
                        Select = new Select {
                            Name = nextState
                        }
                    }
                }
            };

            var updateResponse = await GetResponseAsync<StatesObject>(
                $"https://api.notion.com/v1/pages/{task.Id}",
                HttpMethod.Patch,
                JsonSerializer.Serialize(filter, m_jsonOptions)
            );

            if (!updateResponse.IsOk)
                return updateResponse.Error;
        }

        return Unit.Value;
    }

    private Filter ConstructFilter(List<NotionDatabaseRule> notionPageRules) {
        return new Filter {
            Or = notionPageRules.Select(p => {
                var date = DateTime.Today.AddDays(p.DayOffset).Date.ToString("yyyy-MM-dd");

                return new Or {
                    And = [
                        new And { Property = "Status", Select = new FilterSelect { Equals = p.StartingState } },
                        new And {
                            Property = "Date", Date = new FilterDateObject {
                                OnOrBefore = p.OnDay == "OnOrBefore" ? date : null,
                                Before = p.OnDay != "OnOrBefore" ? date : null
                            }
                        }
                    ]
                };
            }).ToList()
        };
    }

    private Result<string, ActionResult> GetBearerToken() {
        var bearerToken = httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault()?.Split(" ")
            .Last();

        if (IsNullOrEmpty(bearerToken))
            return new ObjectResult("Bearer token was not found") {
                StatusCode = 401
            };

        return bearerToken;
    }

    private async Task<Result<T, ActionResult>> GetResponseAsync<T>(string requestUri, HttpMethod httpMethod,
        string body = null) {
        var bearerToken = GetBearerToken();

        if (!bearerToken.IsOk)
            return bearerToken.Error;

        var httpClient = httpClientFactory.CreateClient();
        var httpRequestMessage = new HttpRequestMessage {
            Method = httpMethod,
            RequestUri = new Uri(requestUri)
        };
        if (body != null)
            httpRequestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json");

        httpRequestMessage.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", bearerToken.Value);
        httpRequestMessage.Headers.Add("Notion-Version", "2022-06-28");

        var response = await httpClient.SendAsync(httpRequestMessage);

        if (!response.IsSuccessStatusCode) {
            var statusCode = (int)response.StatusCode;
            return new ObjectResult(
                $"Response status code does not indicate success: {statusCode} ({ReasonPhrases.GetReasonPhrase(statusCode)}).") {
                StatusCode = statusCode
            };
        }

        try {
            var responseAsObject =
                await JsonSerializer.DeserializeAsync<T>(
                    await response.Content.ReadAsStreamAsync());

            return responseAsObject;
        }
        catch (JsonException exception) {
            return new ObjectResult(exception.Message) {
                StatusCode = 500
            };
        }
    }
}