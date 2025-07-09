using IsoBoiler.Json;
using IsoBoiler.Logging;
using IsoBoiler.Streams;
using IsoBoiler.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IsoBoiler.Tests.Helpers
{
    public class ExampleFunction
    {
        private readonly ILogBoiler _logger;
        private readonly HealthCheckService _healthCheck;

        public ExampleFunction(ILogBoiler logger, HealthCheckService healthCheck)
        {
            _logger = logger;
            _healthCheck = healthCheck;
        }

        [Function("health")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData httpRequestData, FunctionContext functionContext)
        {
            using (_logger.BeginScope(functionContext))
            {
                var someHeaderValueOne = httpRequestData.GetHeaderValue("someHeaderValueOne");
                var someHeaderValueTwo = httpRequestData.GetHeaderValue("someHeaderValueTwo");
                var requestBody = await httpRequestData.Body.ReadStreamAsync();
                var requestObject = requestBody.ToObject<ExampleRequest>();

                var healthStatus = await _healthCheck.CheckHealthAsync();
                var response = httpRequestData.CreateResponse();
                await response.WriteAsJsonAsync(healthStatus);
                return response;
            }

        }
    }
}
