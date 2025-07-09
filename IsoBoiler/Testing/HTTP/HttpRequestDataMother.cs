using IsoBoiler.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Security.Claims;

namespace IsoBoiler.Testing.HTTP
{
    public class HttpRequestDataMother
    {
        private string _method;
        private TestHttpRequestData httpRequestData;
        private HttpHeadersCollection headers;
        private HttpRequestDataMother(string method)
        {
            _method = method;
            headers = new HttpHeadersCollection();
            httpRequestData = new TestHttpRequestData(method, headers);
        }

        public static HttpRequestDataMother Birth(string method = "GET")
        {
            return new HttpRequestDataMother(method);
        }

        public HttpRequestDataMother AddBody(string jsonBodyValue)
        {
            if(_method == "GET")
            {
                throw new InvalidOperationException("You cannot include a Body in a GET request. Please change the method or do not try to use .AddBody()");
            }

            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(jsonBodyValue);
            streamWriter.Flush();
            memoryStream.Position = 0;
            httpRequestData = new TestHttpRequestData(_method, headers, memoryStream);

            return this;
        }

        public HttpRequestDataMother AddBody<TPostBodyModel>(TPostBodyModel postBodyModel)
        {
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(postBodyModel.ToJson());
            streamWriter.Flush();
            memoryStream.Position = 0;
            httpRequestData = new TestHttpRequestData(_method, headers, memoryStream);

            return this;

        }

        public HttpRequestDataMother AddBody<TPostBodyModel>() where TPostBodyModel : class, new()
        {
            var postBodyModel = new TPostBodyModel();
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(postBodyModel.ToJson());
            streamWriter.Flush();
            memoryStream.Position = 0;
            httpRequestData = new TestHttpRequestData(_method, headers, memoryStream);

            return this;

        }

        public HttpRequestDataMother AddHeader(string key, string value)
        {
            //Add to the existing collection and to the private field, so that changes are persisted between new TestHttpRequestData
            //every time it a new one is constructed, this reference is used
            headers.Add(key, value);    
            return this;
        }

        public TestHttpRequestData GetObject()
        {
            return httpRequestData;
        }
    }

    public class TestHttpRequestData : HttpRequestData
    {
        private readonly Stream? _body = null;
        private string _method = "GET";
        private static FunctionContext _functionContext = Mock.Of<FunctionContext>();

        public TestHttpRequestData(string method = "GET", HttpHeadersCollection? headers = null) : base(_functionContext)
        {
            _method = method;
            Headers = headers ?? new HttpHeadersCollection();

            var defaultServiceProvider = ConfigHelper.GetDefaultServiceProvider();
            var mockFunctionContext = new Mock<FunctionContext>();
            mockFunctionContext.Setup(mfc => mfc.InstanceServices).Returns(defaultServiceProvider);

            _functionContext = mockFunctionContext.Object;
        }
        public TestHttpRequestData(string method, HttpHeadersCollection headers, Stream body) : base(_functionContext)
        {
            _method = method;
            _body = body;
            Headers = headers;

            var defaultServiceProvider = ConfigHelper.GetDefaultServiceProvider();
            var mockFunctionContext = new Mock<FunctionContext>();
            mockFunctionContext.Setup(mfc => mfc.InstanceServices).Returns(defaultServiceProvider);

            _functionContext = mockFunctionContext.Object;
        }

        public override Stream Body => _body!;

        public override HttpHeadersCollection Headers { get; } // [];// (HttpHeadersCollection)Activator.CreateInstance(typeof(HttpHeadersCollection), nonPublic: true)!;

        public override IReadOnlyCollection<IHttpCookie> Cookies => Array.Empty<IHttpCookie>();

        public override Uri Url => new Uri("https://localhost/test");

        public override IEnumerable<ClaimsIdentity> Identities => Array.Empty<ClaimsIdentity>();

        public override string Method => _method;


        public override HttpResponseData CreateResponse() => new TestHttpResponseData(_functionContext);
    }

    public class TestHttpResponseData : HttpResponseData
    {
        public TestHttpResponseData(FunctionContext functionContext) : base(functionContext)
        {
            Body = new MemoryStream();
            Headers = (HttpHeadersCollection)Activator.CreateInstance(typeof(HttpHeadersCollection), nonPublic: true)!;
        }

        public override HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public override HttpHeadersCollection Headers { get; set; }
        public override Stream Body { get; set; }
        public override HttpCookies Cookies { get; } = new TestHttpCookies();
    }

    public class TestHttpCookies : HttpCookies
    {
        private readonly Dictionary<string, HttpCookie> _cookies = new();

        public override void Append(string name, string value)
        {
            throw new NotImplementedException();
        }

        public override void Append(IHttpCookie cookie)
        {
            throw new NotImplementedException();
        }

        public override IHttpCookie CreateNew()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<HttpCookie> GetCookies() => _cookies.Values.ToList();
    }
}
