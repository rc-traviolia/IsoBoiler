using AwesomeAssertions;
using IsoBoiler.Logging;
using IsoBoiler.Testing;
using IsoBoiler.Testing.HTTP;
using IsoBoiler.Tests.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;

namespace IsoBoiler.Tests
{
    public class ObjectMotherTests
    {
        [Fact]
        public static void GetObject_WithMissingDependencies_ThrowsException()
        {
            //Arrange
            //Act
            var action = () => ObjectMother<ExampleFunction>.Birth((context, services) => { /* empty for testing*/ })
                                                   .With<HealthCheckService>()
                                                   //.With<ILogBoiler>()
                                                   .GetObject();

            //Assert
            // The message could change someday, in theory, so we don't need to be too attached. Any exception is pretty much fine.
            action.Should().Throw<InvalidOperationException>().WithMessage("Unable to resolve service for type*");
        }

        [Fact]
        public static void GetObject_WithMockOfPassedIn_InjectsDependenciesCorrectly()
        {
            //Arrange
            //Act
            var action = () => ObjectMother<ExampleFunction>.Birth((context, services) => { /* empty for testing*/ })
                                                   .With(Mock.Of<HealthCheckService>())
                                                   .With(Mock.Of<ILogBoiler>())
                                                   .GetObject();
            //Assert
            action.Should().NotThrow<Exception>();
        }

        [Fact]
        public static void GetObject_WithMockReferencesPassedIn_InjectsDependenciesCorrectly()
        {
            //Arrange
            var healthCheckServiceMock = Mock.Of<HealthCheckService>();
            var logBoilerMock = Mock.Of<ILogBoiler>();

            //Act
            var action = () => ObjectMother<ExampleFunction>.Birth((context, services) => { /* empty for testing*/ })
                                                   .With(healthCheckServiceMock)
                                                   .With(logBoilerMock)
                                                   .GetObject();
            //Assert
            action.Should().NotThrow<Exception>();
        }

        [Fact]
        public static void GetObject_WithPendingDependencyOverrideWiths_InjectsDependenciesCorrectly()
        {
            //Arrange
            var healthCheckServiceMock = Mock.Of<HealthCheckService>();
            var logBoilerMock = Mock.Of<ILogBoiler>();

            //Act
            var action = () => ObjectMother<ExampleFunction>.Birth((context, services) => { /* empty for testing*/ })
                                                   .With<HealthCheckService>()
                                                   .With<ILogBoiler>()
                                                   .GetObject();
            //Assert
            action.Should().NotThrow<Exception>();
        }

        [Fact]
        public static void GetObject_WithMixedWithTypes_InjectsDependenciesCorrectly()
        {
            //Arrange
            var healthCheckServiceMockObject = Mock.Of<HealthCheckService>();
            var logBoilerMockObject = Mock.Of<ILogBoiler>();

            //Act
            var action11 = () => ObjectMother<ExampleFunction>.Birth((context, services) => { /* empty for testing*/ })
                                                     .With(healthCheckServiceMockObject)
                                                     .With(logBoilerMockObject)
                                                     .GetObject();
            var action12 = () => ObjectMother<ExampleFunction>.Birth((context, services) => { /* empty for testing*/ })
                                                     .With(healthCheckServiceMockObject)
                                                     .With<ILogBoiler>()
                                                     .GetObject();
            var action13 = () => ObjectMother<ExampleFunction>.Birth((context, services) => { /* empty for testing*/ })
                                                     .With(healthCheckServiceMockObject)
                                                     .With(Mock.Of<ILogBoiler>())
                                                     .GetObject();

            var action21 = () => ObjectMother<ExampleFunction>.Birth((context, services) => { /* empty for testing*/ })
                                                     .With<HealthCheckService>()
                                                     .With(logBoilerMockObject)
                                                     .GetObject();
            var action22 = () => ObjectMother<ExampleFunction>.Birth((context, services) => { /* empty for testing*/ })
                                                     .With<HealthCheckService>()
                                                     .With<ILogBoiler>()
                                                     .GetObject();
            var action23 = () => ObjectMother<ExampleFunction>.Birth((context, services) => { /* empty for testing*/ })
                                                     .With<HealthCheckService>()
                                                     .With(Mock.Of<ILogBoiler>())
                                                     .GetObject();

            var action31 = () => ObjectMother<ExampleFunction>.Birth((context, services) => { /* empty for testing*/ })
                                                     .With(Mock.Of<HealthCheckService>())
                                                     .With(logBoilerMockObject)
                                                     .GetObject();
            var action32 = () => ObjectMother<ExampleFunction>.Birth((context, services) => { /* empty for testing*/ })
                                                     .With(Mock.Of<HealthCheckService>())
                                                     .With<ILogBoiler>()
                                                     .GetObject();
            var action33 = () => ObjectMother<ExampleFunction>.Birth((context, services) => { /* empty for testing*/ })
                                                     .With(Mock.Of<HealthCheckService>())
                                                     .With(Mock.Of<ILogBoiler>())
                                                     .GetObject();
            //Assert
            action11.Should().NotThrow<Exception>();
            action12.Should().NotThrow<Exception>();
            action13.Should().NotThrow<Exception>();
            action21.Should().NotThrow<Exception>();
            action22.Should().NotThrow<Exception>();
            action23.Should().NotThrow<Exception>();
            action31.Should().NotThrow<Exception>();
            action32.Should().NotThrow<Exception>();
            action33.Should().NotThrow<Exception>();
        }



        [Fact]
        public static void GetObject_WithDefaultIServiceProvider_InjectsDependenciesCorrectly()
        {
            //Arrange
            //Act
            var action = () => ObjectMother<ExampleFunction>.Birth(ExampleDefaultServiceProviderBuilder.GetServiceProvider()).GetObject();
            //Assert
            action.Should().NotThrow<Exception>();
        }

        [Fact]
        public static void GetObject_WithCustomIServiceProvider_InjectsDependenciesCorrectly()
        {
            //Arrange
            //Act
            var action = () => ObjectMother<ExampleFunction>.Birth((context, services) =>
            {
                services.AddSingleton(Mock.Of<HealthCheckService>());
                services.AddSingleton(Mock.Of<ILogBoiler>());
            })
                                                    .GetObject();
            //Assert
            action.Should().NotThrow<Exception>();
        }

        [Fact]
        public static void GetObject_WithCustomIServiceProviderAndOverride_InjectsDependenciesCorrectly()
        {
            //Arrange
            //Act
            var action = () => ObjectMother<ExampleFunction>.Birth((context, services) =>
            {
                services.AddSingleton(Mock.Of<HealthCheckService>());
            })
                                                    .With(Mock.Of<ILogBoiler>())
                                                    .GetObject();
            //Assert
            action.Should().NotThrow<Exception>();
        }

        [Fact]
        public static async Task GetObject_WithOverrideFailing_Fails()
        {
            //Arrange
            var succeedingLogBoilerMockObject = Mock.Of<ILogBoiler>();
            var failingLogBoilerMock = new Mock<ILogBoiler>();
            failingLogBoilerMock.Setup(lb => lb.BeginScope(It.IsAny<FunctionContext>()))
                                .Throws(new Exception("simulated failure"));

            var health = ObjectMother<ExampleFunction>.Birth((context, services) =>
            {
                services.AddSingleton(Mock.Of<HealthCheckService>());
                services.AddSingleton(succeedingLogBoilerMockObject);
            })
                                             .With(failingLogBoilerMock.Object)
                                             .GetObject();

            var httpRequestDataMock = HttpRequestDataMother.Birth().GetObject();
            var functionContextMock = Mock.Of<FunctionContext>();

            //Act
            var action = async () => await health.PostFunction(httpRequestDataMock, functionContextMock);

            //Assert
            await action.Should().ThrowAsync<Exception>().WithMessage("simulated failure");
        }

        [Fact]
        public static async Task GetObject_WithOverridePassing_Passes()
        {
            //Arrange
            var succesedingLogBoilerMockObject = Mock.Of<ILogBoiler>();
            var failingLogBoilerMock = new Mock<ILogBoiler>();
            failingLogBoilerMock.Setup(lb => lb.BeginScope(It.IsAny<FunctionContext>()))
                                .Throws(new Exception("simulated failure"));

            var health = ObjectMother<ExampleFunction>.Birth((context, services) =>
                                                       {
                                                          services.AddSingleton(Mock.Of<HealthCheckService>());
                                                          services.AddSingleton(failingLogBoilerMock.Object);
                                                       })
                                                       .With(succesedingLogBoilerMockObject)
                                                       .GetObject();

            var httpRequestData = HttpRequestDataMother.Birth().UseMethod(HttpMethod.Post)
                                                               .AddHeader("someHeaderValueOne", "value1")
                                                               .AddHeader("someHeaderValueTwo", "value2")
                                                               .AddBody<ExampleRequest>()
                                                               .GetObject();
            var functionContextMock = Mock.Of<FunctionContext>();

            //Act
            var action = () => health.PostFunction(httpRequestData, functionContextMock);

            //Assert
            await action.Should().NotThrowAsync<Exception>();
        }

        [Fact]
        public static async Task Setup_WithOverrideFailing_Fails()
        {
            //Arrange
            var health = ObjectMother<ExampleFunction>.Birth(ExampleDefaultServiceProviderBuilder.GetServiceProvider())
                                             .With<ILogBoiler>()
                                             .Setup(logBoilerMock =>
                                             {
                                                 logBoilerMock.Setup(lb => lb.BeginScope(It.IsAny<FunctionContext>()))
                                                              .Throws(new Exception("simulated failure"));
                                             })
                                             .GetObject();

            var httpRequestDataMock = HttpRequestDataMother.Birth().GetObject();
            var functionContextMock = Mock.Of<FunctionContext>();

            //Act
            var action = async () => await health.PostFunction(httpRequestDataMock, functionContextMock);

            //Assert
            await action.Should().ThrowAsync<Exception>().WithMessage("simulated failure");
        }
    }
}
