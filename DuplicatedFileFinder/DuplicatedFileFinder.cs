namespace DuplicatedFileFinder
{
    public class DuplicatedFileFinder
    {
        public DuplicatedFileFinder()
        {
            // Constructor logic if needed
        }

        public List<EqualFiles> FindDuplicates(List<MyFileInfo> files)
        {
            if (files == null || files.Count == 0)
            {
                throw new ArgumentException("File list cannot be null or empty", nameof(files));
            }
            if (files.Count == 1)
            {
                return []; // No duplicates possible with a single file
            }

            var duplicateGroups = files
                .GroupBy(file => file.HashFile)
                .Where(group => group.Count() > 1)
                .Select(group =>
                {
                    var equalFiles = new EqualFiles();
                    foreach (var file in group)
                    {
                        equalFiles.AddOneMoreEqualFile(file);
                    }
                    return equalFiles;
                })
                .OrderByDescending(ef => ef.BytesWasted)
                .ToList();

            return duplicateGroups;
        }
    }
}
