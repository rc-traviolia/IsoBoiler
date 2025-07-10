//using Moq;
//using Moq.Protected;
//using System.Net;

//namespace IsoBoiler.Testing.HTTP
//{


//    public class HttpClientFactoryMother
//    {
//        private Mock<IHttpClientFactory> httpClientFactoryMock { get; set; }
//        //Call the static method Build() instead to create a new instance.
//        private HttpClientFactoryMother()
//        {
//            httpClientFactoryMock = new Mock<IHttpClientFactory>();
//        }

//        public static HttpClientFactoryMother Birth()
//        {
//            return new HttpClientFactoryMother();
//        }

//        public HttpClientFactoryMother With(string clientName, HttpClient dependency)
//        {
//            httpClientFactoryMock.Setup(m => m.CreateClient(clientName)).Returns(dependency);
//            return this;
//        }

//        public PendingHttpClient With(string clientName)
//        {
//            //Create a PendingHttpClient to allow Setup Methodes to be chained
//            return new PendingHttpClient(this, clientName);
//        }

//        public IHttpClientFactory GetObject() => httpClientFactoryMock.Object;
//    }

//    public class PendingHttpClient
//    {
//        public HttpClientFactoryMother HttpClientFactoryMotherReference { get; set; }
//        private string _clientName { get; set; }
//        private Mock<HttpMessageHandler> httpMessageHandlerMock { get; set; }

//        public PendingHttpClient(HttpClientFactoryMother httpClientFactoryMotherReference, string clientName)
//        {
//            HttpClientFactoryMotherReference = httpClientFactoryMotherReference;
//            _clientName = clientName;
//            httpMessageHandlerMock = new Mock<HttpMessageHandler>();
//        }

//        public HttpClient GetDefault(string name)
//        {
//            var handlerMock = new Mock<HttpMessageHandler>();
//            handlerMock.Protected()
//                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
//                       .ReturnsAsync(new HttpResponseMessage
//                       {
//                           StatusCode = HttpStatusCode.OK,
//                           Content = new StringContent("{}")
//                       });

//            var client = new HttpClient(handlerMock.Object)
//            {
//                BaseAddress = new Uri("https://localhost")
//            };

//            return client;
//        }

//        public HttpClientFactoryMother With(string clientName)
//        {
//            //Add the final HttpClient by using With() on the HttpClientFactoryMother and return that reference
//            //So we can move back and forth between ObjectMother and PendingHttpClient
//            return HttpClientFactoryMotherReference.With(_clientName, new HttpClient(httpMessageHandlerMock.Object));
//        }

//        //public PendingDependencyOverride<TServiceToTest, TNewDependency> With<TNewDependency>() where TNewDependency : class
//        //{
//        //    //When beginning a new PendingDependencyOverride, we need to make sure this one was added to the ObjectMotherReference
//        //    return new PendingDependencyOverride<TServiceToTest, TNewDependency>(HttpClientFactoryMotherReference.With(HttpMessageHandlerMock.Object));
//        //}

//        ////This should be the final call to get the testable object
//        //public TServiceToTest GetObject() => HttpClientFactoryMotherReference.With(HttpMessageHandlerMock.Object).GetObject();


//        ////
//        //public PendingDependencyOverride<TServiceToTest, TDependency> Setup(Action<Mock<TDependency>> configure)
//        //{
//        //    configure(HttpMessageHandlerMock);
//        //    return this;
//        //}
//    }
//}