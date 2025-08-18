using Xunit.Abstractions;

namespace DuplicatedFileFinderTest
{
    public class ProgramTestBase : IDisposable
    {
        protected readonly ITestOutputHelper _output;
        protected readonly string _testDirectory;
        protected readonly StringWriter _consoleOutput;
        protected readonly TextWriter _originalConsoleOut;

        public ProgramTestBase(ITestOutputHelper output)
        {
            _output = output;
            _testDirectory = Path.Combine(Path.GetTempPath(), $"DFFTest_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testDirectory);

            // Capturar saída do console
            _originalConsoleOut = Console.Out;
            _consoleOutput = new StringWriter();
            Console.SetOut(_consoleOutput);
        }

        protected void CreateTestFile(string relativePath, string content = "test content", long? size = null)
        {
            var fullPath = Path.Combine(_testDirectory, relativePath);
            var directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (size.HasValue)
            {
                // Criar arquivo com tamanho específico
                using var fs = new FileStream(fullPath, FileMode.Create);
                var buffer = new byte[1024];
                var remaining = size.Value;
                while (remaining > 0)
                {
                    var toWrite = (int)Math.Min(buffer.Length, remaining);
                    fs.Write(buffer, 0, toWrite);
                    remaining -= toWrite;
                }
            }
            else
            {
                File.WriteAllText(fullPath, content);
            }
        }

        protected void CreateDuplicateFiles(string baseName, string content, int count)
        {
            for (int i = 0; i < count; i++)
            {
                CreateTestFile($"{baseName}_{i}.txt", content);
            }
        }

        protected string GetConsoleOutput()
        {
            return _consoleOutput.ToString();
        }

        protected void ClearConsoleOutput()
        {
            _consoleOutput.GetStringBuilder().Clear();
        }

        public void Dispose()
        {
            Console.SetOut(_originalConsoleOut);
            _consoleOutput?.Dispose();

            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    Directory.Delete(_testDirectory, true);
                }
                catch
                {
                    // Ignorar erros de limpeza
                }
            }
        }
    }
}