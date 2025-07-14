using AwesomeAssertions;
using IsoBoiler.Json;
using IsoBoiler.Testing.HTTP;
using IsoBoiler.Tests.Helpers;
using System;
using System.Net;

namespace IsoBoiler.Tests
{
    public class HttpClientMotherTests
    {
        [Fact]
        public async Task GetAsync_WithAlwaysRespondWith200_Returns200OK()
        {
            // Arrange
            var httpClient = HttpClientMother.Birth().AlwaysRespondWith(HttpStatusCode.OK).GetObject();

            //Act
            var response = await httpClient.GetAsync("someroute");

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task PostAsync_WithAlwaysRespondWith200_Returns200OK()
        {
            // Arrange
            var httpClient = HttpClientMother.Birth().AlwaysRespondWith(HttpStatusCode.OK).GetObject();
            var stringContent = new StringContent("data as a string");

            //Act
            var response = await httpClient.PostAsync("someroute", stringContent);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAsync_WithAlwaysRespondWith500_Returns500()
        {
            // Arrange
            var httpClient = HttpClientMother.Birth().AlwaysRespondWith(HttpStatusCode.InternalServerError).GetObject();

            //Act
            var response = await httpClient.GetAsync("someroute");

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task PostAsync_WithAlwaysRespondWith500_Returns500()
        {
            // Arrange
            var httpClient = HttpClientMother.Birth().AlwaysRespondWith(HttpStatusCode.InternalServerError).GetObject();
            var stringContent = new StringContent("data as a string");

            //Act
            var response = await httpClient.PostAsync("someroute", stringContent);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetAsync_WithAlwaysRespondWith200AndStringContent_ReturnsUnmodifiedContent()
        {
            // Arrange
            var content = new ExampleResponse();
            var stringContent = content.ToJson();
            var httpClient = HttpClientMother.Birth().AlwaysRespondWith(HttpStatusCode.OK, stringContent).GetObject();

            //Act
            var response = await httpClient.GetAsync("someroute");
            var responseContent = await response.Content.ReadAsStringAsync();

            //Assert
            responseContent.Should().BeEquivalentTo(stringContent);
        }

        [Fact]
        public async Task GetAsync_WithAddResponseHeaderLastBeforeGetObject_ReturnsUnmodifiedContent()
        {
            // Arrange
            var headerToAdd = new KeyValuePair<string, string>("Header-Key", "Header/Value");

            var httpClient = HttpClientMother.Birth()
                .AlwaysRespondWith(HttpStatusCode.OK)
                .AddResponseHeader(headerToAdd.Key, headerToAdd.Value)
                .GetObject();

            //Act
            var response = await httpClient.GetAsync("someroute");
            var addedHeader = response.Headers.GetValues(headerToAdd.Key).FirstOrDefault();

            //Assert
            addedHeader.Should().BeEquivalentTo(headerToAdd.Value);
        }

        [Fact]
        public async Task GetAsync_WithAddResponseHeaderImmediatelyAfterBirth_ReturnsUnmodifiedContent()
        {
            // Arrange
            var headerToAdd = new KeyValuePair<string, string>("Header-Key", "Header/Value");

            var httpClient = HttpClientMother.Birth()
                .AddResponseHeader(headerToAdd.Key, headerToAdd.Value)
                .AlwaysRespondWith(HttpStatusCode.OK)
                .GetObject();

            //Act
            var response = await httpClient.GetAsync("someroute");
            var addedHeader = response.Headers.GetValues(headerToAdd.Key).FirstOrDefault();

            //Assert
            addedHeader.Should().BeEquivalentTo(headerToAdd.Value);
        }

        [Fact]
        public async Task GetAsync_WithAddResponseHeaders_ReturnsAllHeaders()
        {
            // Arrange
            var headersToAdd = new List<KeyValuePair<string, string>>()
            { 
                new KeyValuePair<string, string>("Header-Key1", "Header/Value2"), 
                new KeyValuePair<string, string>("Header-Key3", "Header/Value4")
            };

            var httpClient = HttpClientMother.Birth()
                .AlwaysRespondWith(HttpStatusCode.OK)
                .AddResponseHeaders(headersToAdd)
                .GetObject();

            //Act
            var response = await httpClient.GetAsync("someroute");
            var addedHeader1 = response.Headers.GetValues("Header-Key1").FirstOrDefault();
            var addedHeader3 = response.Headers.GetValues("Header-Key3").FirstOrDefault();

            //Assert
            addedHeader1.Should().BeEquivalentTo("Header/Value2");
            addedHeader3.Should().BeEquivalentTo("Header/Value4");
        }

        [Fact]
        public void GetObject_WithNoSetup_ThrowsInvalidOperationException()
        {
            // Arrange
            //Act
            var action = () => HttpClientMother.Birth().GetObject();

            //Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void AlwaysRespondsWith_AfterAddSpecificResponse_ThrowsInvalidOperationException()
        {
            // Arrange
            //Act
            var action = () => HttpClientMother.Birth()
                                               .AddSpecificResponse("someroute", HttpMethod.Get, HttpStatusCode.OK)              
                                               .AlwaysRespondWith(HttpStatusCode.OK);

            //Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void AddSpecificResponse_AfterAlwaysRespondsWith_ThrowsInvalidOperationException()
        {
            // Arrange
            //Act
            var action = () => HttpClientMother.Birth()
                                               .AlwaysRespondWith(HttpStatusCode.OK)
                                               .AddSpecificResponse("someroute", HttpMethod.Get, HttpStatusCode.OK);

            //Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task AddSpecificResponse_ThenCallingMatchedRoute_Succeeds()
        {
            // Arrange
            var httpClient = HttpClientMother.Birth()
                                            .AddSpecificResponse("users", HttpMethod.Get, HttpStatusCode.OK)
                                            .GetObject();

            //Act
            var response200 = await httpClient.GetAsync("users");

            //Assert
            response200.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AddSpecificResponse_ThenCallingUnmatchedRoute_Returns404()
        {
            // Arrange
            var httpClient = HttpClientMother.Birth()
                                            .AddSpecificResponse("users", HttpMethod.Get, HttpStatusCode.OK)
                                            .GetObject();

            //Act
            var response400 = await httpClient.GetAsync("admins");

            //Assert
            response400.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AddSpecificResponse_WithDisableFallback404ReponseCalled_ThrowsInvalidOperationException()
        {
            // Arrange
            var httpClient = HttpClientMother.Birth()
                                            .DisableFallback404Reponse()
                                            .AddSpecificResponse("users", HttpMethod.Get, HttpStatusCode.OK)
                                            .GetObject();

            //Act
            var action = () => httpClient.GetAsync("admins");

            //Assert
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("Handler did not return a response message.");
        }

        [Fact]
        public async Task AddSpecificResponse_WithMultipleRoutes_ReturnsAppropriately()
        {
            // Arrange
            var httpClient = HttpClientMother.Birth()
                                            .AddSpecificResponse("users", HttpMethod.Get, HttpStatusCode.OK)
                                            .AddSpecificResponse("admins", HttpMethod.Get, HttpStatusCode.BadRequest)
                                            .AddSpecificResponse("servers", HttpMethod.Get, HttpStatusCode.Unauthorized)
                                            .GetObject();

            //Act
            var response401 = await httpClient.GetAsync("servers");
            var response400 = await httpClient.GetAsync("admins");
            var response200 = await httpClient.GetAsync("users");

            //Assert
            response401.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            response400.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response200.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AddSpecificResponse_WithMultiplePostsToTheSameRoute_ReturnsAppropriately()
        {
            // Arrange
            var requestModel1 = new ExampleRequest { RequestID = 7 };
            var requestModel2 = new ExampleRequest { RequestID = 11 };
            var requestModel3 = new ExampleRequest { RequestID = 42 };
            var responseModel1 = new ExampleResponse { RequestID = 7, ResponseID = 42, Successful = true };
            var responseModel2 = new ExampleResponse { RequestID = 11, ResponseID = 43, Successful = true };
            var httpClient = HttpClientMother.Birth()
                                            .AddSpecificResponse("users", HttpMethod.Post, requestModel1.ToJson(), HttpStatusCode.OK, responseModel1.ToJson())
                                            .AddSpecificResponse("users", HttpMethod.Post, requestModel2.ToJson(), HttpStatusCode.OK, responseModel2.ToJson())
                                            .GetObject();

            //Act
            var response1 = await httpClient.PostAsync("users", new StringContent(requestModel1.ToJson()));
            var response1Content = await response1.Content.ReadAsStringAsync();
            var response1Deserialized = response1Content.ToObject<ExampleResponse>();

            var response2 = await httpClient.PostAsync("users", new StringContent(requestModel2.ToJson()));
            var response2Content = await response2.Content.ReadAsStringAsync();
            var response2Deserialized = response2Content.ToObject<ExampleResponse>();

            var response3 = await httpClient.PostAsync("users", new StringContent(requestModel3.ToJson()));


            //Assert
            response1Deserialized.Should().BeEquivalentTo(responseModel1);
            response2Deserialized.Should().BeEquivalentTo(responseModel2);
            response3.StatusCode.Should().Be(HttpStatusCode.NotFound);

        }
    }
}
