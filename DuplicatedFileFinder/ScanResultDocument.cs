using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicatedFileFinder
{
    public class ScanResultDocument
    {
        [BsonId]
        public ObjectId? Id { get; set; }
        public DateTime ScanDate { get; set; }
        public string Directory { get; set; } = string.Empty;
        public int TotalFiles { get; set; }
        public int DuplicateGroups { get; set; }
        public long TotalBytesWasted { get; set; }
        public List<DuplicateGroupDocument> Duplicates { get; set; } = [];

        public override string ToString()
        {
            return $"{ScanDate:yyyy-MM-dd HH:mm} - {Directory} - {DuplicateGroups} grupos, {Utils.FormatFileSize(TotalBytesWasted)} desperdiçados";
        }
    }

    public class DuplicateGroupDocument
    {
        public string Hash { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public long FileSize { get; set; }
        public long BytesWasted { get; set; }
        public List<FileInfoDocument> Files { get; set; } = [];
    }

    public class FileInfoDocument
    {
        public string FullPath { get; set; } = string.Empty;
        public string Directory { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public string Hash { get; set; } = string.Empty;
    }
}
