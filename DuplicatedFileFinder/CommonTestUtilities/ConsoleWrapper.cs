using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicatedFileFinderTest.CommonTestUtilities
{
    public class ConsoleWrapper : IConsoleWrapper
    {
        public void Clear() => Console.Clear();
        public void WriteLine(string value) => Console.WriteLine(value);
        public void WriteLine() => Console.WriteLine();
        public ConsoleColor ForegroundColor
        {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }
        public void ResetColor() => Console.ResetColor();
    }
}
