using IsoBoiler.Json;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using Moq.Protected;
using System.Diagnostics;
using System.Net;
using System.Net.Http;

namespace IsoBoiler.Testing.HTTP
{

    public class HttpClientMother
    {
        private Mock<HttpMessageHandler> httpMessageHandlerMock;
        private PendingHttpResponseMessage singlePendingHttpResponseMessage;
        private List<PendingHttpResponseMessage> multiplePendingHttpResponseMessages;
        private HttpHeadersCollection globalHeaders;
        private bool? isUsingSingleResponse;
        private bool Return404ForUnmappedRoutes = true;

        private HttpClientMother()
        {
            httpMessageHandlerMock = new();
            singlePendingHttpResponseMessage = new(ItExpr.IsAny<HttpRequestMessage>());
            multiplePendingHttpResponseMessages = new();
            globalHeaders = new();
        }

        public static HttpClientMother Birth()
        {
            return new HttpClientMother();
        }

        /// <summary>
        /// Adds a response header to ALL responses for this HttpClient.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpClientMother AddResponseHeader(string key, string value)
        {
            globalHeaders.Add(key, value);

            return this;
        }

        public HttpClientMother AddResponseHeaders(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            foreach (var kvp in keyValuePairs)
            {
                globalHeaders.Add(kvp.Key, kvp.Value);
            }

            return this;
        }

        public HttpClientMother AlwaysRespondWith(HttpStatusCode httpStatusCode, string? stringContent = null, IEnumerable<KeyValuePair<string, string>>? keyValuePairs = null)
        {
            if (isUsingSingleResponse is not null && !isUsingSingleResponse.Value)
            {
                throw new InvalidOperationException("You cannot use AlwaysRespondWith() when using AddSpecificResponse().");
            }
            isUsingSingleResponse = true;

            singlePendingHttpResponseMessage.StatusCode = httpStatusCode;
            singlePendingHttpResponseMessage.Content = new StringContent(stringContent ?? "{}");
            foreach (var header in keyValuePairs ?? new List<KeyValuePair<string, string>>())
            {
                singlePendingHttpResponseMessage.Headers.Add(header.Key, header.Value);
            }

            return this;
        }

        public HttpClientMother AlwaysRespondWith<TPostBodyModel>(HttpStatusCode httpStatusCode, TPostBodyModel postBodyModel, IEnumerable<KeyValuePair<string, string>>? keyValuePairs = null)
        {
            if (isUsingSingleResponse is not null && !isUsingSingleResponse.Value)
            {
                throw new InvalidOperationException("You cannot use AlwaysRespondWith() when using AddSpecificResponse().");
            }
            isUsingSingleResponse = true;

            singlePendingHttpResponseMessage.StatusCode = httpStatusCode;
            singlePendingHttpResponseMessage.Content = new StringContent(postBodyModel.ToJson());
            foreach (var header in keyValuePairs ?? new List<KeyValuePair<string, string>>())
            {
                singlePendingHttpResponseMessage.Headers.Add(header.Key, header.Value);
            }

            return this;
        }

        public HttpClientMother DisableFallback404Reponse()
        {
            Return404ForUnmappedRoutes = false;
            return this;
        }

        public HttpClientMother AddSpecificResponse(string route, HttpMethod method, HttpStatusCode responseCode, string responseBody = "", IEnumerable<KeyValuePair<string, string>>? headers = null)
        {
            return AddSpecificResponse(route, method, "", responseCode, responseBody, headers);
        }
        public HttpClientMother AddSpecificResponse(string route, HttpMethod method, string body, HttpStatusCode responseCode, string responseBody, IEnumerable<KeyValuePair<string, string>>? headers = null)
        {
            if (isUsingSingleResponse is not null && isUsingSingleResponse.Value)
            {
                throw new InvalidOperationException("You cannot use AddSpecificResponse() when using AlwaysRespondWith().");
            }
            isUsingSingleResponse = false;

            var pendingHttpResponseMessage = new PendingHttpResponseMessage();

            if (!string.IsNullOrEmpty(body))
            {
                pendingHttpResponseMessage.RequestMessageItExpr = ItExpr.Is<HttpRequestMessage>(hrm => hrm.RequestUri!.AbsolutePath.EndsWith($"/{route}") == true &&
                                                                                                        hrm.Method == method &&
                                                                                                        hrm.Content != null &&
                                                                                                        hrm.Content.ReadAsStringAsync().Result == body);
            }
            else
            {
                pendingHttpResponseMessage.RequestMessageItExpr = ItExpr.Is<HttpRequestMessage>(hrm => hrm.RequestUri!.AbsolutePath.EndsWith($"/{route}") == true &&
                                                                                                        hrm.Method == method);
            }

            pendingHttpResponseMessage.StatusCode = responseCode;
            pendingHttpResponseMessage.Content = new StringContent(responseBody);
            foreach (var header in headers ?? new List<KeyValuePair<string, string>>())
            {
                pendingHttpResponseMessage.Headers.Add(header.Key, header.Value);
            }

            multiplePendingHttpResponseMessages.Add(pendingHttpResponseMessage);

            return this;
        }

        public HttpClient GetObject()
        {
            if (isUsingSingleResponse is null)
            {
                throw new InvalidOperationException("You must call AlwaysRespondWith() or AddSpecificResponse() before calling GetObject().");
            }

            if (isUsingSingleResponse.Value)
            {
                var httpResponseMessage = new HttpResponseMessage
                {
                    StatusCode = singlePendingHttpResponseMessage.StatusCode,
                    Content = singlePendingHttpResponseMessage.Content
                };

                foreach (var header in singlePendingHttpResponseMessage.Headers)
                {
                    httpResponseMessage.Headers.Add(header.Key, header.Value);
                }

                foreach (var header in globalHeaders)
                {
                    httpResponseMessage.Headers.Add(header.Key, header.Value);
                }

                httpMessageHandlerMock.Protected()
                                       .Setup<Task<HttpResponseMessage>>("SendAsync", singlePendingHttpResponseMessage.RequestMessageItExpr, ItExpr.IsAny<CancellationToken>())
                                       .ReturnsAsync(httpResponseMessage);

                return new HttpClient(httpMessageHandlerMock.Object)
                {
                    BaseAddress = new Uri("https://localhost")
                };
            }
            else
            {
                //Add default 400 response for all other requests that are not matched
                //MUST ADD FIRST. ADDING AFTER ANOTHER SETUP WILL OVERRIDE IT IF THE ItExpr MATCHES
                if (Return404ForUnmappedRoutes)
                {
                    var default404Message = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Content = new StringContent("Not Found")
                    };

                    foreach (var header in globalHeaders)
                    {
                        default404Message.Headers.Add(header.Key, header.Value);
                    }

                    httpMessageHandlerMock.Protected()
                                           .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                                           .ReturnsAsync(default404Message);
                }
               

                foreach (var pendingHttpResponseMessage in multiplePendingHttpResponseMessages)
                {
                    var httpResponseMessage = new HttpResponseMessage
                    {
                        StatusCode = pendingHttpResponseMessage.StatusCode,
                        Content = pendingHttpResponseMessage.Content
                    };

                    foreach (var header in pendingHttpResponseMessage.Headers)
                    {
                        httpResponseMessage.Headers.Add(header.Key, header.Value);
                    }

                    foreach (var header in globalHeaders)
                    {
                        httpResponseMessage.Headers.Add(header.Key, header.Value);
                    }

                    httpMessageHandlerMock.Protected()
                                           .Setup<Task<HttpResponseMessage>>("SendAsync", pendingHttpResponseMessage.RequestMessageItExpr, ItExpr.IsAny<CancellationToken>())
                                           .ReturnsAsync(httpResponseMessage);
                }



                return new HttpClient(httpMessageHandlerMock.Object)
                {
                    BaseAddress = new Uri("https://localhost")
                };
            }
        }
    }

    public class PendingHttpResponseMessage
    {
        public object RequestMessageItExpr;
        public HttpHeadersCollection Headers;
        public HttpStatusCode StatusCode;
        public StringContent Content;
        public PendingHttpResponseMessage(object? httpRequestMessageItExpr = null)
        {
            RequestMessageItExpr = httpRequestMessageItExpr ?? ItExpr.IsAny<HttpRequestMessage>();
            Headers = new HttpHeadersCollection();
            StatusCode = HttpStatusCode.NotImplemented;
            Content = new StringContent("{}");
        }

    }
}
