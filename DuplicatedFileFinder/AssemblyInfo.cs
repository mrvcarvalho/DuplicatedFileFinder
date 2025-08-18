using System.Diagnostics;
using System.Reflection;


namespace DuplicatedFileFinder
{
    public static class AssemblyInfo
    {
        private static Assembly Assembly => Assembly.GetExecutingAssembly();
        private static FileVersionInfo FileVersionInfo => FileVersionInfo.GetVersionInfo(Assembly.Location);

        public static string Version => Assembly.GetName().Version?.ToString() ?? "Unknown";
        public static string FileVersion => FileVersionInfo.FileVersion ?? "Unknown";
        public static string ProductVersion => FileVersionInfo.ProductVersion ?? "Unknown";
        public static string ProductTitle => FileVersionInfo.ProductName ?? "DuplicatedFileFinder";
        public static string Description => FileVersionInfo.FileDescription ?? "";


        public static string Company
        {
            get
            {
                var assembly = Assembly.GetEntryAssembly();
                var attribute = assembly?.GetCustomAttribute<AssemblyCompanyAttribute>();
                return attribute?.Company ?? "Company não encontrada";
            }
        }

        private static Assembly CurrentAssembly => Assembly.GetExecutingAssembly();
        public static string Copyright
        {
            get
            {
                var attribute = CurrentAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
                return attribute?.Copyright ?? "Copyright não encontrado";
            }
        }

        public static string InformationalVersion
        {
            get
            {
                var attribute = Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                return attribute?.InformationalVersion ?? Version;
            }
        }

        public static DateTime BuildDate
        {
            get
            {
                // Tentar obter da metadata primeiro
                var buildDateAttr = Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                    .FirstOrDefault(a => a.Key == "BuildDate");

                if (buildDateAttr != null && DateTime.TryParse(buildDateAttr.Value, out var buildDate))
                {
                    return buildDate;
                }

                // Fallback: data do arquivo
                try
                {
                    return File.GetLastWriteTime(Assembly.Location);
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
        }

        public static string BuildMachine
        {
            get
            {
                var attr = Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                    .FirstOrDefault(a => a.Key == "BuildMachine");
                return attr?.Value ?? Environment.MachineName;
            }
        }

        public static string BuildUser
        {
            get
            {
                var attr = Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                    .FirstOrDefault(a => a.Key == "BuildUser");
                return attr?.Value ?? Environment.UserName;
            }
        }

        // Propriedades formatadas
        public static string ShortVersion => $"v{Version}";

        public static string MediumVersion => $"v{Version} (Build {FileVersion})";

        public static string FullVersionInfo =>
            $"{ProductTitle} {InformationalVersion}\n" +
            $"Build: {BuildDate:yyyy-MM-dd HH:mm:ss} on {BuildMachine}";

        public static string HeaderVersion =>
            $"{ShortVersion} - Build {FileVersion.Split('.').LastOrDefault()}";

        public static void ShowVersion(TextWriter? writer = null)
        {
            writer ??= Console.Out;

            writer.WriteLine($"Product          : {AssemblyInfo.ProductTitle}");
            writer.WriteLine($"Assembly Version : {AssemblyInfo.Version}");
            writer.WriteLine($"File Version     : {AssemblyInfo.FileVersion}");
            writer.WriteLine($"Product Version  : {AssemblyInfo.ProductVersion}");
            writer.WriteLine($"Build Date       : {AssemblyInfo.BuildDate:yyyy-MM-dd HH:mm:ss}");
            writer.WriteLine($"Build Machine    : {AssemblyInfo.BuildMachine}");

            if (!string.IsNullOrEmpty(AssemblyInfo.Copyright))
            {
                writer.WriteLine(AssemblyInfo.Copyright);
            }
        }

        public static void DebugAssemblyInfo(TextWriter? writer = null)
        {
            writer ??= Console.Out;

            writer.WriteLine();
            writer.WriteLine("=== Assembly Information: DebugAssemblyInfo() ===");
            var assembly = Assembly.GetExecutingAssembly();

            writer.WriteLine("=== INFORMAÇÕES DO ASSEMBLY ===");
            writer.WriteLine($"Nome        : {assembly.GetName().Name}");
            writer.WriteLine($"Versão      : {assembly.GetName().Version}");
            writer.WriteLine($"Local       : {assembly.Location}");

            writer.WriteLine("\n=== ATRIBUTOS PADRÃO ===");
            writer.WriteLine($"Copyright   : {AssemblyInfo.Copyright}");
            writer.WriteLine($"Title       : {AssemblyInfo.ProductTitle}");
            writer.WriteLine($"Description : {AssemblyInfo.Description}");
            writer.WriteLine($"Company     : {AssemblyInfo.Company}");
            writer.WriteLine($"Product     : {AssemblyInfo.ProductTitle}");
            writer.WriteLine($"Version     : {AssemblyInfo.Version}");
            writer.WriteLine($"FileVersion : {AssemblyInfo.FileVersion}");

            writer.WriteLine("\n=== METADADOS CUSTOMIZADOS ===");
            writer.WriteLine($"BuildDate   : {AssemblyInfo.BuildDate}");
            writer.WriteLine($"BuildMachine: {AssemblyInfo.BuildMachine}");
            writer.WriteLine($"BuildUser   : {AssemblyInfo.BuildUser}");

            writer.WriteLine("\n=== TODOS OS ATRIBUTOS ===");
            var attributes = assembly.GetCustomAttributes();
            foreach (var attr in attributes.OrderBy(a => a.GetType().Name))
            {
                writer.WriteLine($"{attr.GetType().Name}: {attr}");
            }
            writer.WriteLine("=== Assembly Information: DebugAssemblyInfo() ===");
            writer.WriteLine();
        }
    }
}