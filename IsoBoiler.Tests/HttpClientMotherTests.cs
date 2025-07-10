//using AwesomeAssertions;
//using IsoBoiler.Json;
//using IsoBoiler.Testing.HTTP;
//using IsoBoiler.Tests.Helpers;
//using System.Net;

//namespace IsoBoiler.Tests
//{
//    public class HttpClientMotherTests
//    {
//        [Fact]
//        public async Task GetAsync_WithDefaultClient_Returns200OK()
//        {
//            // Arrange
//            var httpClient = HttpClientMother.Birth();

//            //Act
//            var response = await httpClient.GetAsync("something");

//            //Assert
//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//        }

//        [Fact]
//        public async Task GetAsync_With500HttpStatusCodeOverride_Returns500()
//        {
//            // Arrange
//            var httpClient = HttpClientMother.Birth(HttpStatusCode.InternalServerError);

//            //Act
//            var response = await httpClient.GetAsync("something");

//            //Assert
//            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
//        }

//        [Fact]
//        public async Task GetAsync_WithProvidedContent_ReturnsUnmodifiedContent()
//        {
//            // Arrange
//            var content = new ExampleResponse();
//            var contentAsString = content.ToJson();
//            var httpClient = HttpClientMother.Birth(HttpStatusCode.OK, contentAsString);

//            //Act
//            var response = await httpClient.GetAsync("something");
//            var responseContent = await response.Content.ReadAsStringAsync();

//            //Assert
//            responseContent.Should().BeEquivalentTo(contentAsString);
//        }

//        [Fact]
//        public async Task PostAsync_WithDefaultClient_Returns200OK()
//        {
//            // Arrange
//            var httpClient = HttpClientMother.Birth();
//            var stringContent = new StringContent("data as a string");

//            //Act
//            var response = await httpClient.PostAsync("something", stringContent);

//            //Assert
//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//        }



//        //[Fact]
//        //public async Task GetAsync_AtVariousEndpoints_ReturnsDifferentResponses()
//        //{
//        //    // Arrange
//        //    var httpClient = HttpClientMother.Birth()
//        //        .Setup();

//        //    //Act
//        //    var response = await httpClient.GetAsync("something");

//        //    //Assert
//        //    response.StatusCode.Should().Be(HttpStatusCode.OK);
//        //}
//    }
//}
