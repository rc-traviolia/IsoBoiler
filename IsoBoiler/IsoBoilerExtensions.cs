using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IsoBoiler
{
    public static class IsoBoilerExtensions
    {
        public static IHostBuilder AddInitialConfiguration(this IHostBuilder iHostBuilder)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AppConfigurationConnectionString")))
            {
                throw new InvalidOperationException("You must have an Environment Variable named: 'AppConfigurationConnectionString' in order to use AddInitialConfiguration().");
            }

            iHostBuilder.ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddAzureAppConfiguration(options =>
                {
                    options.Connect(Environment.GetEnvironmentVariable("AppConfigurationConnectionString"))
                        .ConfigureKeyVault(kv => { kv.SetCredential(new DefaultAzureCredential()); });
                });
            })
            .ConfigureFunctionsWorkerDefaults(builder =>
            {
                builder.AddApplicationInsights().AddApplicationInsightsLogger();
            });
            return iHostBuilder;
        }

        public static IHostBuilder AddInitialConfiguration(this IHostBuilder iHostBuilder, Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AppConfigurationConnectionString")))
            {
                throw new InvalidOperationException("You must have an Environment Variable named: 'AppConfigurationConnectionString' in order to use AddInitialConfiguration().");
            }

            iHostBuilder.ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddAzureAppConfiguration(options =>
                {
                    options.Connect(Environment.GetEnvironmentVariable("AppConfigurationConnectionString"))
                        .ConfigureKeyVault(kv => { kv.SetCredential(new DefaultAzureCredential()); });
                });
            })
            .ConfigureFunctionsWorkerDefaults(builder =>
            {
                builder.AddApplicationInsights().AddApplicationInsightsLogger();
            })
            .ConfigureServices(configureDelegate);

            return iHostBuilder;
        }

        public static TServiceType GetService<TServiceType>(this IServiceCollection services)
        {
            var service = services.BuildServiceProvider().GetService<TServiceType>();
            if(service == null)
            {
                throw new InvalidOperationException($"Service type was not found in the IServicesCollection");
            }

            return service;
        }

    }
}

