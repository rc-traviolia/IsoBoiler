using IsoBoiler.HTTP;
using IsoBoiler.Json;
using IsoBoiler.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IsoBoiler.UnitTests.Helpers
{
    public class ExampleFunction
    {
        private readonly ILog _logger;
        private readonly HealthCheckService _healthCheck;

        public ExampleFunction(ILog logger, HealthCheckService healthCheck)
        {
            _logger = logger;
            _healthCheck = healthCheck;
        }

        [Function("DefaultGetFunction")]
        public async Task<HttpResponseData> DefaultGetFunction([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData httpRequestData, FunctionContext functionContext)
        {
            using (_logger.BeginScope(functionContext))
            {
                var healthStatus = await _healthCheck.CheckHealthAsync();
                var response = httpRequestData.CreateResponse();
                await response.WriteAsJsonAsync(healthStatus);
                return response;
            }
        }

        [Function("GetFunction")]
        public async Task<HttpResponseData> GetFunction([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData httpRequestData, FunctionContext functionContext)
        {
            using (_logger.BeginScope(functionContext))
            {
                var someHeaderValueOne = httpRequestData.GetHeaderValue("someHeaderValueOne");
                var someHeaderValueTwo = httpRequestData.GetHeaderValue("someHeaderValueTwo");
                var someQueryParameterOne = httpRequestData.Query["someQueryParameter1"];
                var someQueryParameterTwo = httpRequestData.Query["someQueryParameter2"];

                var healthStatus = await _healthCheck.CheckHealthAsync();
                var response = httpRequestData.CreateResponse();
                await response.WriteAsJsonAsync(healthStatus);
                return response;
            }
        }

        [Function("PostFunction")]
        public async Task<HttpResponseData> PostFunction([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData httpRequestData, FunctionContext functionContext)
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
