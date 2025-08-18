using DuplicatedFileFinderTest.CommonTestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace DuplicatedFileFinderTest
{
    public class ProgramDisplayTest : ProgramTestBase
    {
        private readonly TestConsoleWrapper _testConsole;

        public ProgramDisplayTest(ITestOutputHelper output) : base(output)
        {
            _testConsole = new TestConsoleWrapper();
            // Configurar o wrapper de teste
            var programType = typeof(DuplicatedFileFinder.Program);
            var method = programType.GetMethod("SetConsoleWrapper",
                BindingFlags.NonPublic | BindingFlags.Static);
            method?.Invoke(null, new object[] { _testConsole });
        }

        [Fact]
        public void ShowHeader_ShouldDisplayCorrectInformation()
        {
            // Act
            InvokeShowHeader();
            var output = _testConsole.GetOutput();

            // Assert
            Assert.Contains("DUPLICATED FILE FINDER", output);
            Assert.Contains(DuplicatedFileFinder.AssemblyInfo.ShortVersion, output);
            Assert.Contains(DuplicatedFileFinder.AssemblyInfo.FileVersion, output);

            _output.WriteLine("Header Output:");
            _output.WriteLine(output);
        }

        [Fact]
        public void ShowHelp_ShouldDisplayUsageInformation()
        {
            // Act
            InvokeShowHelp();
            var output = GetConsoleOutput();

            // Assert
            Assert.Contains("USO:", output);
            Assert.Contains("--directory", output);
            Assert.Contains("--help", output);
            Assert.Contains("EXEMPLOS:", output);

            // Help Output
            _output.WriteLine(output);
        }

        [Fact]
        public void ShowHelp_ShouldContainAllMajorOptions()
        {
            // Act
            InvokeShowHelp();
            var output = GetConsoleOutput();

            // Assert
            var expectedOptions = new[]
            {
            "--directory", "--extensions", "--exclude", "--min-size",
            "--max-results", "--silent", "--verbose", "--list",
            "--load", "--export", "--help", "--version"
        };

            foreach (var option in expectedOptions)
            {
                Assert.Contains(option, output);
            }

            // Help Output
            _output.WriteLine(output);
        }

        private void InvokeShowHeader()
        {
            var programType = typeof(DuplicatedFileFinder.Program);
            var method = programType.GetMethod("ShowHeader",
                BindingFlags.NonPublic | BindingFlags.Static);
            method.Invoke(null, null);
        }

        private void InvokeShowHelp()
        {
            var programType = typeof(DuplicatedFileFinder.Program);
            var method = programType.GetMethod("ShowHelp",
                BindingFlags.NonPublic | BindingFlags.Static);
            method.Invoke(null, null);
        }
    }
}
