using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

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
            //.ConfigureOpenApi()
            .ConfigureFunctionsWorkerDefaults(builder =>
            {
                builder.AddApplicationInsights().AddApplicationInsightsLogger();
            });
            return iHostBuilder;
        }
    }
}

