using IsoBoiler.HTTP.Authentication;
using IsoBoiler.Json;

namespace IsoBoiler.UnitTests.Helpers.HTTP.Authentication
{
    internal interface IExampleHttpService_Other
    {
        Task<string> GetRequest();
        Task<string> PostRequest();
    }
    internal class ExampleHttpService_Other : IExampleHttpService_Other
    {
        private readonly HttpClient _httpClient;
        public ExampleHttpService_Other(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MyHttpClientName_Other");
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
