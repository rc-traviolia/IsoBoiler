using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IsoBoiler.Testing
{
    public static class ConfigHelper
    {
        public static IConfiguration BuildConfiguration(string configurationUri, string configurationFilter, string? configurationSnapshot = null)
        {
            if(configurationSnapshot is null)
            {
                return new ConfigurationBuilder()
                .AddAzureAppConfiguration(options =>
                {
                    options.Connect(new Uri(configurationUri), new DefaultAzureCredential())
                           .Select($"{configurationFilter}:*")
                           .ConfigureKeyVault(kv => { kv.SetCredential(new DefaultAzureCredential()); })
                           .ConfigureRefresh(refreshOptions =>
                           {
                               refreshOptions.Register($"{configurationFilter}:Sentinel", refreshAll: true)
                                               .SetRefreshInterval(TimeSpan.FromSeconds(30)); //Default value is 30
                           });
                })
                .Build();
            }
            else
            {
                return new ConfigurationBuilder()
                .AddAzureAppConfiguration(options =>
                {
                    options.Connect(new Uri(configurationUri), new DefaultAzureCredential())
                           .Select($"{configurationFilter}:*")
                           .SelectSnapshot(configurationSnapshot)
                           .ConfigureKeyVault(kv => { kv.SetCredential(new DefaultAzureCredential()); })
                           .ConfigureRefresh(refreshOptions =>
                           {
                               refreshOptions.Register($"{configurationFilter}:Sentinel", refreshAll: true)
                                               .SetRefreshInterval(TimeSpan.FromSeconds(30)); //Default value is 30
                           });
                })
                .Build();
            }
            
        }

        public static IOptionsSnapshot<TSettingsModel> GetSettings<TSettingsModel>(this IConfiguration configuration, string configurationSection) where TSettingsModel : class, new()
        {
            var services = new ServiceCollection();
            services.Configure<TSettingsModel>(configuration.GetSection(configurationSection));
            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<IOptionsSnapshot<TSettingsModel>>();
        }
    }
}
