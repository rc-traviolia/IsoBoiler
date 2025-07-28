using Azure.Core.Serialization;
using Azure.Identity;
using IsoBoiler.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using System.Reflection;
using System.Text.Json;

namespace IsoBoiler
{
    public static class HostRunnerExtensions
    {
        /// <summary>
        /// If you do not apply a ConfigurationFilter then your snapshot will include <strong>everything</strong> on the configuration resource,
        /// instead of just the section for your applicaiton.
        /// </summary>
        /// <param name="configurationFilter"></param>
        /// <returns></returns>
        public static IsoBoilerOptions UseConfigurationFilter(this IsoBoilerOptions storedConfigurationOptions, string configurationFilter)
        {
            if (!string.IsNullOrWhiteSpace(storedConfigurationOptions.ConfigurationFilter))
            {
                throw new InvalidOperationException("A Filter value is already set for your StoredConfigurationOptions");
            }

            storedConfigurationOptions.ConfigurationFilter = configurationFilter;

            return storedConfigurationOptions;
        }

        public static IsoBoilerOptions UseConfigurationSnapshot(this IsoBoilerOptions storedConfigurationOptions, string snapshotName)
        {
            if (!string.IsNullOrWhiteSpace(storedConfigurationOptions.ConfigurationSnapshot))
            {
                throw new InvalidOperationException("A Snapshot value is already set for your StoredConfigurationOptions");
            }

            storedConfigurationOptions.ConfigurationSnapshot = snapshotName;

            return storedConfigurationOptions;
        }

        //public static IsoBoilerOptions UseOpenApi(this IsoBoilerOptions optionsToExtend)
        //{
        //    optionsToExtend.UseOpenApi = true;
        //    return optionsToExtend;
        //}

        public static IHostBuilder AddConfiguration(this IHostBuilder iHostBuilder, IsoBoilerOptions? storedConfigurationOptions = null)
        {
            var appConfigEndpoint = Environment.GetEnvironmentVariable(Constants.AzureAppConfigurationPrimaryEndpoint);

            if (string.IsNullOrWhiteSpace(appConfigEndpoint))
            {
                throw new InvalidOperationException($"You must have an Environment Variable named: '{Constants.AzureAppConfigurationPrimaryEndpoint}' in order to use RunWithServices(). 'AppConfigurationConnectionString' has been deprecated.");
            }

            var configurationFilter = string.Empty;
            var configurationSnapshot = string.Empty;

            if (storedConfigurationOptions is not null)
            {
                configurationFilter = storedConfigurationOptions.ConfigurationFilter;
                configurationSnapshot = storedConfigurationOptions.ConfigurationSnapshot;
            }

            //Default Filter
            if (string.IsNullOrWhiteSpace(configurationFilter))
            {
                configurationFilter = Assembly.GetEntryAssembly()!.GetName()!.Name!.Contains('.') ? Assembly.GetEntryAssembly()!.GetName()!.Name!.Split('.').Last() : Assembly.GetEntryAssembly()!.GetName()!.Name!;
                if (string.IsNullOrWhiteSpace(configurationFilter))
                {
                    throw new InvalidOperationException("There was an error automatically creating your configurationFilter for the Azure App Configuration. Please provide one by using HostRunner.UseConfigurationFilter(string configurationFilter)");
                }
            }

            //Don't use configurationSnapshot if there isn't one
            if (string.IsNullOrWhiteSpace(configurationSnapshot))
            {
                iHostBuilder.ConfigureAppConfiguration((context, builder) =>
                {

                    builder.AddAzureAppConfiguration(options =>
                    {
                        options.Connect(new Uri(appConfigEndpoint), new DefaultAzureCredential())
                               .Select($"{configurationFilter}:*")
                               .ConfigureKeyVault(kv => { kv.SetCredential(new DefaultAzureCredential()); })
                               .ConfigureRefresh(refreshOptions =>
                               {
                                   refreshOptions.Register($"{configurationFilter}:Sentinel", refreshAll: true)
                                                 .SetRefreshInterval(TimeSpan.FromSeconds(30)); //Default value is 30
                               });
                    });
                })
                .ConfigureServices(services =>
                {
                    //This is required for the IOptionsSnapshots to actually refresh
                    //https://github.com/Azure/AppConfiguration/blob/main/examples/DotNetCore/AzureFunction/FunctionAppIsolatedMode/Program.cs
                    services.AddAzureAppConfiguration()
                            .AddFeatureManagement();
                })
                .ConfigureFunctionsWorkerDefaults(app =>
                {
                    ///For Config Refresh
                    app.UseAzureAppConfiguration();
                });

                return iHostBuilder;
            }
            else
            {
                iHostBuilder.ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddAzureAppConfiguration(options =>
                    {
                        options.Connect(new Uri(appConfigEndpoint), new DefaultAzureCredential())
                               .Select($"{configurationFilter}:*")
                               .SelectSnapshot(configurationSnapshot)
                               .ConfigureKeyVault(kv => { kv.SetCredential(new DefaultAzureCredential()); })
                               .ConfigureRefresh(refreshOptions =>
                               {
                                   refreshOptions.Register($"{configurationFilter}:Sentinel", refreshAll: true)
                                                 .SetRefreshInterval(TimeSpan.FromSeconds(30)); //Default value is 30
                               });
                    });
                })
                .ConfigureServices(services =>
                {
                    //This is required for the IOptionsSnapshots to actually refresh
                    //https://github.com/Azure/AppConfiguration/blob/main/examples/DotNetCore/AzureFunction/FunctionAppIsolatedMode/Program.cs
                    services.AddAzureAppConfiguration()
                            .AddFeatureManagement();
                })
                .ConfigureFunctionsWorkerDefaults(app =>
                {
                    ///For Config Refresh
                    app.UseAzureAppConfiguration();
                });

                return iHostBuilder;
            }
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
                    options.PropertyNameCaseInsensitive = true;
                    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
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
        public static TConfiguration GetConfig<TConfiguration>(this IServiceCollection services)
        {
            var service = services.BuildServiceProvider().GetService<TConfiguration>();
            if (service == null)
            {
                throw new InvalidOperationException($"Service type was not found in the IServicesCollection");
            }

            return service;
        }

    }
}

