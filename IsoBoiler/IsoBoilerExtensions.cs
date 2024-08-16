using Azure.Core.Serialization;
using Azure.Identity;
using IsoBoiler.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace IsoBoiler
{
    public static class IsoBoilerExtensions
    {
        public static IHostBuilder AddConfiguration(this IHostBuilder iHostBuilder, string configurationFilter = "")
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
                        });

            return iHostBuilder;
        }

        public static IHostBuilder AddApplicationInsights(this IHostBuilder iHostBuilder)
        {
            iHostBuilder.ConfigureServices(services =>
                        {
                            services.AddApplicationInsightsTelemetryWorkerService();
                            services.ConfigureFunctionsApplicationInsights();
                        })
                        .ConfigureLogging(logging =>
                        {
                            ///The Worker logger is configured separately from the Host logger, which is configured in the host.json file.
                            //Removing the default rule allows Information to be traced by the Worker process to Application Insights.
                            //Console logging is handled separately, and you will need to add "Function":"Information" to the host.json "logLevel" section
                            logging.Services.Configure<LoggerFilterOptions>(options =>
                            {
                                LoggerFilterRule? defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                                if (defaultRule is not null)
                                {
                                    options.Rules.Remove(defaultRule);
                                }
                            });
                        });

            return iHostBuilder;
        }

        public static IHostBuilder AddDefaultJsonSerializerOptions(this IHostBuilder iHostBuilder)
        {
            iHostBuilder.ConfigureServices(services =>
            {
                services.Configure<JsonSerializerOptions>(options =>
                {
                    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    //options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.WriteIndented = false;
                });
            });

            return iHostBuilder;
        }

        public static IHostBuilder AddLogBoiler(this IHostBuilder iHostBuilder)
        {
            iHostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<ILogBoiler, LogBoiler>();
            });

            return iHostBuilder;
        }

        public static TServiceType GetService<TServiceType>(this IServiceCollection services)
        {
            var service = services.BuildServiceProvider().GetService<TServiceType>();
            if (service == null)
            {
                throw new InvalidOperationException($"Service type was not found in the IServicesCollection");
            }

            return service;
        }

    }
}

