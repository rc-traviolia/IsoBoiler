using IsoBoiler.Logging;
using IsoBoiler.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;

namespace IsoBoiler.UnitTests.Helpers
{
    public static class ExampleDefaultServiceProviderBuilder
    {
        public static IServiceProvider GetServiceProvider()
        {
            return ConfigHelper.GetServiceProvider((context, services) =>
            {

                services.AddSingleton(Mock.Of<ILog>());
                services.AddSingleton(Mock.Of<HealthCheckService>());

            });
        }
    }
}

