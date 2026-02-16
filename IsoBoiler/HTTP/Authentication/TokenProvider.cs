using IsoBoiler.Json;
using IsoBoiler.Logging;
using IsoBoiler.MemoryCache;
using IsoBoiler.Validation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text;

namespace IsoBoiler.HTTP.Authentication
{
    public class TokenProvider<TTokenFormat> : ITokenProvider<TTokenFormat> where TTokenFormat : IAuthToken
    {
        private readonly ILog _logger;
        private readonly AuthSettings<TTokenFormat> _authSettings;
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;

        public TokenProvider(ILog logger, IOptionsSnapshot<AuthSettings<TTokenFormat>> authSettings, IMemoryCache memoryCache, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _authSettings = authSettings.Value;
            _authSettings.Validate(); //Validate here so we give a good error to consumers that are missing configurations
            _memoryCache = memoryCache;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetTokenAsync()
        {
            return await _memoryCache.Get(typeof(TTokenFormat).Name, MintNewToken, new TimeSpan(0, 0, _authSettings.TTL_Seconds));
        }

        private async Task<string> MintNewToken()
        {
            _logger.Log($"Attempting to mint new '{typeof(TTokenFormat).Name}' Token from {_authSettings.URI}");
            var httpClient = _httpClientFactory.CreateClient(typeof(TTokenFormat).Name);

            //This is happening in the Extension where the service is being registered.
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_authorizationSettings.CurrentValue.ClientID}:{_authorizationSettings.CurrentValue.ClientSecret}")));

            var response = await httpClient.PostAsync(_authSettings.URI, new StringContent($"grant_type={_authSettings.GrantType}&scope={_authSettings.Scope}", Encoding.UTF8, "application/x-www-form-urlencoded"));
            var result = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var newToken = result.ToObject<TTokenFormat>();
            if (newToken != null)
            {
                _logger.Log($"Minted new '{typeof(TTokenFormat).Name}' Token from {_authSettings.URI}:{Environment.NewLine}{newToken.ToJson()}");
                return newToken.GetToken();
            }
            else
            {
                throw new Exception($"Failed to retrieve '{typeof(TTokenFormat).Name}' Token from {_authSettings.URI}");
            }
        }
    }
}
