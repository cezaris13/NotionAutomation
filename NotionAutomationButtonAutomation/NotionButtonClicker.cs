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
using NotionAutomationButtonAutomation.Objects;

namespace NotionAutomationButtonAutomation
{
    public class NotionButtonClicker : INotionButtonClicker
    {
        private readonly IHttpClientFactory m_httpClientFactory;
        private readonly Guid blockId;
        private readonly Guid toDoListId;
        private readonly Guid collectionId;
        private readonly string token;

        public NotionButtonClicker(IHttpClientFactory httpClientFactory)
        {
            m_httpClientFactory = httpClientFactory;
            blockId = new Guid("0f5a4922-b70a-4672-9e72-a0757b105313");
            toDoListId = new Guid("2bdfee17-7532-4986-82ae-9756c7840db3");
            collectionId = new Guid("f46a8f52-4b96-4c0c-b025-c73646e198ef");
            token =
                "v02%3Auser_token_or_cookies%3AJEgVnwWEzuK3w0sHqXwmmyhJyEomMQc00iUGjewJQKc35Jyi2Mdf5vWds6lvLdvwDiZvj6Wgbf1sacwMQdx_XLoWma3rBupGdUHYWvFATXSMnxylCjQolridnNvt8_GWshPn";
        }

        public async Task ExecuteClickAsync()
        {
            Console.WriteLine("status");
            try
            {
                var userId = await GetUserId();
                Console.WriteLine($"{userId}");
                var spaceId = await GetSpaceId();
                Console.WriteLine($"{spaceId}");
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
                BlockId = blockId,
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