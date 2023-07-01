using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NotionAutomationButtonAutomation.Extensions;
using NotionAutomationButtonAutomation.Objects;

namespace NotionAutomationButtonAutomation
{

    
    public class NotionButtonClicker : INotionButtonClicker
    {
        private readonly IHttpClientFactory m_httpClientFactory;
        private readonly Guid m_blockId;
        private readonly Guid m_toDoListId;
        private readonly Guid m_collectionId;
        private readonly string m_token;

        public NotionButtonClicker(IHttpClientFactory httpClientFactory)
        {
            m_httpClientFactory = httpClientFactory;
            m_blockId = new Guid("0f5a4922-b70a-4672-9e72-a0757b105313");
            m_toDoListId = new Guid("2bdfee17-7532-4986-82ae-9756c7840db3");
            m_collectionId = new Guid("f46a8f52-4b96-4c0c-b025-c73646e198ef");
            m_token =
                "v02%3Auser_token_or_cookies%3AJEgVnwWEzuK3w0sHqXwmmyhJyEomMQc00iUGjewJQKc35Jyi2Mdf5vWds6lvLdvwDiZvj6Wgbf1sacwMQdx_XLoWma3rBupGdUHYWvFATXSMnxylCjQolridnNvt8_GWshPn";
        }

        public async Task ExecuteClickAsync()
        {
            Console.WriteLine("status");
            try
            {
                // var userId = await GetUserId();
                // Console.WriteLine($"{userId}");
                // var spaceId = await GetSpaceId();
                // Console.WriteLine($"{spaceId}");
                await UpdateBlockProperties(Guid.Parse("2776e0a6-fbfe-45ef-8ff2-14145037ce4d"),States.Doing);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception is thrown: {ex.Message}");
            }
        }

        private async Task<Guid> GetUserId()
        {
            var responseAsObject =
                await GetResponseAsync<UsersObject>("https://api.notion.com/v1/users", HttpMethod.Get);

            var userId = responseAsObject.Results.Single(p => p.Type == "person").Id;
            return userId;
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
        
        private async Task<string> GetListOfBlocksToBeUpdated()
        {
            
            var responseAsObject = await GetResponseAsync<string>("https://www.notion.so/api/v3/queryCollection?src=queryCollectionAction", HttpMethod.Post);
            return responseAsObject;
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
            var response =
                await GetResponseAsync<string>($"https://api.notion.com/v1/pages/{blockId}", HttpMethod.Patch, bodyAsString);
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
                httpRequestMessage.Content = new StringContent(body,Encoding.UTF8,"application/json");
            }

            httpRequestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", "secret_SdaJ2UQX71BrSgEy0CHeBOnwwO5rVufBIwNd8L9ddpE");
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
    }
}