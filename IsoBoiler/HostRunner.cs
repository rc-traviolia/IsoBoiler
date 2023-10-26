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
        [Obsolete("Please use RunWithServices() instead.")]
        public static async Task RunHostWithServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AppConfigurationConnectionString")))
            {
                throw new InvalidOperationException("You must have an Environment Variable named: 'AppConfigurationConnectionString' in order to use AddInitialConfiguration().");
            }

            var host = new HostBuilder().AddInitialConfiguration(configureDelegate)
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
    }
}
