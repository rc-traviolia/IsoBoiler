using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            var host = new HostBuilder().AddInitialConfiguration(configureDelegate, configurationFilterObject.Value)
            .Build();

            await host.RunAsync();
        }

        public static async Task RunWithServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AppConfigurationConnectionString")))
            {
                throw new InvalidOperationException("You must have an Environment Variable named: 'AppConfigurationConnectionString' in order to use AddInitialConfiguration().");
            }       

            var host = new HostBuilder().AddInitialConfiguration(configureDelegate)
            .Build();

            await host.RunAsync();
        }

        public static async Task RunBasicWithServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {

            var host = new HostBuilder().AddBasicConfiguration().ConfigureServices(configureDelegate)
            .Build();

            await host.RunAsync();
        }
    }

    public class StoredConfigurationFilter
    {
        public string Value { get; set; }
    }
}
