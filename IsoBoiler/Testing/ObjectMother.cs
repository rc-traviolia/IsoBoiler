using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace IsoBoiler.Testing
{

    public class ObjectMother<TServiceToTest> where TServiceToTest : class
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly List<object> _serviceOverrides = new();
        public static IServiceProvider DefaultServiceProvider { get; set; } = ConfigHelper.GetDefaultServiceProvider();

        //Call the static method Build() instead to create a new instance.
        private ObjectMother(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static ObjectMother<TServiceToTest> Birth(IServiceProvider serviceProvider)
        {
            return new ObjectMother<TServiceToTest>(serviceProvider);
        }

        public static ObjectMother<TServiceToTest> Birth(Action<HostBuilderContext, IServiceCollection>? configureDelegate = null)
        {
            if (configureDelegate is not null)
            {
                return new ObjectMother<TServiceToTest>(ConfigHelper.GetServiceProvider(configureDelegate));
            }
            else
            {
                return new ObjectMother<TServiceToTest>(DefaultServiceProvider);
            }
        }

        public ObjectMother<TServiceToTest> With<TDependency>(TDependency dependency)
        {
            //Add the dependency provided as an override
            _serviceOverrides.Add(dependency!);
            return this;
        }

        public ObjectMother<TServiceToTest> With<TDependency>(Action<Mock<TDependency>>? setupDelegate = null) where TDependency : class
        {
            if(setupDelegate is null)
            {
                return this.With(Mock.Of<TDependency>());
            }
            else
            {
                var dependencyMock = new Mock<TDependency>();
                setupDelegate(dependencyMock);
                return this.With(dependencyMock.Object);
            }
        }

        //This should be the final call to get the testable object
        public TServiceToTest GetObject() => ActivatorUtilities.CreateInstance<TServiceToTest>(_serviceProvider, _serviceOverrides.ToArray());
    }
}
