namespace DuplicatedFileFinder
{
    public class CommandLineOptions
    {
        public string Directory { get; set; } = string.Empty;
        public bool ShowHelp { get; set; }
        public bool ListScans { get; set; }
        public string LoadScanId { get; set; } = string.Empty;
        public long MinFileSize { get; set; } = 0;
        public int MaxResults { get; set; } = 50;
        public List<string>? FileExtensions { get; set; }
        public List<string>? ExcludePatterns { get; set; }
        public bool SilentMode { get; set; }
        public bool VerboseMode { get; set; }
        public bool NoPause { get; set; }
        public string ExportPath { get; set; } = string.Empty;
    }
}
