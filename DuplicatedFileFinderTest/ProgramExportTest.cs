using DuplicatedFileFinder;
using System.Reflection;
using Xunit.Abstractions;

namespace DuplicatedFileFinderTest
{
    public class ProgramExportTest : ProgramTestBase
    {
        public ProgramExportTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void ExportToJson_ShouldCreateValidJsonFile()
        {
            // Arrange
            var duplicates = CreateSampleDuplicatesWithRealFiles();
            var exportPath = Path.Combine(_testDirectory, "export.json");

            // Act
            InvokeExportToJson(duplicates, exportPath);

            // Assert
            Assert.True(File.Exists(exportPath));

            var jsonContent = File.ReadAllText(exportPath);
            Assert.Contains("Hash", jsonContent);
            Assert.Contains("Count", jsonContent);
            Assert.Contains("Files", jsonContent);

            // Verificar que contém dados reais dos arquivos criados
            Assert.Contains("duplicate1.txt", jsonContent);
            Assert.Contains("duplicate2.txt", jsonContent);
            Assert.Contains("duplicate3.txt", jsonContent);

            _output.WriteLine($"JSON Export: {jsonContent}");
        }

        [Fact]
        public void ExportToCsv_ShouldCreateValidCsvFile()
        {
            // Arrange
            var duplicates = CreateSampleDuplicatesWithRealFiles();
            var exportPath = Path.Combine(_testDirectory, "export.csv");

            // Act
            InvokeExportToCsv(duplicates, exportPath);

            // Assert
            Assert.True(File.Exists(exportPath));

            var csvContent = File.ReadAllText(exportPath);
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Assert.True(lines.Length > 1); // Header + data
            Assert.Contains("Hash,GroupSize,FileSize", lines[0]);

            // Verificar que tem dados dos arquivos reais
            Assert.Contains("duplicate1.txt", csvContent);
            Assert.Contains("duplicate2.txt", csvContent);

            _output.WriteLine($"CSV Export: {csvContent}");
        }

        [Fact]
        public void ExportToText_ShouldCreateValidTextFile()
        {
            // Arrange
            var duplicates = CreateSampleDuplicatesWithRealFiles();
            var exportPath = Path.Combine(_testDirectory, "export.txt");

            // Act
            InvokeExportToText(duplicates, exportPath);

            // Assert
            Assert.True(File.Exists(exportPath));

            var textContent = File.ReadAllText(exportPath);
            Assert.Contains("RELATÓRIO DE ARQUIVOS DUPLICADOS", textContent);
            Assert.Contains("Grupo -", textContent);
            Assert.Contains("duplicate1.txt", textContent);

            _output.WriteLine($"Text Export: {textContent}");
        }

        [Fact]
        public void CreateSampleDuplicatesWithRealFiles_ShouldCreateValidStructure()
        {
            // Act
            var duplicates = CreateSampleDuplicatesWithRealFiles();

            // Assert
            Assert.NotEmpty(duplicates);

            var firstGroup = duplicates.First();
            Assert.True(firstGroup.Count >= 2, "Should have at least 2 files for duplicates");
            Assert.True(firstGroup.BytesWasted > 0, "Should have bytes wasted");
            Assert.NotEmpty(firstGroup.HashFile);
            Assert.NotEmpty(firstGroup.FileName);
            Assert.True(firstGroup.Size > 0);

            _output.WriteLine($"First group: {firstGroup.Count} files, {firstGroup.BytesWasted} bytes wasted");
            _output.WriteLine($"Hash: {firstGroup.HashFile}");
            _output.WriteLine($"Files: {string.Join(", ", firstGroup.File.Select(f => f.FileName))}");
        }

        [Fact]
        public void CreateSampleDuplicatesWithRealFiles_ShouldWorkWithActualFiles()
        {
            // Act
            var duplicates = CreateSampleDuplicatesWithRealFiles();

            // Assert
            Assert.NotEmpty(duplicates);

            var group = duplicates.First();
            Assert.True(group.Count >= 2);
            Assert.True(group.BytesWasted > 0);
            Assert.All(group.File, f => Assert.True(File.Exists(f.FullPath)));

            // Verificar que todos os arquivos têm o mesmo hash
            var hashes = group.File.Select(f => f.HashFile).Distinct().ToList();
            Assert.Single(hashes); // Todos devem ter o mesmo hash

            _output.WriteLine($"Real files group: {group}");
        }

        [Fact]
        public void ExportToJson_WithMultipleGroups_ShouldExportAllGroups()
        {
            // Arrange
            var duplicates = CreateMultipleDuplicateGroups();
            var exportPath = Path.Combine(_testDirectory, "export_multiple.json");

            // Act
            InvokeExportToJson(duplicates, exportPath);

            // Assert
            Assert.True(File.Exists(exportPath));

            var jsonContent = File.ReadAllText(exportPath);

            // Verificar que ambos os grupos estão presentes
            Assert.Contains("group1_file", jsonContent);
            Assert.Contains("group2_file", jsonContent);

            _output.WriteLine($"Multiple Groups JSON Export: {jsonContent}");
        }

        [Fact]
        public void ExportToCsv_WithDifferentFileSizes_ShouldHandleCorrectly()
        {
            // Arrange
            var duplicates = CreateDuplicatesWithDifferentSizes();
            var exportPath = Path.Combine(_testDirectory, "export_sizes.csv");

            // Act
            InvokeExportToCsv(duplicates, exportPath);

            // Assert
            Assert.True(File.Exists(exportPath));

            var csvContent = File.ReadAllText(exportPath);
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            // Verificar que tem pelo menos header + dados
            Assert.True(lines.Length >= 2);

            _output.WriteLine($"Different Sizes CSV Export: {csvContent}");
        }

        // ===== MÉTODOS AUXILIARES =====

        private List<EqualFiles> CreateSampleDuplicatesWithRealFiles()
        {
            var content = "This is duplicate content for testing - same hash expected";

            CreateTestFile("duplicate1.txt", content);
            CreateTestFile("duplicate2.txt", content);
            CreateTestFile("subfolder/duplicate3.txt", content);

            var file1Path = Path.Combine(_testDirectory, "duplicate1.txt");
            var file2Path = Path.Combine(_testDirectory, "duplicate2.txt");
            var file3Path = Path.Combine(_testDirectory, "subfolder", "duplicate3.txt");

            var fileInfo1 = new MyFileInfo(file1Path);
            var fileInfo2 = new MyFileInfo(file2Path);
            var fileInfo3 = new MyFileInfo(file3Path);

            var duplicateGroup = new EqualFiles();
            duplicateGroup.AddOneMoreEqualFile(fileInfo1);
            duplicateGroup.AddOneMoreEqualFile(fileInfo2);
            duplicateGroup.AddOneMoreEqualFile(fileInfo3);

            return new List<EqualFiles> { duplicateGroup };
        }

        private List<EqualFiles> CreateMultipleDuplicateGroups()
        {
            var duplicates = new List<EqualFiles>();

            // Grupo 1: Arquivos de texto
            var content1 = "Content for group 1 - text files";
            CreateTestFile("group1_file1.txt", content1);
            CreateTestFile("group1_file2.txt", content1);

            var group1 = new EqualFiles();
            group1.AddOneMoreEqualFile(new MyFileInfo(Path.Combine(_testDirectory, "group1_file1.txt")));
            group1.AddOneMoreEqualFile(new MyFileInfo(Path.Combine(_testDirectory, "group1_file2.txt")));
            duplicates.Add(group1);

            // Grupo 2: Arquivos de imagem (simulados)
            var content2 = "Content for group 2 - image files simulation";
            CreateTestFile("group2_file1.jpg", content2);
            CreateTestFile("group2_file2.jpg", content2);

            var group2 = new EqualFiles();
            group2.AddOneMoreEqualFile(new MyFileInfo(Path.Combine(_testDirectory, "group2_file1.jpg")));
            group2.AddOneMoreEqualFile(new MyFileInfo(Path.Combine(_testDirectory, "group2_file2.jpg")));
            duplicates.Add(group2);

            return duplicates;
        }

        private List<EqualFiles> CreateDuplicatesWithDifferentSizes()
        {
            var duplicates = new List<EqualFiles>();

            // Grupo com arquivos pequenos
            var smallContent = "Small";
            CreateTestFile("small1.txt", smallContent);
            CreateTestFile("small2.txt", smallContent);

            var smallGroup = new EqualFiles();
            smallGroup.AddOneMoreEqualFile(new MyFileInfo(Path.Combine(_testDirectory, "small1.txt")));
            smallGroup.AddOneMoreEqualFile(new MyFileInfo(Path.Combine(_testDirectory, "small2.txt")));
            duplicates.Add(smallGroup);

            // Grupo com arquivos maiores
            var largeContent = new string('A', 1000); // 1000 caracteres
            CreateTestFile("large1.txt", largeContent);
            CreateTestFile("large2.txt", largeContent);

            var largeGroup = new EqualFiles();
            largeGroup.AddOneMoreEqualFile(new MyFileInfo(Path.Combine(_testDirectory, "large1.txt")));
            largeGroup.AddOneMoreEqualFile(new MyFileInfo(Path.Combine(_testDirectory, "large2.txt")));
            duplicates.Add(largeGroup);

            return duplicates;
        }

        // ===== MÉTODOS DE INVOCAÇÃO =====

        private void InvokeExportToJson(List<EqualFiles> duplicates, string path)
        {
            var programType = typeof(DuplicatedFileFinder.Program);
            var method = programType.GetMethod("ExportToJson",
                BindingFlags.NonPublic | BindingFlags.Static);
            method.Invoke(null, new object[] { duplicates, path });
        }

        private void InvokeExportToCsv(List<EqualFiles> duplicates, string path)
        {
            var programType = typeof(DuplicatedFileFinder.Program);
            var method = programType.GetMethod("ExportToCsv",
                BindingFlags.NonPublic | BindingFlags.Static);
            method.Invoke(null, new object[] { duplicates, path });
        }

        private void InvokeExportToText(List<EqualFiles> duplicates, string path)
        {
            var programType = typeof(DuplicatedFileFinder.Program);
            var method = programType.GetMethod("ExportToText",
                BindingFlags.NonPublic | BindingFlags.Static);
            method.Invoke(null, new object[] { duplicates, path });
        }
    }
}