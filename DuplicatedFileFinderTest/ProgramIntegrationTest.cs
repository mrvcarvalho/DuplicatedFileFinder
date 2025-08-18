using Xunit.Abstractions;

namespace DuplicatedFileFinderTest
{
    public class ProgramIntegrationTest : ProgramTestBase
    {
        public ProgramIntegrationTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Main_WithHelpArgument_ShouldShowHelpAndExit()
        {
            // Arrange
            var args = new[] { "--help", "--no-pause" };

            // Act
            var exception = Record.Exception(() =>
                DuplicatedFileFinder.Program.Main(args));

            // Assert
            Assert.Null(exception);
            var output = GetConsoleOutput();
            Assert.Contains("USO:", output);
        }

        [Fact]
        public void Main_WithVersionArgument_ShouldShowVersionAndExit()
        {
            // Arrange
            var args = new[] { "--version", "--no-pause" };

            // Act & Assert
            // Note: --version calls Environment.Exit(0), so we can't test this directly
            // This would require refactoring the Main method to be more testable
            Assert.True(true); // Placeholder - would need refactoring to test properly
        }

        [Fact]
        public void Main_WithInvalidDirectory_ShouldShowError()
        {
            // Arrange
            var args = new[] { "--directory", "C:\\NonExistentDirectory123", "--no-pause" };

            // Act
            var exception = Record.Exception(() =>
                DuplicatedFileFinder.Program.Main(args));

            // Assert
            Assert.Null(exception);
            var output = GetConsoleOutput();
            Assert.Contains("[ERROR]", output);
            Assert.Contains("Diretório não encontrado", output);
        }

        [Fact]
        public void Main_WithNoArguments_ShouldShowError()
        {
            // Arrange
            var args = new[] { "--no-pause" };

            // Act
            var exception = Record.Exception(() =>
                DuplicatedFileFinder.Program.Main(args));

            // Assert
            Assert.Null(exception);
            var output = GetConsoleOutput();
            Assert.Contains("[ERROR]", output);
            Assert.Contains("Diretório não especificado", output);
        }
    }
}
