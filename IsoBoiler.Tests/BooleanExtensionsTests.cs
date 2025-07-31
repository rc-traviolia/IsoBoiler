using AwesomeAssertions;
using IsoBoiler.Booleans;

namespace IsoBoiler.Tests
{
    public class BooleanExtensionsTests
    {
        [Fact]
        public void In_WithNoParams_ThrowsArgumentException()
        {
            //For the sake of code coverage.
            //I think this kind of code should probably be kept in a shared NuGet Package, rather than living in each repository

            //Arrange
            //NONE

            //Act
            var action = () => 1.In();

            //Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void In_WithMatchingValueInParams_ReturnsTrue()
        {
            //For the sake of code coverage.
            //I think this kind of code should probably be kept in a shared NuGet Package, rather than living in each repository

            //Arrange
            //NONE

            //Act
            var result = 1.In(1, 2, 3);

            //Assert
            result.Should().Be(true);
        }

        [Fact]
        public void In_WithoutMatchingValueInParams_ReturnsFalse()
        {
            //For the sake of code coverage.
            //I think this kind of code should probably be kept in a shared NuGet Package, rather than living in each repository

            //Arrange
            //NONE

            //Act
            var result = 1.In(2, 3, 4);

            //Assert
            result.Should().Be(false);
        }
    }
}