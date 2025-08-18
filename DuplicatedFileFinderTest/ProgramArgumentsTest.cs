using DuplicatedFileFinder;
using System.Reflection;
using Xunit.Abstractions;

namespace DuplicatedFileFinderTest
{
    public class ProgramArgumentsTest : ProgramTestBase
    {
        public ProgramArgumentsTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void ParseArguments_WithValidDirectory_ShouldSetDirectory()
        {
            // Arrange
            var args = new[] { "--directory", _testDirectory };

            // Act
            var options = InvokeParseArguments(args);

            // Assert
            Assert.Equal(_testDirectory, options.Directory);
            Assert.False(options.ShowHelp);
        }

        [Fact]
        public void ParseArguments_WithHelpFlag_ShouldSetShowHelp()
        {
            // Arrange
            var args = new[] { "--help" };

            // Act
            var options = InvokeParseArguments(args);

            // Assert
            Assert.True(options.ShowHelp);
        }

        [Fact]
        public void ParseArguments_WithShortHelpFlag_ShouldSetShowHelp()
        {
            // Arrange
            var args = new[] { "-h" };

            // Act
            var options = InvokeParseArguments(args);

            // Assert
            Assert.True(options.ShowHelp);
        }

        [Theory]
        [InlineData("--extensions", "jpg,png,pdf")]
        [InlineData("-e", "txt,doc")]
        public void ParseArguments_WithExtensions_ShouldParseCorrectly(string flag, string extensions)
        {
            // Arrange
            var args = new[] { flag, extensions };

            // Act
            var options = InvokeParseArguments(args);

            // Assert
            var expectedExtensions = extensions.Split(',').Select(e => e.Trim().ToLower()).ToList();
            Assert.Equal(expectedExtensions, options.FileExtensions);
        }

        [Fact]
        public void ParseArguments_WithMinSize_ShouldParseCorrectly()
        {
            // Arrange
            var args = new[] { "--min-size", "1024" };

            // Act
            var options = InvokeParseArguments(args);

            // Assert
            Assert.Equal(1024, options.MinFileSize);
        }

        [Fact]
        public void ParseArguments_WithMaxResults_ShouldParseCorrectly()
        {
            // Arrange
            var args = new[] { "--max-results", "100" };

            // Act
            var options = InvokeParseArguments(args);

            // Assert
            Assert.Equal(100, options.MaxResults);
        }

        [Fact]
        public void ParseArguments_WithSilentMode_ShouldSetFlag()
        {
            // Arrange
            var args = new[] { "--silent" };

            // Act
            var options = InvokeParseArguments(args);

            // Assert
            Assert.True(options.SilentMode);
        }

        [Fact]
        public void ParseArguments_WithVerboseMode_ShouldSetFlag()
        {
            // Arrange
            var args = new[] { "--verbose" };

            // Act
            var options = InvokeParseArguments(args);

            // Assert
            Assert.True(options.VerboseMode);
        }

        [Fact]
        public void ParseArguments_WithDirectoryWithoutFlag_ShouldSetDirectory()
        {
            // Arrange
            var args = new[] { _testDirectory };

            // Act
            var options = InvokeParseArguments(args);

            // Assert
            Assert.Equal(_testDirectory, options.Directory);
        }

        [Fact]
        public void ParseArguments_WithExcludePatterns_ShouldParseCorrectly()
        {
            // Arrange
            var args = new[] { "--exclude", "temp,cache,log" };

            // Act
            var options = InvokeParseArguments(args);

            // Assert
            var expected = new[] { "temp", "cache", "log" };
            Assert.Equal(expected, options.ExcludePatterns);
        }

        private CommandLineOptions InvokeParseArguments(string[] args)
        {
            var programType = typeof(DuplicatedFileFinder.Program);
            var method = programType.GetMethod("ParseArguments",
                BindingFlags.NonPublic | BindingFlags.Static);

            return (CommandLineOptions)method.Invoke(null, new object[] { args });
        }
    }
}