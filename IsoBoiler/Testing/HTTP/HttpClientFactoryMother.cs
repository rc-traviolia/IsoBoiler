using Moq;

namespace IsoBoiler.Testing.HTTP
{


    public class HttpClientFactoryMother
    {
        private Mock<IHttpClientFactory> httpClientFactoryMock { get; set; }
        //Call the static method Build() instead to create a new instance.
        private HttpClientFactoryMother()
        {
            httpClientFactoryMock = new Mock<IHttpClientFactory>();
        }

        public static HttpClientFactoryMother Birth()
        {
            return new HttpClientFactoryMother();
        }

        public HttpClientFactoryMother With(string clientName, HttpClient dependency)
        {
            httpClientFactoryMock.Setup(m => m.CreateClient(clientName)).Returns(dependency);
            return this;
        }

        public HttpClientFactoryMother With(string clientName, Action<HttpClientMother> configure)
        {
            var newClient = HttpClientMother.Birth();
            configure(newClient);
            return this.With(clientName, newClient.GetObject());
        }

        public HttpClientFactoryMother With(string clientName, Func<HttpClient> httpClientCreationFunction)
        {
            httpClientFactoryMock.Setup(m => m.CreateClient(clientName)).Returns(httpClientCreationFunction());
            return this;
        }

        public IHttpClientFactory GetObject() => httpClientFactoryMock.Object;
    }
}