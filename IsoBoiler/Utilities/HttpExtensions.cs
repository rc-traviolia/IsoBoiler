using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace IsoBoiler.Utilities
{
    public static class HttpExtensions
    {
        public static string GetHeaderValue(this HttpRequestData httpRequestData, string headerKey)
        {
            IEnumerable<string>? headerValues;
            httpRequestData.Headers.TryGetValues(headerKey, out headerValues);
            if (headerValues?.Count() == 1)
            {
                return headerValues.First();
            }
            else
            {
                throw new Exception($"Missing/Invalid '{headerKey}' Header");
            }
        }

        public static async Task<HttpResponseData> CreateResponse<TJsonSerializableClass>(this HttpRequestData requestData, TJsonSerializableClass jsonSerializableClass, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var response = requestData.CreateResponse();
            await response.WriteAsJsonAsync(jsonSerializableClass);
            response.StatusCode = statusCode;
            return response;
        }
        public static async Task<HttpResponseData> CreateResponse(this HttpRequestData requestData, Exception exception, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            var response = requestData.CreateResponse();
            await response.WriteStringAsync(exception.Message);
            response.StatusCode = statusCode;
            return response;
        }
        public static async Task<HttpResponseData> CreateResponse<TJsonSerializableClass>(this HttpRequestData requestData, TJsonSerializableClass jsonSerializableClass, HttpStatusCode? statusCode = null)
        {
            var response = requestData.CreateResponse();

            if (jsonSerializableClass is Exception)
            {
                var exception = jsonSerializableClass as Exception;
                await response.WriteStringAsync(exception!.Message);
                response.StatusCode = statusCode ?? HttpStatusCode.InternalServerError;
            }
            else
            {
                await response.WriteAsJsonAsync(jsonSerializableClass);
                response.StatusCode = statusCode ?? HttpStatusCode.OK;
            }

            return response;
        }
    }
}
