using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;

namespace IsoBoiler.HTTP.Authentication
{
    public static class Extensions
    {
        public static IServiceCollection AddTokenProvider<TTokenFormat>(this IServiceCollection services, IConfigurationSection configurationSection) where TTokenFormat : IAuthToken
        {
            services.Configure<AuthSettings<TTokenFormat>>(configurationSection);

            services.AddHttpClient(typeof(TTokenFormat).Name, client =>
            {
                var authorizationSettings = services.GetService<IOptionsSnapshot<AuthSettings<TTokenFormat>>>().Value;
                client.BaseAddress = new Uri(authorizationSettings.URI);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{authorizationSettings.ClientID}:{authorizationSettings.ClientSecret}")));
            });
            services.AddScoped<ITokenProvider<TTokenFormat>, TokenProvider<TTokenFormat>>();
            return services.AddScoped(typeof(AuthTokenHandler<TTokenFormat>));
        }

        public static IHttpClientBuilder UseTokenProvider<TTokenFormat>(this IHttpClientBuilder builder) where TTokenFormat : IAuthToken
        {
            return builder.AddHttpMessageHandler<AuthTokenHandler<TTokenFormat>>();
        }

        public static IServiceCollection AddDefaultOktaTokenProvider(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            return services.AddTokenProvider<DefaultOktaToken>(configurationSection);
        }

        public static IHttpClientBuilder UseDefaultOktaTokenProvider(this IHttpClientBuilder builder)
        {
            return builder.UseTokenProvider<DefaultOktaToken>();
        }
    }
}
