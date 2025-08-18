namespace DuplicatedFileFinder
{
    internal static class Utils
    {

        public static string FormatFileSize(long bytes)
        {
            string[] suffixes = ["bytes", "Kb", "Mb", "Gb", "Tb"];
            int counter = 0;
            decimal number = bytes;

            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }

            return $"{number:n1} {suffixes[counter]}";
        }

        public static string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
                return input;

            return input[..(maxLength - 3)] + "...";
        }
    }
}