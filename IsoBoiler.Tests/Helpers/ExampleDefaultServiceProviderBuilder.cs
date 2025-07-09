﻿using IsoBoiler.Logging;
using IsoBoiler.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;

namespace IsoBoiler.Tests.Helpers
{
    public static class ExampleDefaultServiceProviderBuilder
    {
        public static IServiceProvider GetServiceProvider()
        {
            return ConfigHelper.GetServiceProvider((context, services) =>
            {

                services.AddSingleton(Mock.Of<ILogBoiler>());
                services.AddSingleton(Mock.Of<HealthCheckService>());

            });
        }
    }
}

