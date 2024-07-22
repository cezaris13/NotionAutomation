using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NotionTaskAutomation.Extensions;
using NotionTaskAutomation.Objects;

namespace NotionTaskAutomation;

public class NotionButtonClicker : INotionButtonClicker
{
    private readonly IHttpClientFactory m_httpClientFactory;
    private readonly IFilterFactory m_filterFactory;
    private readonly Guid m_blockId;
    private readonly string m_bearerToken;
    private readonly string m_cookie;

    public NotionButtonClicker(IHttpClientFactory httpClientFactory, IFilterFactory filterFactory,
        IConfiguration configuration)
    {
        m_httpClientFactory = httpClientFactory;
        m_filterFactory = filterFactory;
        m_blockId = configuration.GetValue<Guid>("blockId");
        m_bearerToken = configuration.GetValue<string>("bearerToken");
        m_cookie = configuration.GetValue<string>("cookie");
    }

    public async Task<string> ExecuteClickAsync()
    {
        try
        {
            var spaceId = await GetSpaceId();
            var filters = GetFilters(spaceId);
            foreach (var filter in filters)
            {
                var blockIds = await GetListOfBlocksToBeUpdated(filter.Item1);
                foreach (var blockId in blockIds)
                {
                    await UpdateBlockProperties(blockId, filter.Item2);
                }
            }

            return "Successfully updated the tasks";
        }
        catch (Exception ex)
        {
            return $"Exception is thrown: {ex.Message}";
        }
    }

    private async Task<Guid> GetSpaceId()
    {
        var publicPageData = new PublicPageData
        {
            Type = "block-space",
            Name = "page",
            BlockId = m_blockId,
            ShouldDuplicate = false,
            RequestedOnPublicdomain = false
        };

        var data = JsonSerializer.Serialize(publicPageData);

        var responseAsObject =
            await GetResponseAsync<PageObject>("https://www.notion.so/api/v3/getPublicPageData", HttpMethod.Post,
                data);

        return responseAsObject.SpaceId;
    }

    private async Task<List<Guid>> GetListOfBlocksToBeUpdated(string body)
    {
        var responseAsObject = await GetResponseAsync<FilterResponseObject>(
            "https://www.notion.so/api/v3/queryCollection?src=queryCollectionAction", HttpMethod.Post, body);
        return responseAsObject.Result.ReducerResults.Results.BlockIds;
    }

    private async Task UpdateBlockProperties(Guid blockId, States state)
    {
        var body = new PropertiesObject
        {
            Properties = new PropertyObject
            {
                Status = new Status
                {
                    Select = new Select
                    {
                        Name = state.ToDescriptionString()
                    }
                }
            }
        };

        var bodyAsString = JsonSerializer.Serialize(body);
        await GetResponseAsync<object>($"https://api.notion.com/v1/pages/{blockId}", HttpMethod.Patch,
            bodyAsString);
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
        httpRequestMessage.Headers.Add("Cookie", m_cookie);
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

    private List<Tuple<string, States>> GetFilters(Guid spaceId)
    {
        var todoTomorrowFilter = m_filterFactory.CreateTodoTomorrowFilter(spaceId);
        var todoFilter = m_filterFactory.CreateTodoFilter(spaceId);
        var eventFilter = m_filterFactory.CreateEventFilter(spaceId);

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true
        };

        return new List<Tuple<string, States>>
        {
            new Tuple<string, States>(JsonSerializer.Serialize(todoTomorrowFilter, jsonSerializerOptions).Replace("Filters","filters"),
                States.Doing),
            new Tuple<string, States>(JsonSerializer.Serialize(todoFilter, jsonSerializerOptions).Replace("Filters","filters"),
                States.TodoTomorrow),
            new Tuple<string, States>(JsonSerializer.Serialize(eventFilter, jsonSerializerOptions).Replace("Filters","filters"),
                States.EventDone)
        };
    }
}
