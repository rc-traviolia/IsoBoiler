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

        public PendingDependencyOverride<TServiceToTest, TDependency> With<TDependency>() where TDependency : class
        {
            //Create a PendingDependencyOverride for the given TDependency type to allow Setup Methodes to be chained
            return new PendingDependencyOverride<TServiceToTest, TDependency>(this);
        }

        //This should be the final call to get the testable object
        public TServiceToTest GetObject() => ActivatorUtilities.CreateInstance<TServiceToTest>(_serviceProvider, _serviceOverrides.ToArray());
    }

    public class PendingDependencyOverride<TServiceToTest, TDependency> where TDependency : class
                                                                        where TServiceToTest : class
    {
        public ObjectMother<TServiceToTest> ObjectMotherReference { get; set; }
        public Mock<TDependency> DependencyOverride { get; set; }
        public PendingDependencyOverride(ObjectMother<TServiceToTest> objectMotherReference)
        {
            ObjectMotherReference = objectMotherReference;
            DependencyOverride = new Mock<TDependency>();
        }

        public ObjectMother<TServiceToTest> With<TNewDependency>(TNewDependency dependency)
        {
            //Add this PendingDependencyOverride, and then execute the With() on the ObjectMother and return that reference
            //So we can move back and forth between ObjectMother and PendingDependencyOverrides
            return ObjectMotherReference.With(DependencyOverride.Object).With(dependency);
        }

        public PendingDependencyOverride<TServiceToTest, TNewDependency> With<TNewDependency>() where TNewDependency : class
        {
            //When beginning a new PendingDependencyOverride, we need to make sure this one was added to the ObjectMotherReference
            return new PendingDependencyOverride<TServiceToTest, TNewDependency>(ObjectMotherReference.With(DependencyOverride.Object));
        }

        //This should be the final call to get the testable object
        public TServiceToTest GetObject() => ObjectMotherReference.With(DependencyOverride.Object).GetObject();


        //
        public PendingDependencyOverride<TServiceToTest, TDependency> Setup(Action<Mock<TDependency>> configure)
        {
            configure(DependencyOverride);
            return this;
        }
    }
}
