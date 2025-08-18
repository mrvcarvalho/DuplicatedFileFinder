using LiteDB;

namespace DuplicatedFileFinder
{
    public class LiteDbPersistence
    {
        private readonly string _dbPath;

        public LiteDbPersistence(string dbPath = "duplicates.db")
        {
            _dbPath = dbPath;
        }

        public ObjectId SaveScanResults(string scannedDirectory, List<MyFileInfo> allFiles, List<EqualFiles> duplicates)
        {
            using var db = new LiteDatabase(_dbPath);
            var collection = db.GetCollection<ScanResultDocument>("scans");

            var scanDocument = new ScanResultDocument
            {
                ScanDate = DateTime.Now,
                Directory = scannedDirectory,
                TotalFiles = allFiles.Count,
                DuplicateGroups = duplicates.Count,
                TotalBytesWasted = duplicates.Sum(d => d.BytesWasted),
                Duplicates = duplicates.Select(d => new DuplicateGroupDocument
                {
                    Hash = d.HashFile,
                    FileCount = d.Count,
                    FileSize = d.Size,
                    BytesWasted = d.BytesWasted,
                    Files = d.File.Select(f => new FileInfoDocument
                    {
                        FullPath = f.FullPath,
                        Directory = f.Directory,
                        FileName = f.FileName,
                        Size = f.Size,
                        Hash = f.HashFile
                    }).ToList()
                }).ToList()
            };

            return collection.Insert(scanDocument);
        }

        public List<ScanResultDocument> GetAllScans()
        {
            using var db = new LiteDatabase(_dbPath);
            var collection = db.GetCollection<ScanResultDocument>("scans");

            return collection.Query()
                .OrderByDescending(x => x.ScanDate)
                .ToList();
        }

        public ScanResultDocument GetScanById(ObjectId id)
        {
            using var db = new LiteDatabase(_dbPath);
            var collection = db.GetCollection<ScanResultDocument>("scans");

            return collection.FindById(id);
        }

        public List<EqualFiles> LoadDuplicates(ObjectId scanId)
        {
            var scan = GetScanById(scanId);
            if (scan == null) return [];

            var duplicates = new List<EqualFiles>();

            foreach (var group in scan.Duplicates)
            {
                var equalFiles = new EqualFiles();
                foreach (var file in group.Files)
                {
                    // Reconstroir MyFileInfo (assumindo que o arquivo ainda existe)
                    if (File.Exists(file.FullPath))
                    {
                        var fileInfo = new MyFileInfo(file.FullPath);
                        equalFiles.AddOneMoreEqualFile(fileInfo);
                    }
                }

                if (equalFiles.Count > 0)
                {
                    duplicates.Add(equalFiles);
                }
            }

            return duplicates.OrderByDescending(ef => ef.BytesWasted).ToList();
        }

        // Consultas avançadas específicas do NoSQL
        public List<ScanResultDocument> FindScansByDirectory(string directory)
        {
            using var db = new LiteDatabase(_dbPath);
            var collection = db.GetCollection<ScanResultDocument>("scans");

            return collection.Query()
                .Where(x => x.Directory.Contains(directory))
                .OrderByDescending(x => x.ScanDate)
                .ToList();
        }

        public List<DuplicateGroupDocument> FindLargestWasteGroups(int limit = 10)
        {
            using var db = new LiteDatabase(_dbPath);
            var collection = db.GetCollection<ScanResultDocument>("scans");

            return collection.Query()
                .ToList()
                .SelectMany(scan => scan.Duplicates)
                .OrderByDescending(group => group.BytesWasted)
                .Take(limit)
                .ToList();
        }

        public Dictionary<string, int> GetFileExtensionStats()
        {
            using var db = new LiteDatabase(_dbPath);
            var collection = db.GetCollection<ScanResultDocument>("scans");

            return collection.Query()
                .ToList()
                .SelectMany(scan => scan.Duplicates)
                .SelectMany(group => group.Files)
                .GroupBy(file => Path.GetExtension(file.FileName).ToLower())
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}