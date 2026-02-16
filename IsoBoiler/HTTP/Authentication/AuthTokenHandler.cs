using System.Net.Http.Headers;

namespace IsoBoiler.HTTP.Authentication
{
    public class AuthTokenHandler<TTokenFormat> : DelegatingHandler where TTokenFormat : IAuthToken
    {
        private readonly ITokenProvider<TTokenFormat> _tokenProvider;

        public AuthTokenHandler(ITokenProvider<TTokenFormat> tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _tokenProvider.GetTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
