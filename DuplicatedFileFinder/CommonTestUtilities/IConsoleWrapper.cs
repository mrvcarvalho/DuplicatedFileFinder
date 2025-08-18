using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicatedFileFinderTest
{
    public interface IConsoleWrapper
    {
        void Clear();
        void WriteLine(string value);
        void WriteLine();
        ConsoleColor ForegroundColor { get; set; }
        void ResetColor();
    }
}
