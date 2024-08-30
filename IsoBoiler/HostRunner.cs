using IsoBoiler.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;

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
        public static StoredConfigurationOptions UseConfigurationFilter(string configurationFilter)
        {
            return new StoredConfigurationOptions() { Filter = configurationFilter };
        }

        public static StoredConfigurationOptions UseConfigurationSnapshot(string snapshotName)
        {
            return new StoredConfigurationOptions() { Snapshot = snapshotName };
        }        

        public static async Task RunWithServices(this StoredConfigurationOptions storedConfigurationOptions, Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Constants.APP_CONFIG_ENDPOINT)))
            {
                throw new InvalidOperationException($"You must have an Environment Variable named: '{Constants.APP_CONFIG_ENDPOINT}' in order to use RunWithServices(). 'AppConfigurationConnectionString' has been deprecated.");
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
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Constants.APP_CONFIG_ENDPOINT)))
            {
                throw new InvalidOperationException($"You must have an Environment Variable named: '{Constants.APP_CONFIG_ENDPOINT}' in order to use RunWithServices(). 'AppConfigurationConnectionString' has been deprecated.");
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

    public class StoredConfigurationOptions
    {
        public string Filter { get; set; } = string.Empty;
        public string Snapshot { get; set; } = string.Empty;
    }

}
