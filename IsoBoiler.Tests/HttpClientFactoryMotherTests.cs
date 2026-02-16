using AwesomeAssertions;
using IsoBoiler.Json;
using IsoBoiler.Testing.HTTP;
using IsoBoiler.UnitTests.Helpers;
using System.Net;

namespace IsoBoiler.UnitTests
{
    public class HttpClientFactoryMotherTests
    {
        [Fact]
        public async Task CreateClient_AfterSetup_ReturnsAppropriatelyRespondingHttpClients()
        {
            // Arrange
            var requestModel1 = new ExampleRequest { RequestID = 7 };
            var requestModel2 = new ExampleRequest { RequestID = 11 };
            var requestModel3 = new ExampleRequest { RequestID = 42 };
            var responseModel1 = new ExampleResponse { RequestID = 7, ResponseID = 42, Successful = true };
            var responseModel2 = new ExampleResponse { RequestID = 11, ResponseID = 43, Successful = true };
            var client200 = HttpClientMother.Birth().AlwaysRespondWith(HttpStatusCode.OK).GetObject();
            var httpClientFactory = HttpClientFactoryMother.Birth()
                                               .With("myFirstClient", client200)
                                               .With("myClient", mother =>
                                               {
                                                   mother.AlwaysRespondWith(HttpStatusCode.InsufficientStorage).GetObject();
                                               })
                                               .With("myOtherOtherClient", mother =>
                                               {
                                                   mother.AddSpecificResponse("users", HttpMethod.Post, requestModel1.ToJson(), HttpStatusCode.PartialContent, responseModel1.ToJson())
                                                         .AddSpecificResponse("users", HttpMethod.Post, requestModel2.ToJson(), HttpStatusCode.MultiStatus, responseModel2.ToJson());
                                               })
                                               .GetObject();
            //Act
            var client1 = httpClientFactory.CreateClient("myFirstClient");
            var response1 = await client1.GetAsync("test");

            var client2 = httpClientFactory.CreateClient("myClient");
            var response2 = await client2.GetAsync("test");

            var client3 = httpClientFactory.CreateClient("myOtherOtherClient");
            var response31 = await client3.GetAsync("users"); /* Bad Method */
            var response32 = await client3.PostAsync("test", new StringContent(requestModel1.ToJson())); /* Bad route */
            var response33 = await client3.PostAsync("users", new StringContent(requestModel1.ToJson())); /* Correct route/body */
            var response34 = await client3.PostAsync("users", new StringContent(requestModel2.ToJson())); /* Correct route/body */


            //Assert
            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.InsufficientStorage);
            response31.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response32.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response33.StatusCode.Should().Be(HttpStatusCode.PartialContent);
            response34.StatusCode.Should().Be(HttpStatusCode.MultiStatus);
        }

    }
}
