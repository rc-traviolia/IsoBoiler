using AwesomeAssertions;
using IsoBoiler.Csv;

namespace IsoBoiler.UnitTests
{
    public static class CsvUtilityTests
    {

        [Fact]
        public static void MakePlatformNeutralPath_WithFileNameOnly_ReturnsOnePartPath()
        {
            //Arrange
            var fileName = "myfile.txt";

            //Act
            var result = CsvUtility.MakePlatformNeutralPath(fileName);

            //Assert
            result.Should().BeEquivalentTo(fileName);
        }

        [Fact]
        public static void MakePlatformNeutralPath_WithFileNameAndLeadingSlash_ReturnsFileNameOnly()
        {
            //Arrange
            var fileNameWithLeadingSlash = "/myfile.txt";
            var justFileName = fileNameWithLeadingSlash.Replace('/', ' ').Trim();

            //Act
            var result = CsvUtility.MakePlatformNeutralPath(fileNameWithLeadingSlash);

            //Assert
            result.Should().BeEquivalentTo(justFileName);
        }

        [Fact]
        public static void MakePlatformNeutralPath_WithSplitting_ReturnsValidPath()
        {
            //Arrange
            var unsplitLinuxFilePath = "Data/Test/Folder/myfile.txt";
            var windowsFilePath = unsplitLinuxFilePath.Replace('/', Path.DirectorySeparatorChar);

            //Act
            var result = CsvUtility.MakePlatformNeutralPath(unsplitLinuxFilePath.Split('/'));

            //Assert
            result.Should().BeEquivalentTo(windowsFilePath);
        }

        [Fact]
        public static void MakePlatformNeutralPath_WithoutSplitting_ReturnsValidPath()
        {
            //Arrange
            var unsplitLinuxFilePath = "Data/Test/Folder/myfile.txt";
            var windowsFilePath = unsplitLinuxFilePath.Replace('/', Path.DirectorySeparatorChar);

            //Act
            var result = CsvUtility.MakePlatformNeutralPath(unsplitLinuxFilePath);

            //Assert
            result.Should().BeEquivalentTo(windowsFilePath);
        }

        [Fact]
        public static void MakePlatformNeutralPath_WithBothSplittingAndNotSplitting_ReturnsValidPath()
        {
            //Arrange
            var unsplitWindowsFilPath = "Test\\Folder\\Level\\One";
            var aRandomFolderInBetween = "FOLDER_NAME";
            var unsplitLinuxFilePath = "Data/Test/Folder/myfile.txt";
            var windowsFilePath = Path.Combine(unsplitWindowsFilPath.Replace('\\', Path.DirectorySeparatorChar), aRandomFolderInBetween, unsplitLinuxFilePath.Replace('/', Path.DirectorySeparatorChar));

            //Act
            var result = CsvUtility.MakePlatformNeutralPath(unsplitWindowsFilPath, aRandomFolderInBetween, unsplitLinuxFilePath);

            //Assert
            result.Should().BeEquivalentTo(windowsFilePath);
        }
    }
}
