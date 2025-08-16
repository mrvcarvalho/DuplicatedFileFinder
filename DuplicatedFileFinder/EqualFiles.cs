namespace DuplicatedFileFinder
{
    public class EqualFiles
    {
        public int Count => File.Count;
        public string FullPath => File.FirstOrDefault()?.FullPath ?? string.Empty;
        public string Directory => File.FirstOrDefault()?.Directory ?? string.Empty;
        public string FileName => File.FirstOrDefault()?.FileName ?? string.Empty;
        public long Size => File.FirstOrDefault()?.Size ?? 0;
        public string HashFile => File.FirstOrDefault()?.HashFile ?? string.Empty;

        public long BytesWasted
        {
            get
            {
                if (File.Count <= 1)    // 0 or 1 file means no duplicates
                {
                    return 0;
                }
                return File.Sum(f => f.Size) - File.First().Size;
            }
        }
        // List to hold all files that are considered equal
        // This will be used to store files that have the same hash

        public List<MyFileInfo> File { get; set; } = [];

        public EqualFiles(){}

        public EqualFiles(MyFileInfo file)
        {
            AddOneMoreEqualFile(file);
        }

        public void AddOneMoreEqualFile(MyFileInfo file)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file), "File cannot be null");
            }
            if (File.Any() && File.Any(f => !f.Equals(file)))
            {
                throw new InvalidOperationException("Ivalid attempt to insert a diferent File.");
            }
            File.Add(file);
        }

        public override string ToString()
        {
            if (File.Count == 0)
            {
                return "EqualFiles: No files found.";
            }

            var firstFile = File.First();

            if (File.Count == 1)
            {
                return $"EqualFiles: Single file" +
                       $"\n{firstFile.FullPath}" +
                       $"\n  Size: {FormatFileSize(firstFile.Size)}" +
                       $"\n  Hash: {firstFile.HashFile}" +
                       $"\n  BytesWasted : {FormatFileSize(BytesWasted)}";
            }

            // If there are multiple files, return the list of files
            var fileList = string.Join("\n", File.Select(f => $"  {f.FullPath}"));

            return $"EqualFiles: {File.Count} duplicate files found" +
                   $"\n{fileList}" +
                   $"\n  Size: {FormatFileSize(firstFile.Size)} each" +
                   $"\n  Hash: {firstFile.HashFile}" +
                   $"\n  BytesWasted : {FormatFileSize(BytesWasted)}";
        }

        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "bytes", "Kb", "Mb", "Gb", "Tb" };
            int counter = 0;
            decimal number = bytes;

            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }

            return $"{number:n1} {suffixes[counter]}";
        }
    }
}
