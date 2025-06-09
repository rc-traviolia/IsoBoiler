using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace IsoBoiler.Testing
{
    public static class ConfigHelper
    {
        public static IConfiguration BuildConfiguration(string configurationUri, string? configurationFilter = null, string? configurationSnapshot = null)
        {
            if (configurationFilter is null)
            {
                //.SkipLast(1) skips the typical .Tests suffix, for projects where that nomenclature is used. 
                configurationFilter = Assembly.GetCallingAssembly()!.GetName()!.Name!.Contains('.') ? Assembly.GetCallingAssembly()!.GetName()!.Name!.Split('.').SkipLast(1).Last() : Assembly.GetCallingAssembly()!.GetName()!.Name!;
            }

            if (configurationSnapshot is null)
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

        public static TSettingsModel GetSettings<TSettingsModel>(this IConfiguration configuration, string configurationSection) where TSettingsModel : class, new()
        {
            var services = new ServiceCollection();
            services.Configure<TSettingsModel>(configuration.GetSection(configurationSection));
            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<IOptionsSnapshot<TSettingsModel>>().Value;
        }

        public static TSettingsModel GetSettings<TSettingsModel>(string configurationUri, string configurationSection) where TSettingsModel : class, new()
        {
            var configuration = BuildConfiguration(configurationUri, configurationSection);
            var services = new ServiceCollection();
            services.Configure<TSettingsModel>(configuration.GetSection(configurationSection));
            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<IOptionsSnapshot<TSettingsModel>>().Value;
        }

        public static IServiceProvider GetDefaultServiceProvider()
        {
            var host = new HostBuilder().ConfigureFunctionsWorkerDefaults()
                                        .AddDefaultJsonSerializerOptions()
                                        .Build();

            return host.Services.GetRequiredService<IServiceProvider>();
        }

        public static IServiceProvider GetServiceProvider(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            var host = new HostBuilder().ConfigureFunctionsWorkerDefaults()
                                        .AddDefaultJsonSerializerOptions()
                                        .ConfigureServices(configureDelegate)
                                        .Build();

            return host.Services.GetRequiredService<IServiceProvider>();
        }
    }
}
