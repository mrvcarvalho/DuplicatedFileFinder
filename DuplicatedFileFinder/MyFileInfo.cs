using System.Security.Cryptography;

namespace DuplicatedFileFinder
{
    public class MyFileInfo
    {
        public string Directory{ get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }
        public string HashFile { get; set; } = string.Empty;
        public string FullPath;

        public MyFileInfo(string fullFileName)
        {
            if (string.IsNullOrEmpty(fullFileName))
            {
                throw new ArgumentException("File name cannot be null or empty", nameof(fullFileName));
            }
            if (!File.Exists(fullFileName))
            {
                throw new FileNotFoundException($"File not found: {fullFileName}");
            }
            FullPath = fullFileName;
            Directory = Path.GetDirectoryName(fullFileName) ?? string.Empty;
            FileName = Path.GetFileName(fullFileName);
            Size = new FileInfo(fullFileName).Length;
            HashFile = CalculateHash();
        }

        public string CalculateHash()
        {
            /*
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(FullPath))
            {
                var hashBytes = md5.ComputeHash(stream);
                return Convert.ToHexString(hashBytes);
            }
            */
            try
            {
                using (var sha256 = SHA256.Create())
                using (var stream = File.OpenRead(FullPath))
                {
                    var hashBytes = sha256.ComputeHash(stream);
                    return Convert.ToHexString(hashBytes);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error calculating hash for file: {FullPath}", ex);
            }
        }

        public override string ToString()
        {
            return $"{FileName} ({Size} bytes)";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FileName, Size);
        }
        public static bool operator ==(MyFileInfo left, MyFileInfo right)
        {
            return Equals(left, right);
        }
        public static bool Equals(MyFileInfo left, MyFileInfo right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
            return left.HashFile == right.HashFile && left.Size == right.Size;
        }

        public static bool operator !=(MyFileInfo left, MyFileInfo right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object? obj)
        {
            if (obj is MyFileInfo other)
            {
                return HashFile == other.HashFile && Size == other.Size;
            }
            return false;
        }
    }
}
