using IsoBoiler.HTTP.Authentication;
using IsoBoiler.Json;

namespace IsoBoiler.UnitTests.Helpers.HTTP.Authentication
{
    internal interface IExampleHttpService
    {
        Task<string> GetRequest();
        Task<string> PostRequest();
    }
    internal class ExampleHttpService : IExampleHttpService
    {
        private readonly HttpClient _httpClient;
        public ExampleHttpService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MyHttpClientName");
        }
        public async Task<string> GetRequest()
        {
            var result = await _httpClient.GetAsync("");
            return await result.Content.ReadAsStringAsync();
        }
        public async Task<string> PostRequest()
        {
            var contentData = new StringContent(new DefaultOktaToken().ToJson(), System.Text.Encoding.Default, "application/json");
            var result = await _httpClient.PostAsync("", contentData);
            return await result.Content.ReadAsStringAsync();
        }
    }
}
