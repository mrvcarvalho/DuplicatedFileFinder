using Xunit.Abstractions;

namespace DuplicatedFileFinderTest
{
    public class AssemblyInfoTest
    {
        private readonly ITestOutputHelper _output;

        public AssemblyInfoTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Version_ShouldReturnValidVersionString()
        {
            // Act
            var version = DuplicatedFileFinder.AssemblyInfo.Version;

            // Assert
            Assert.NotNull(version);
            Assert.NotEqual("Unknown", version);
            Assert.True(Version.TryParse(version, out _), $"Version '{version}' should be a valid version format");

            _output.WriteLine($"Version: {version}");
        }

        [Fact]
        public void FileVersion_ShouldReturnValidFileVersion()
        {
            // Act
            var fileVersion = DuplicatedFileFinder.AssemblyInfo.FileVersion;

            // Assert
            Assert.NotNull(fileVersion);
            Assert.NotEqual("Unknown", fileVersion);

            _output.WriteLine($"FileVersion: {fileVersion}");
        }

        [Fact]
        public void ProductVersion_ShouldReturnValidProductVersion()
        {
            // Act
            var productVersion = DuplicatedFileFinder.AssemblyInfo.ProductVersion;

            // Assert
            Assert.NotNull(productVersion);
            Assert.NotEqual("Unknown", productVersion);

            _output.WriteLine($"ProductVersion: {productVersion}");
        }

        [Fact]
        public void ProductTitle_ShouldReturnExpectedTitle()
        {
            // Act
            var productTitle = DuplicatedFileFinder.AssemblyInfo.ProductTitle;

            // Assert
            Assert.NotNull(productTitle);
            Assert.NotEmpty(productTitle);

            _output.WriteLine($"ProductTitle: {productTitle}");
        }

        [Fact]
        public void Description_ShouldReturnDescription()
        {
            // Act
            var description = DuplicatedFileFinder.AssemblyInfo.Description;

            // Assert
            Assert.NotNull(description);

            _output.WriteLine($"Description: {description}");
        }

        [Fact]
        public void Company_ShouldReturnCompanyName()
        {
            // Act
            var company = DuplicatedFileFinder.AssemblyInfo.Company;

            // Assert
            Assert.NotNull(company);
            Assert.NotEmpty(company);

            _output.WriteLine($"Company: {company}");
        }

        [Fact]
        public void Copyright_ShouldReturnCopyrightInfo()
        {
            // Act
            var copyright = DuplicatedFileFinder.AssemblyInfo.Copyright;

            // Assert
            Assert.NotNull(copyright);
            Assert.NotEmpty(copyright);

            _output.WriteLine($"Copyright: {copyright}");
        }

        [Fact]
        public void InformationalVersion_ShouldReturnValidVersion()
        {
            // Act
            var informationalVersion = DuplicatedFileFinder.AssemblyInfo.InformationalVersion;

            // Assert
            Assert.NotNull(informationalVersion);
            Assert.NotEmpty(informationalVersion);

            _output.WriteLine($"InformationalVersion: {informationalVersion}");
        }

        [Fact]
        public void BuildDate_ShouldReturnValidDate()
        {
            // Act
            var buildDate = DuplicatedFileFinder.AssemblyInfo.BuildDate;

            // Assert
            Assert.NotEqual(DateTime.MinValue, buildDate);
            Assert.True(buildDate <= DateTime.Now, "Build date should not be in the future");

            _output.WriteLine($"BuildDate: {buildDate:yyyy-MM-dd HH:mm:ss}");
        }

        [Fact]
        public void BuildMachine_ShouldReturnMachineName()
        {
            // Act
            var buildMachine = DuplicatedFileFinder.AssemblyInfo.BuildMachine;

            // Assert
            Assert.NotNull(buildMachine);
            Assert.NotEmpty(buildMachine);

            _output.WriteLine($"BuildMachine: {buildMachine}");
        }

        [Fact]
        public void BuildUser_ShouldReturnUserName()
        {
            // Act
            var buildUser = DuplicatedFileFinder.AssemblyInfo.BuildUser;

            // Assert
            Assert.NotNull(buildUser);
            Assert.NotEmpty(buildUser);

            _output.WriteLine($"BuildUser: {buildUser}");
        }

        [Fact]
        public void ShortVersion_ShouldStartWithV()
        {
            // Act
            var shortVersion = DuplicatedFileFinder.AssemblyInfo.ShortVersion;

            // Assert
            Assert.NotNull(shortVersion);
            Assert.StartsWith("v", shortVersion);

            _output.WriteLine($"ShortVersion: {shortVersion}");
        }

        [Fact]
        public void MediumVersion_ShouldContainVersionAndBuild()
        {
            // Act
            var mediumVersion = DuplicatedFileFinder.AssemblyInfo.MediumVersion;

            // Assert
            Assert.NotNull(mediumVersion);
            Assert.StartsWith("v", mediumVersion);
            Assert.Contains("Build", mediumVersion);

            _output.WriteLine($"MediumVersion: {mediumVersion}");
        }

        [Fact]
        public void FullVersionInfo_ShouldContainProductTitleAndVersion()
        {
            // Act
            var fullVersionInfo = DuplicatedFileFinder.AssemblyInfo.FullVersionInfo;

            // Assert
            Assert.NotNull(fullVersionInfo);
            Assert.Contains(DuplicatedFileFinder.AssemblyInfo.ProductTitle, fullVersionInfo);
            Assert.Contains("Build:", fullVersionInfo);

            _output.WriteLine($"FullVersionInfo: {fullVersionInfo}");
        }

        [Fact]
        public void HeaderVersion_ShouldContainVersionAndBuildNumber()
        {
            // Act
            var headerVersion = DuplicatedFileFinder.AssemblyInfo.HeaderVersion;

            // Assert
            Assert.NotNull(headerVersion);
            Assert.StartsWith("v", headerVersion);
            Assert.Contains("Build", headerVersion);

            _output.WriteLine($"HeaderVersion: {headerVersion}");
        }

        [Fact]
        public void ShowVersion_WithStringWriter_ShouldWriteCorrectContent()
        {
            // Arrange
            using var stringWriter = new StringWriter();

            // Act
            DuplicatedFileFinder.AssemblyInfo.ShowVersion(stringWriter);
            var output = stringWriter.ToString();

            // Assert
            Assert.Contains($"Product          : ", output);
            Assert.Contains($"Assembly Version : ", output);
            Assert.Contains($"File Version     : ", output);
            Assert.Contains($"Product Version  : ", output);
            Assert.Contains($"Build Date       : ", output);
            Assert.Contains($"Build Machine    : ", output);

            _output.WriteLine("Captured Output:");
            _output.WriteLine(output);
        }

        [Fact]
        public void ShowVersion_WithNullWriter_ShouldUseConsoleOut()
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                DuplicatedFileFinder.AssemblyInfo.ShowVersion(null));

            Assert.Null(exception);
        }

        [Fact]
        public void DebugAssemblyInfo_WithStringWriter_ShouldWriteCorrectContent()
        {
            // Arrange
            using var stringWriter = new StringWriter();

            // Act
            DuplicatedFileFinder.AssemblyInfo.DebugAssemblyInfo(stringWriter);
            var output = stringWriter.ToString();

            // Assert
            Assert.Contains("DebugAssemblyInfo()", output);
            Assert.Contains("Nome", output);
            Assert.Contains("Versão", output);
            Assert.Contains("Local", output);
            Assert.Contains("Copyright", output);
            Assert.Contains("Title", output);
            Assert.Contains("Description", output);
            Assert.Contains("Company", output);
            Assert.Contains("Product", output);
            Assert.Contains("Version", output);
            Assert.Contains("FileVersion", output);
            Assert.Contains("BuildDate", output);
            Assert.Contains("BuildMachine", output);
            Assert.Contains("BuildUser", output);

            _output.WriteLine("Captured Output:");
            _output.WriteLine(output);
        }

        [Fact]
        public void DebugAssemblyInfo_WithNullWriter_ShouldUseConsoleOut()
        {
            // Act & Assert
            var exception = Record.Exception(() => DuplicatedFileFinder.AssemblyInfo.DebugAssemblyInfo());

            Assert.Null(exception);
        }


        [Theory]
        [InlineData("1.0.0.0")]
        [InlineData("2.1.3.4")]
        public void Version_ShouldMatchExpectedFormat(string expectedPattern)
        {
            // Act
            var version = DuplicatedFileFinder.AssemblyInfo.Version;

            // Assert
            Assert.Matches(@"^\d+\.\d+\.\d+\.\d+$", version);

            _output.WriteLine($"Version matches pattern: {version}");
        }

        [Fact]
        public void AllStringProperties_ShouldNotBeNullOrWhitespace()
        {
            // Arrange
            var stringProperties = new[]
            {
                DuplicatedFileFinder.AssemblyInfo.Version,
                DuplicatedFileFinder.AssemblyInfo.FileVersion,
                DuplicatedFileFinder.AssemblyInfo.ProductVersion,
                DuplicatedFileFinder.AssemblyInfo.ProductTitle,
                DuplicatedFileFinder.AssemblyInfo.Company,
                DuplicatedFileFinder.AssemblyInfo.Copyright,
                DuplicatedFileFinder.AssemblyInfo.InformationalVersion,
                DuplicatedFileFinder.AssemblyInfo.BuildMachine,
                DuplicatedFileFinder.AssemblyInfo.BuildUser,
                DuplicatedFileFinder.AssemblyInfo.ShortVersion,
                DuplicatedFileFinder.AssemblyInfo.MediumVersion,
                DuplicatedFileFinder.AssemblyInfo.FullVersionInfo,
                DuplicatedFileFinder.AssemblyInfo.HeaderVersion
            };

            // Act & Assert
            foreach (var property in stringProperties)
            {
                Assert.False(string.IsNullOrWhiteSpace(property),
                    $"Property should not be null or whitespace: {property}");
            }
        }

        [Fact]
        public void BuildDate_ShouldBeReasonable()
        {
            // Act
            var buildDate = DuplicatedFileFinder.AssemblyInfo.BuildDate;
            var oneYearAgo = DateTime.Now.AddYears(-1);
            var tomorrow = DateTime.Now.AddDays(1);

            // Assert
            Assert.True(buildDate >= oneYearAgo,
                $"Build date {buildDate:yyyy-MM-dd} should not be older than one year");
            Assert.True(buildDate <= tomorrow,
                $"Build date {buildDate:yyyy-MM-dd} should not be in the future");
        }
    }
}