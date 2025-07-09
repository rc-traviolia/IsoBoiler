using AwesomeAssertions;
using IsoBoiler.Testing;
using IsoBoiler.Testing.HTTP;
using IsoBoiler.Tests.Helpers;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System.Net;

namespace IsoBoiler.Tests
{
    public class HttpRequestDataMotherTests
    {

        [Fact]
        public async Task Run_WithValidHeadersAndBody_Returns200OK()
        {
            // Arrange
            var shippingFunction = ObjectMother<ExampleFunction>.Birth(ExampleDefaultServiceProviderBuilder.GetServiceProvider()).GetObject();
            var httpRequestData = HttpRequestDataMother.Birth().AddHeader("someHeaderValueOne", "value1")
                                                               .AddHeader("someHeaderValueTwo", "value2")
                                                               .AddBody<ExampleRequest>()
                                                               .GetObject();

            //Act
            var httpResponseData = await shippingFunction.Run(httpRequestData, Mock.Of<FunctionContext>());

            //Assert
            httpResponseData.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
