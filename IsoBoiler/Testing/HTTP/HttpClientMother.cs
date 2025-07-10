//using IsoBoiler.Json;
//using Microsoft.Azure.Functions.Worker.Http;
//using Moq;
//using Moq.Protected;
//using System.Net;

//namespace IsoBoiler.Testing.HTTP
//{

//    public class HttpClientMother
//    {
//        private Mock<HttpMessageHandler> httpMessageHandlerMock;
//        private HttpHeadersCollection headers;

//        private HttpClientMother()
//        {
//            httpMessageHandlerMock = new Mock<HttpMessageHandler>();
//        }

//        public static HttpClientMother Birth(string method = "GET")
//        {
//            return new HttpClientMother();
//        }

//        public HttpClientMother AddResponseBody(string jsonBodyValue)
//        {
//            if (_method == "GET")
//            {
//                throw new InvalidOperationException("You cannot include a Body in a GET request. Please change the method or do not try to use .AddBody()");
//            }

//            var memoryStream = new MemoryStream();
//            var streamWriter = new StreamWriter(memoryStream);
//            streamWriter.Write(jsonBodyValue);
//            streamWriter.Flush();
//            memoryStream.Position = 0;
//            httpRequestData = new TestHttpRequestData(_method, headers, memoryStream);

//            return this;
//        }

//        public HttpClientMother AddBody<TPostBodyModel>(TPostBodyModel postBodyModel)
//        {
//            var memoryStream = new MemoryStream();
//            var streamWriter = new StreamWriter(memoryStream);
//            streamWriter.Write(postBodyModel.ToJson());
//            streamWriter.Flush();
//            memoryStream.Position = 0;
//            httpRequestData = new TestHttpRequestData(_method, headers, memoryStream);

//            return this;

//        }

//        public HttpClientMother AddBody<TPostBodyModel>() where TPostBodyModel : class, new()
//        {
//            var postBodyModel = new TPostBodyModel();
//            var memoryStream = new MemoryStream();
//            var streamWriter = new StreamWriter(memoryStream);
//            streamWriter.Write(postBodyModel.ToJson());
//            streamWriter.Flush();
//            memoryStream.Position = 0;
//            httpRequestData = new TestHttpRequestData(_method, headers, memoryStream);

//            return this;

//        }

//        public HttpClientMother AddHeader(string key, string value)
//        {
//            //Add to the existing collection and to the private field, so that changes are persisted between new TestHttpRequestData
//            //every time it a new one is constructed, this reference is used
//            headers.Add(key, value);
//            return this;
//        }

//        public HttpClient GetObject()
//        {
//            return new HttpClient(httpMessageHandlerMock.Object);
//        }

//        public static HttpClient Birth(HttpStatusCode httpStatusCode = HttpStatusCode.OK, string? content = null)
//        {
//            var handlerMock = new Mock<HttpMessageHandler>();
//            handlerMock.Protected()
//                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
//                       .ReturnsAsync(new HttpResponseMessage
//                       {
//                           StatusCode = httpStatusCode,
//                           Content = new StringContent(content ?? "{}")
//                       });

//            var client = new HttpClient(handlerMock.Object)
//            {
//                BaseAddress = new Uri("https://localhost")
//            };

//            return client;
//        }
//    }
//}
