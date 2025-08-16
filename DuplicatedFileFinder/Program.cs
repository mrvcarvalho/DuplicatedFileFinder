// 
// This code is a simple console application that searches for duplicate files in a specified folder.
// It retrieves all files from the folder, compares their names and sizes, and deletes duplicates.
// Make sure to test this code in a safe environment before running it on important files, as it will delete files permanently.
// Also, consider adding error handling and logging for a production-level application.
// 
// Note: The above code is a basic implementation for finding and deleting duplicate files based on their names and sizes.
// It does not handle exceptions or edge cases such as files with the same name but different content.
// For a more robust solution, consider using hash comparisons or file content checks.
// This code is a simple console application that searches for duplicate files in a specified folder.
// It retrieves all files from the folder, compares their names and sizes, and deletes duplicates.
// Make sure to test this code in a safe environment before running it on important files, as it will delete files permanently.
// Also, consider adding error handling and logging for a production-level application.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuplicatedFileFinder
{

    internal class Program
    {
        //private static string testingDirectory = @"D:\_DuplicationFileTest";
        private static string testingDirectory = @"F:\EBOOKS-DIVERSOS";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            string[] filesInDirectory = Directory.GetFiles(testingDirectory, "*.*", SearchOption.AllDirectories);

            var filesToCheck = new List<MyFileInfo>();
            foreach (var file in filesInDirectory)
            {
                try
                {
                    filesToCheck.Add(new MyFileInfo(file));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {file}: {ex.Message}");
                }
            }
            
            var finder = new DuplicatedFileFinder();

            var duplicates = finder.FindDuplicates(filesToCheck);
            foreach (var pair in duplicates)
            {
                Console.WriteLine(pair);
            }
        }

    }
}

