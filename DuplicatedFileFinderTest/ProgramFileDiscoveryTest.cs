using DuplicatedFileFinder;
using System.Reflection;
using Xunit.Abstractions;

namespace DuplicatedFileFinderTest
{
    public class ProgramFileDiscoveryTest : ProgramTestBase
    {
        public ProgramFileDiscoveryTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void DiscoverFiles_WithValidDirectory_ShouldFindFiles()
        {
            // Arrange
            CreateTestFile("file1.txt");
            CreateTestFile("file2.txt");
            CreateTestFile("subfolder/file3.txt");

            var options = new CommandLineOptions { Directory = _testDirectory };

            // Act
            var files = InvokeDiscoverFiles(options);

            // Assert
            Assert.Equal(3, files.Length);
            Assert.All(files, f => Assert.True(File.Exists(f)));
        }

        [Fact]
        public void DiscoverFiles_WithExtensionFilter_ShouldFilterCorrectly()
        {
            // Arrange
            CreateTestFile("file1.txt");
            CreateTestFile("file2.jpg");
            CreateTestFile("file3.png");
            CreateTestFile("file4.pdf");

            var options = new CommandLineOptions
            {
                Directory = _testDirectory,
                FileExtensions = new List<string> { "txt", "jpg" }
            };

            // Act
            var files = InvokeDiscoverFiles(options);

            // Assert
            Assert.Equal(2, files.Length);
            Assert.Contains(files, f => f.EndsWith("file1.txt"));
            Assert.Contains(files, f => f.EndsWith("file2.jpg"));
        }

        [Fact]
        public void DiscoverFiles_WithMinSizeFilter_ShouldFilterCorrectly()
        {
            // Arrange
            CreateTestFile("small.txt", size: 100);
            CreateTestFile("large.txt", size: 2000);

            var options = new CommandLineOptions
            {
                Directory = _testDirectory,
                MinFileSize = 1000
            };

            // Act
            var files = InvokeDiscoverFiles(options);

            // Assert
            Assert.Single(files);
            Assert.Contains(files, f => f.EndsWith("large.txt"));
        }

        [Fact]
        public void DiscoverFiles_WithExcludePatterns_ShouldExcludeMatching()
        {
            // Arrange
            CreateTestFile("normal.txt");
            CreateTestFile("temp_file.txt");
            CreateTestFile("cache/data.txt");

            var options = new CommandLineOptions
            {
                Directory = _testDirectory,
                ExcludePatterns = new List<string> { "temp", "cache" }
            };

            // Act
            var files = InvokeDiscoverFiles(options);

            // Assert
            Assert.Single(files);
            Assert.Contains(files, f => f.EndsWith("normal.txt"));
        }

        private string[] InvokeDiscoverFiles(CommandLineOptions options)
        {
            var programType = typeof(DuplicatedFileFinder.Program);
            var method = programType.GetMethod("DiscoverFiles",
                BindingFlags.NonPublic | BindingFlags.Static);

            return (string[])method.Invoke(null, new object[] { options });
        }
    }
}
