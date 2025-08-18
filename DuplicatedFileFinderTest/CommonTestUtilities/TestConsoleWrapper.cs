using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicatedFileFinderTest.CommonTestUtilities
{
    public class TestConsoleWrapper : IConsoleWrapper
    {
        private readonly StringBuilder _output = new();

        public void Clear() { /* Não faz nada no teste */ }
        public void WriteLine(string value) => _output.AppendLine(value);
        public void WriteLine() => _output.AppendLine();
        public ConsoleColor ForegroundColor { get; set; }
        public void ResetColor() { /* Não faz nada no teste */ }

        public string GetOutput() => _output.ToString();
        public void ClearOutput() => _output.Clear();
    }
}
