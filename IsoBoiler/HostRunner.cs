using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IsoBoiler
{
    public static class HostRunner
    {

        /// <summary>
        /// If you do not apply a ConfigurationFilter then your snapshot will include <strong>everything</strong> on the configuration resource,
        /// instead of just the section for your applicaiton.
        /// </summary>
        /// <param name="configurationFilter"></param>
        /// <returns></returns>
        public static IsoBoilerOptions UseConfigurationFilter(string configurationFilter)
        {
            return new IsoBoilerOptions() { ConfigurationFilter = configurationFilter };
        }
        public static IsoBoilerOptions UseConfigurationFilter(this IsoBoilerOptions optionsToExtend, string configurationFilter)
        {
            optionsToExtend.ConfigurationFilter = configurationFilter;
            return optionsToExtend;
        }

        public static IsoBoilerOptions UseConfigurationSnapshot(string snapshotName)
        {
            return new IsoBoilerOptions() { ConfigurationSnapshot = snapshotName };
        }
        public static IsoBoilerOptions UseConfigurationSnapshot(this IsoBoilerOptions optionsToExtend, string snapshotName)
        {
            optionsToExtend.ConfigurationSnapshot = snapshotName;
            return optionsToExtend;
        }

        //public static IsoBoilerOptions UseOpenApi()
        //{
        //    return new IsoBoilerOptions() { UseOpenApi = true };
        //}


        public static async Task RunWithServices(this IsoBoilerOptions storedConfigurationOptions, Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Constants.AzureAppConfigurationPrimaryEndpoint)))
            {
                throw new InvalidOperationException($"You must have an Environment Variable named: '{Constants.AzureAppConfigurationPrimaryEndpoint}' in order to use RunWithServices(). 'AppConfigurationConnectionString' has been deprecated.");
            }

            var host = new HostBuilder().AddDefaultJsonSerializerOptions()
                                        .AddApplicationInsights()
                                        .AddLogBoiler()
                                        .AddConfiguration(storedConfigurationOptions)
                                        .ConfigureServices(configureDelegate)
                                        .Build();

            await host.RunAsync();
        }

        public static async Task RunWithServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Constants.AzureAppConfigurationPrimaryEndpoint)))
            {
                throw new InvalidOperationException($"You must have an Environment Variable named: '{Constants.AzureAppConfigurationPrimaryEndpoint}' in order to use RunWithServices(). 'AppConfigurationConnectionString' has been deprecated.");
            }

            var host = new HostBuilder().AddDefaultJsonSerializerOptions()
                                        .AddApplicationInsights()
                                        .AddLogBoiler()
                                        .AddConfiguration()
                                        .ConfigureServices(configureDelegate)
                                        .Build();

            await host.RunAsync();
        }

        public static async Task RunBasicWithServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {

            var host = new HostBuilder().ConfigureFunctionsWorkerDefaults() //Is inside AddConfiguration() as well. Changes here need to be made there.
                                        .AddDefaultJsonSerializerOptions()
                                        .AddApplicationInsights()
                                        .AddLogBoiler()
                                        .ConfigureServices(configureDelegate)
                                        .Build();

            await host.RunAsync();
        }
    }

    public class IsoBoilerOptions
    {
        public string ConfigurationFilter { get; set; } = string.Empty;
        public string ConfigurationSnapshot { get; set; } = string.Empty;
    }

}
