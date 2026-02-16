using AwesomeAssertions;
using IsoBoiler.Testing;
using IsoBoiler.Testing.HTTP;
using IsoBoiler.UnitTests.Helpers;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System.Net;

namespace IsoBoiler.UnitTests
{
    public class HttpRequestDataMotherTests
    {
        [Fact]
        public async Task GetFunction_WhenDefault_Returns200OK()
        {
            //Arrange
            var shippingFunction = ObjectMother<ExampleFunction>.Birth(ExampleDefaultServiceProviderBuilder.GetServiceProvider()).GetObject();
            var httpRequestData = HttpRequestDataMother.Birth().GetObject();

            //Act
            var httpResponseData = await shippingFunction.DefaultGetFunction(httpRequestData, Mock.Of<FunctionContext>());

            //Assert
            httpResponseData.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetFunction_WhenGetAndValidHeadersAndQueryParameters_Returns200OK()
        {
            //Arrange
            var shippingFunction = ObjectMother<ExampleFunction>.Birth(ExampleDefaultServiceProviderBuilder.GetServiceProvider()).GetObject();
            var httpRequestData = HttpRequestDataMother.Birth().AddHeader("someHeaderValueOne", "value1")
                                                                .AddHeader("someHeaderValueTwo", "value2")
                                                                .AddQueryParameter("someQueryParameter1", "val1")
                                                                .AddQueryParameter("someQueryParameter2", "val2")
                                                                .GetObject();

            //Act
            var httpResponseData = await shippingFunction.GetFunction(httpRequestData, Mock.Of<FunctionContext>());

            //Assert
            httpResponseData.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task PostFunction_WhenPostAndValidHeadersAndBody_Returns200OK()
        {
            //Arrange
            var shippingFunction = ObjectMother<ExampleFunction>.Birth(ExampleDefaultServiceProviderBuilder.GetServiceProvider()).GetObject();
            var httpRequestData = HttpRequestDataMother.Birth().UseMethod(HttpMethod.Post)
                                                                .AddHeader("someHeaderValueOne", "value1")
                                                                .AddHeader("someHeaderValueTwo", "value2")
                                                                .AddBody<ExampleRequest>()
                                                                .GetObject();

            //Act
            var httpResponseData = await shippingFunction.PostFunction(httpRequestData, Mock.Of<FunctionContext>());

            //Assert
            httpResponseData.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public void AddBody_WhenHttpMethodIsGet_ThrowsException()
        {
            //Arrange
            //Act
            var action1 = () => HttpRequestDataMother.Birth().AddBody("{}");
            var action2 = () => HttpRequestDataMother.Birth().AddBody(new ExampleRequest());
            var action3 = () => HttpRequestDataMother.Birth().AddBody<ExampleRequest>();

            //Assert
            action1.Should().Throw<InvalidOperationException>();
            action2.Should().Throw<InvalidOperationException>();
            action3.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void AddBody_WhenHttpMethodIsPost_Succeeds()
        {
            //Arrange
            //Act
            var action1 = () => HttpRequestDataMother.Birth().UseMethod(HttpMethod.Post).AddBody("{}");
            var action2 = () => HttpRequestDataMother.Birth().UseMethod(HttpMethod.Post).AddBody(new ExampleRequest());
            var action3 = () => HttpRequestDataMother.Birth().UseMethod(HttpMethod.Post).AddBody<ExampleRequest>();

            //Assert
            action1.Should().NotThrow();
            action2.Should().NotThrow();
            action3.Should().NotThrow();
        }
    }
}
