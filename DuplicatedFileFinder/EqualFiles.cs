namespace DuplicatedFileFinder
{
    public class EqualFiles
    {
        public EqualFiles() { }

        public EqualFiles(MyFileInfo file)
        {
            AddOneMoreEqualFile(file);
        }

        public int Count => EqualFileList.Count;
        public string FullPath => EqualFileList.FirstOrDefault()?.FullPath ?? string.Empty;
        public string Directory => EqualFileList.FirstOrDefault()?.Directory ?? string.Empty;
        public string FileName => EqualFileList.FirstOrDefault()?.FileName ?? string.Empty;
        public long Size => EqualFileList.FirstOrDefault()?.Size ?? 0;
        public string HashFile => EqualFileList.FirstOrDefault()?.HashFile ?? string.Empty;

        public long BytesWasted
        {
            get
            {
                if (EqualFileList.Count <= 1)    // 0 or 1 file means no duplicates
                {
                    return 0;
                }
                return EqualFileList.Sum(f => f.Size) - EqualFileList.First().Size;
            }
        }
        // List to hold all files that are considered equal
        // This will be used to store files that have the same hash

        public List<MyFileInfo> EqualFileList { get; set; } = [];

        public void AddOneMoreEqualFile(MyFileInfo file)
        {
            Logger.LogMethodEntry(nameof(AddOneMoreEqualFile), file?.FileName ?? "null");

            if (file is null)
            {
                var ex = new ArgumentNullException(nameof(file), "File cannot be null");
                Logger.Error($"Tentativa de adicionar arquivo nulo ao grupo", ex);
                throw ex;
            }
            
            if (file.Size == 0)
            {
                Logger.Error($"Tentativa de adicionar arquivo com Size==0 grupo {nameof(file)} : {file.ToString()}");
                return;
            }

            if (EqualFileList.Count != 0 && EqualFileList.Any(f => !f.Equals(file)))
            {
                var expectedHash = EqualFileList[0].HashFile;
                var actualHash = file.HashFile;
                var message = $"Tentativa de adicionar arquivo com hash diferente. Esperado: {expectedHash}, Recebido: {actualHash}";

                Logger.Error(message);
                Logger.Debug($"Arquivo rejeitado: {file.FileName}");
                Logger.Debug($"Arquivos existentes no grupo: {string.Join(", ", EqualFileList.Select(f => f.FileName))}");

                var ex = new InvalidOperationException("Invalid attempt to insert a diferent File.");
                Logger.LogException("AddOneMoreEqualFile", ex);
                throw ex;
            }

            EqualFileList.Add(file);

            Logger.Info($"Arquivo adicionado ao grupo: {file.FileName} (Hash: {file.HashFile[..8]}...)");
            Logger.Debug($"Grupo agora possui {EqualFileList.Count} arquivos");
            Logger.LogMethodExit(nameof(AddOneMoreEqualFile), $"Group count: {EqualFileList.Count}");
        }

        public override string ToString()
        {
            if (EqualFileList.Count == 0)
            {
                return "EqualFiles: No files found.";
            }

            var firstFile = EqualFileList.First();

            if (EqualFileList.Count == 1)
            {
                return $"EqualFiles: Single file" +
                       $"\n{firstFile.FullPath}" +
                       $"\n  Size: {Utils.FormatFileSize(firstFile.Size)}" +
                       $"\n  Hash: {firstFile.HashFile}" +
                       $"\n  BytesWasted : {Utils.FormatFileSize(BytesWasted)}";
            }

            // If there are multiple files, return the list of files
            var fileList = string.Join("\n", EqualFileList.Select(f => $"  {f.FullPath}"));

            return $"EqualFiles: {EqualFileList.Count} duplicate files found" +
                   $"\n{fileList}" +
                   $"\n  Size: {Utils.FormatFileSize(firstFile.Size)} each" +
                   $"\n  Hash: {firstFile.HashFile}" +
                   $"\n  BytesWasted : {Utils.FormatFileSize(BytesWasted)}";
        }
    }
}
