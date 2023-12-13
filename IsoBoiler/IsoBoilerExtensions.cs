using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace IsoBoiler
{
    public static class IsoBoilerExtensions
    {
        public static IHostBuilder AddBasicConfiguration(this IHostBuilder iHostBuilder)
        {
            iHostBuilder.ConfigureFunctionsWorkerDefaults(builder =>
            {
                builder.AddApplicationInsights().AddApplicationInsightsLogger();
            });
            return iHostBuilder;
        }
        public static IHostBuilder AddInitialConfiguration(this IHostBuilder iHostBuilder, string configurationFilter = "")
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AppConfigurationConnectionString")))
            {
                throw new InvalidOperationException("You must have an Environment Variable named: 'AppConfigurationConnectionString' in order to use AddInitialConfiguration().");
            }

            if (string.IsNullOrWhiteSpace(configurationFilter))
            {
                configurationFilter = Assembly.GetEntryAssembly().GetName().Name.Contains('.') ? Assembly.GetEntryAssembly().GetName().Name.Split('.').Last() : Assembly.GetEntryAssembly().GetName().Name;
                if (string.IsNullOrWhiteSpace(configurationFilter))
                {
                    throw new InvalidOperationException("There was an error automatically creating your configurationFilter for the Azure App Configuration. Please provide one");
                }
            }

            iHostBuilder.ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddAzureAppConfiguration(options =>
                {
                    options.Connect(Environment.GetEnvironmentVariable("AppConfigurationConnectionString"))
                           .Select($"{configurationFilter}:*")
                           .ConfigureKeyVault(kv => { kv.SetCredential(new DefaultAzureCredential()); });
                });
            })
            .ConfigureFunctionsWorkerDefaults(builder =>
            {
                builder.AddApplicationInsights().AddApplicationInsightsLogger();
            });
            return iHostBuilder;
        }

        public static IHostBuilder AddInitialConfiguration(this IHostBuilder iHostBuilder, Action<HostBuilderContext, IServiceCollection> configureDelegate, string configurationFilter = "")
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AppConfigurationConnectionString")))
            {
                throw new InvalidOperationException("There was an error automatically creating your configurationFilter for the Azure App Configuration. Please provide one by using HostRunner.UseConfigurationFilter() or the appropriate .AddInitialConfiguration() parameter.");
            }

            if (string.IsNullOrWhiteSpace(configurationFilter))
            {
                configurationFilter = Assembly.GetEntryAssembly().GetName().Name.Contains('.') ? Assembly.GetEntryAssembly().GetName().Name.Split('.').Last() : Assembly.GetEntryAssembly().GetName().Name;
                if (string.IsNullOrWhiteSpace(configurationFilter))
                {
                    throw new InvalidOperationException("There was an error automatically creating your configurationFilter for the Azure App Configuration. Please provide one by using HostRunner.UseConfigurationFilter() or the appropriate .AddInitialConfiguration() parameter.");
                }
            }

            iHostBuilder.ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddAzureAppConfiguration(options =>
                {
                    options.Connect(Environment.GetEnvironmentVariable("AppConfigurationConnectionString"))
                           .Select($"{configurationFilter}:*")
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

