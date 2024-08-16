using IsoBoiler.Logging;
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
        public static StoredConfigurationFilter UseConfigurationFilter(string configurationFilter)
        {
            return new StoredConfigurationFilter() { Value = configurationFilter };
        }

        public static async Task RunWithServices(this StoredConfigurationFilter configurationFilterObject, Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AppConfigurationConnectionString")))
            {
                throw new InvalidOperationException("You must have an Environment Variable named: 'AppConfigurationConnectionString' in order to use AddInitialConfiguration().");
            }

            var host = new HostBuilder().ConfigureFunctionsWorkerDefaults()
                                        .AddDefaultJsonSerializerOptions()
                                        .AddApplicationInsights()
                                        .AddLogBoiler()
                                        .AddConfiguration(configurationFilterObject.Value)
                                        .ConfigureServices(configureDelegate)
                                        .Build();

            await host.RunAsync();
        }

        public static async Task RunWithServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AppConfigurationConnectionString")))
            {
                throw new InvalidOperationException("You must have an Environment Variable named: 'AppConfigurationConnectionString' in order to use AddInitialConfiguration().");
            }

            var host = new HostBuilder().ConfigureFunctionsWorkerDefaults()
                                        .AddDefaultJsonSerializerOptions()
                                        .AddApplicationInsights()
                                        .AddLogBoiler()
                                        .AddConfiguration()
                                        .ConfigureServices(configureDelegate)
                                        .Build();

            await host.RunAsync();
        }

        public static async Task RunBasicWithServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {

            var host = new HostBuilder().ConfigureFunctionsWorkerDefaults()
                                        .AddDefaultJsonSerializerOptions()
                                        .AddApplicationInsights()
                                        .AddLogBoiler()
                                        .ConfigureServices(configureDelegate)
                                        .Build();

            await host.RunAsync();
        }
    }

    public class StoredConfigurationFilter
    {
        public string Value { get; set; }
    }
}
