using IsoBoiler.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Diagnostics;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Security.Policy;

namespace IsoBoiler.Testing.HTTP
{
    public class HttpRequestDataMother
    {
        private HttpMethod _httpMethod;
        private HttpHeadersCollection headers;
        private MemoryStream memoryStream;
        private Dictionary<string, string> queryParameters;
        private TestHttpRequestData httpRequestData;
        private HttpRequestDataMother(HttpMethod httpMethod)
        {
            _httpMethod = httpMethod;
            headers = new HttpHeadersCollection();
            queryParameters = new Dictionary<string, string>();
            httpRequestData = new TestHttpRequestData(_httpMethod.ToString(), headers, queryParameters);
        }

        public static HttpRequestDataMother Birth()
        {
            return new HttpRequestDataMother(HttpMethod.Get);
        }

        public HttpRequestDataMother UseMethod(HttpMethod httpMethod)
        {
            _httpMethod = httpMethod;
            if (_httpMethod == HttpMethod.Get)
            {
                httpRequestData = new TestHttpRequestData(_httpMethod.ToString(), headers, queryParameters);
            }
            else
            {
                httpRequestData = new TestHttpRequestData(_httpMethod.ToString(), headers, memoryStream);
            }

            return this;
        }
        public HttpRequestDataMother AddBody(string jsonBodyValue)
        {
            if(_httpMethod == HttpMethod.Get)
            {
                throw new InvalidOperationException("You cannot include a Body in a GET request. Please change the method or do not try to use .AddBody()");
            }

            memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(jsonBodyValue);
            streamWriter.Flush();
            memoryStream.Position = 0;
            httpRequestData = new TestHttpRequestData(_httpMethod.ToString(), headers, memoryStream);

            return this;
        }

        public HttpRequestDataMother AddBody<TPostBodyModel>(TPostBodyModel postBodyModel)
        {
            if (_httpMethod == HttpMethod.Get)
            {
                throw new InvalidOperationException("You cannot include a Body in a GET request. Please change the method or do not try to use .AddBody()");
            }

            memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(postBodyModel.ToJson());
            streamWriter.Flush();
            memoryStream.Position = 0;
            httpRequestData = new TestHttpRequestData(_httpMethod.ToString(), headers, memoryStream);

            return this;

        }

        public HttpRequestDataMother AddBody<TPostBodyModel>() where TPostBodyModel : class, new()
        {
            if (_httpMethod == HttpMethod.Get)
            {
                throw new InvalidOperationException("You cannot include a Body in a GET request. Please change the method or do not try to use .AddBody()");
            }

            var postBodyModel = new TPostBodyModel();
            memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            streamWriter.Write(postBodyModel.ToJson());
            streamWriter.Flush();
            memoryStream.Position = 0;
            httpRequestData = new TestHttpRequestData(_httpMethod.ToString(), headers, memoryStream);

            return this;

        }

        public HttpRequestDataMother AddHeader(string key, string value)
        {
            //Add to the existing collection and to the private field, so that changes are persisted between new TestHttpRequestData
            //every time it a new one is constructed, this reference is used
            headers.Add(key, value);    
            return this;
        }

        public HttpRequestDataMother AddQueryParameter(string key, string value)
        {
            if (_httpMethod != HttpMethod.Get)
            {
                throw new InvalidOperationException("Query Parameters are only currently supported for GET requests.");
            }

            queryParameters.Add(key, value);
            httpRequestData = new TestHttpRequestData(_httpMethod.ToString(), headers, queryParameters);
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

        public TestHttpRequestData(string method, HttpHeadersCollection? headers, Dictionary<string,string> queryParameters) : base(_functionContext)
        {
            _method = method;
            Headers = headers ?? new HttpHeadersCollection();
            var urlString = queryParameters.Count() > 0 ? $"https://localhost/test?{string.Join("&", queryParameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"))}" : "https://localhost/test";
            _url = new Uri(urlString);

            var defaultServiceProvider = ConfigHelper.GetDefaultServiceProvider();
            var mockFunctionContext = new Mock<FunctionContext>();
            mockFunctionContext.Setup(mfc => mfc.InstanceServices).Returns(defaultServiceProvider);

            _functionContext = mockFunctionContext.Object;
        }
        public TestHttpRequestData(string method, HttpHeadersCollection? headers, Stream body) : base(_functionContext)
        {
            _method = method;
            _body = body;
            Headers = headers ?? new HttpHeadersCollection();

            var defaultServiceProvider = ConfigHelper.GetDefaultServiceProvider();
            var mockFunctionContext = new Mock<FunctionContext>();
            mockFunctionContext.Setup(mfc => mfc.InstanceServices).Returns(defaultServiceProvider);

            _functionContext = mockFunctionContext.Object;
        }

        public override Stream Body => _body!;

        public override HttpHeadersCollection Headers { get; } // [];// (HttpHeadersCollection)Activator.CreateInstance(typeof(HttpHeadersCollection), nonPublic: true)!;

        public override IReadOnlyCollection<IHttpCookie> Cookies => Array.Empty<IHttpCookie>();

        public override Uri Url => _url;
        private Uri _url;

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
