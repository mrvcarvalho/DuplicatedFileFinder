
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
using DuplicatedFileFinderTest;
using DuplicatedFileFinderTest.CommonTestUtilities;
using System.Diagnostics;

namespace DuplicatedFileFinder
{
    public class Program
    {
        private static readonly LiteDbPersistence _persistence = new("duplicates.db");

        public static void Main(string[] args)
        {
            try
            {
                ShowHeader();

                var options = ParseArguments(args);

                if (options.ShowHelp)
                {
                    ShowHelp();
                    return;
                }

                if (options.ListScans)
                {
                    ShowPreviousScans();
                    return;
                }

                if (!string.IsNullOrEmpty(options.LoadScanId))
                {
                    LoadAndDisplayScan(options.LoadScanId);
                    return;
                }

                if (string.IsNullOrEmpty(options.Directory))
                {
                    Console.WriteLine("[ERROR]  Diretório não especificado. Use --help para ver as opções.");
                    return;
                }

                if (!Directory.Exists(options.Directory))
                {
                    Console.WriteLine($"[ERROR]  Diretório não encontrado: {options.Directory}");
                    return;
                }

                PerformDuplicateScan(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]  Erro fatal: {ex.Message}");
                if (args.Contains("--debug"))
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
            finally
            {
                if (!args.Contains("--no-pause"))
                {
                    Console.WriteLine("\n[RELOAD] Pressione qualquer tecla para sair...");
                    Console.ReadKey();
                }
            }
        }

        private static IConsoleWrapper _console = new ConsoleWrapper();

        // Método para testes definirem o wrapper
        internal static void SetConsoleWrapper(IConsoleWrapper console)
        {
            _console = console;
        }

        private static void ShowHeader()
        {
            _console.Clear();
            _console.ForegroundColor = ConsoleColor.Cyan;

            _console.WriteLine( "╔══════════════════════════════════════════════════════════════╗");
            _console.WriteLine( "║                   DUPLICATED FILE FINDER                     ║");
            _console.WriteLine($"║                           {AssemblyInfo.ShortVersion,-10}                         ║");
            _console.WriteLine( "╚══════════════════════════════════════════════════════════════╝");

            _console.ResetColor();
            _console.ForegroundColor = ConsoleColor.Gray;
            _console.WriteLine($"Build: {AssemblyInfo.FileVersion} | {AssemblyInfo.BuildDate:yyyy-MM-dd HH:mm}");
            if (!string.IsNullOrEmpty(AssemblyInfo.Copyright))
            {
                _console.WriteLine(AssemblyInfo.Copyright);
            }
            _console.ResetColor();
            _console.WriteLine();
        }

        private static CommandLineOptions ParseArguments(string[] args)
        {
            var options = new CommandLineOptions();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--version":
                    case "-v":
                        AssemblyInfo.ShowVersion();
                        Environment.Exit(0);
                        break;

                    case "--help":
                    case "-h":
                        options.ShowHelp = true;
                        break;

                    case "--directory":
                    case "-d":
                        if (i + 1 < args.Length)
                            options.Directory = args[++i];
                        break;

                    case "--list":
                    case "-l":
                        options.ListScans = true;
                        break;

                    case "--load":
                        if (i + 1 < args.Length)
                            options.LoadScanId = args[++i];
                        break;

                    case "--min-size":
                        if (i + 1 < args.Length && long.TryParse(args[++i], out long minSize))
                            options.MinFileSize = minSize;
                        break;

                    case "--max-results":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int maxResults))
                            options.MaxResults = maxResults;
                        break;

                    case "--extensions":
                    case "-e":
                        if (i + 1 < args.Length)
                            options.FileExtensions = args[++i].Split(',').Select(e => e.Trim().ToLower()).ToList();
                        break;

                    case "--exclude":
                        if (i + 1 < args.Length)
                            options.ExcludePatterns = args[++i].Split(',').Select(p => p.Trim()).ToList();
                        break;

                    case "--silent":
                    case "-s":
                        options.SilentMode = true;
                        break;

                    case "--verbose":
                        options.VerboseMode = true;
                        break;

                    case "--no-pause":
                        options.NoPause = true;
                        break;

                    case "--export":
                        if (i + 1 < args.Length)
                            options.ExportPath = args[++i];
                        break;

                    default:
                        // Se não tem prefixo --, assume que é o diretório
                        if (!args[i].StartsWith('-') && string.IsNullOrEmpty(options.Directory))
                            options.Directory = args[i];
                        break;
                }
            }

            return options;
        }

        private static void ShowHelp()
        {
            Console.WriteLine();
            Console.WriteLine($"[HELP]    {AssemblyInfo.ProductTitle} {AssemblyInfo.ShortVersion}");
            Console.WriteLine(AssemblyInfo.Description);
            Console.WriteLine();
            Console.WriteLine($"[HELP] USO:");
            Console.WriteLine( " DuplicatedFileFinder.exe [OPÇÕES] [DIRETÓRIO]");
            Console.WriteLine();
            Console.WriteLine("[DIR]     OPÇÕES DE SCAN:");
            Console.WriteLine("  -d, --directory <path>     Diretório para escanear");
            Console.WriteLine("  -e, --extensions <list>    Extensões de arquivo (ex: jpg,png,pdf)");
            Console.WriteLine("  --exclude <patterns>       Padrões para excluir (ex: temp,cache)");
            Console.WriteLine("  --min-size <bytes>         Tamanho mínimo do arquivo em bytes");
            Console.WriteLine();
            Console.WriteLine("[STATS]   OPÇÕES DE EXIBIÇÃO:");
            Console.WriteLine("  --max-results <number>     Máximo de grupos a exibir (padrão: 50)");
            Console.WriteLine("  -s, --silent               Modo silencioso (menos output)");
            Console.WriteLine("  --verbose                  Modo verboso (mais detalhes)");
            Console.WriteLine("  --no-pause                 Não pausar no final");
            Console.WriteLine();
            Console.WriteLine("[DISK]    OPÇÕES DE DADOS:");
            Console.WriteLine("  -l, --list                 Listar scans anteriores");
            Console.WriteLine("  --load <id>                Carregar scan específico");
            Console.WriteLine("  --export <path>            Exportar resultados para arquivo");
            Console.WriteLine();
            Console.WriteLine("[INFO]    OUTRAS:");
            Console.WriteLine("  -h, --help                 Mostrar esta ajuda");
            Console.WriteLine("  -v, --version              Mostrar informações de versão");
            Console.WriteLine();
            Console.WriteLine("[EXAMPLE] EXEMPLOS:");
            Console.WriteLine("  DuplicatedFileFinder.exe \"C:\\Meus Documentos\\\"");
            Console.WriteLine("  DuplicatedFileFinder.exe -d \"D:\\Fotos\" -e jpg,png --min-size 1024");
            Console.WriteLine("  DuplicatedFileFinder.exe --list");
            Console.WriteLine("  DuplicatedFileFinder.exe --load 12345");
        }

        private static void ShowPreviousScans()
        {
            Console.WriteLine("[STATS] SCANS ANTERIORES:");
            Console.WriteLine("".PadRight(80, '='));

            try
            {
                var scans = _persistence.GetAllScans();

                if (!scans.Any())
                {
                    Console.WriteLine("[EMPTY]  Nenhum scan encontrado no banco de dados.");
                    return;
                }

                Console.WriteLine($"{"ID",-8} {"Data/Hora",-20} {"Diretório",-40} {"Grupos",-8} {"Desperdício",-15}");
                Console.WriteLine("".PadRight(80, '-'));

                foreach (var scan in scans.Take(20)) // Mostrar apenas os 20 mais recentes
                {
                    var id = scan.Id.ToString()[^6..]; // Últimos 6 caracteres do ID
                    Console.WriteLine($"{id,-8} {scan.ScanDate:yyyy-MM-dd HH:mm}   {Utils.TruncateString(scan.Directory, 38),-40} {scan.DuplicateGroups,-8} {Utils.FormatFileSize(scan.TotalBytesWasted),-15}");
                }

                if (scans.Count > 20)
                {
                    Console.WriteLine($"\n... e mais {scans.Count - 20} scans. Use --load <id> para carregar um específico.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]  Erro ao carregar scans: {ex.Message}");
            }
        }

        private static void LoadAndDisplayScan(string scanId)
        {
            try
            {
                Console.WriteLine($"[SEARCH] Carregando scan: {scanId}");

                // Tentar converter para ObjectId do LiteDB
                var scans = _persistence.GetAllScans();
                var scan = scans.FirstOrDefault(s => s.Id.ToString().EndsWith(scanId));

                if (scan == null)
                {
                    Console.WriteLine($"[ERROR]  Scan não encontrado: {scanId}");
                    return;
                }

                Console.WriteLine($"[DATE]   Data: {scan.ScanDate:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"[DIR]    Diretório: {scan.Directory}");
                Console.WriteLine($"[STATUS] Total de arquivos: {scan.TotalFiles:N0}");
                Console.WriteLine($"[SEARCH] Grupos de duplicatas: {scan.DuplicateGroups:N0}");
                Console.WriteLine($"[DISK]   Espaço desperdiçado: {Utils.FormatFileSize(scan.TotalBytesWasted)}");
                Console.WriteLine();

                var duplicates = _persistence.LoadDuplicates(scan.Id);
                DisplayDuplicates(duplicates, new CommandLineOptions { MaxResults = 50 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Erro ao carregar scan: {ex.Message}");
            }
        }

        private static void PerformDuplicateScan(CommandLineOptions options)
        {
            var stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"[SCAN]   INICIANDO SCAN");
            Console.WriteLine($"[DIR]    Diretório: {options.Directory}");

            if (options.FileExtensions?.Any() == true)
                Console.WriteLine($"[FILE]   Extensões: {string.Join(", ", options.FileExtensions)}");

            if (options.MinFileSize > 0)
                Console.WriteLine($"[SIZE]   Tamanho mínimo: {Utils.FormatFileSize(options.MinFileSize)}");

            Console.WriteLine("".PadRight(80, '='));

            try
            {
                // 1. Descobrir arquivos
                var allFiles = DiscoverFiles(options);
                if (!allFiles.Any())
                {
                    Console.WriteLine("[EMPTY] Nenhum arquivo encontrado para processar.");
                    return;
                }

                // 2. Processar arquivos
                var filesToCheck = ProcessFiles(allFiles, options);
                if (filesToCheck.Count == 0)
                {
                    Console.WriteLine("[EMPTY]  Nenhum arquivo válido para verificar duplicatas.");
                    return;
                }

                // 3. Encontrar duplicatas
                var finder = new DuplicatedFileFinder();
                var duplicates = finder.FindDuplicates(filesToCheck);

                stopwatch.Stop();

                // 4. Salvar resultados
                var scanId = _persistence.SaveScanResults(options.Directory, filesToCheck, duplicates);

                // 5. Exibir resultados
                ShowScanResults(duplicates, filesToCheck.Count, stopwatch.Elapsed, scanId);
                DisplayDuplicates(duplicates, options);

                // 6. Exportar se solicitado
                if (!string.IsNullOrEmpty(options.ExportPath))
                {
                    ExportResults(duplicates, options.ExportPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]  Erro durante o scan: {ex.Message}");
                throw;
            }
        }

        private static string[] DiscoverFiles(CommandLineOptions options)
        {
            Console.WriteLine("[SEARCH] Descobrindo arquivos...");

            var searchPattern = "*.*";
            var allFiles = Directory.GetFiles(options.Directory, searchPattern, SearchOption.AllDirectories);

            // Filtrar por extensões
            if (options.FileExtensions?.Any() == true)
            {
                allFiles = allFiles.Where(f =>
                    options.FileExtensions.Contains(Path.GetExtension(f).ToLower().TrimStart('.'))).ToArray();
            }

            // Filtrar por padrões de exclusão - APENAS na parte após o diretório base
            if (options.ExcludePatterns?.Any() == true)
            {
                allFiles = allFiles.Where(f =>
                {
                    // Obter a parte do caminho após o diretório base
                    var relativePath = GetRelativePathFromBase(f, options.Directory);

                    // Verificar se algum padrão de exclusão está presente no caminho relativo
                    return !options.ExcludePatterns.Any(pattern =>
                        relativePath.Contains(pattern, StringComparison.OrdinalIgnoreCase));
                }).ToArray();
            }

            // Filtrar por tamanho mínimo
            if (options.MinFileSize > 0)
            {
                allFiles = allFiles.Where(f =>
                {
                    try { return new FileInfo(f).Length >= options.MinFileSize; }
                    catch { return false; }
                }).ToArray();
            }

            Console.WriteLine($"[STATS]  Encontrados {allFiles.Length:N0} arquivos para processar");
            return allFiles;
        }

        /// <summary>
        /// Obtém o caminho relativo de um arquivo em relação ao diretório base
        /// </summary>
        /// <param name="fullPath">Caminho completo do arquivo</param>
        /// <param name="basePath">Diretório base</param>
        /// <returns>Caminho relativo após o diretório base</returns>
        private static string GetRelativePathFromBase(string fullPath, string basePath)
        {
            // Normalizar os caminhos para comparação
            var normalizedFullPath = Path.GetFullPath(fullPath);
            var normalizedBasePath = Path.GetFullPath(basePath);

            // Garantir que o basePath termine com separador
            if (!normalizedBasePath.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                !normalizedBasePath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                normalizedBasePath += Path.DirectorySeparatorChar;
            }

            // Verificar se o arquivo está dentro do diretório base
            if (normalizedFullPath.StartsWith(normalizedBasePath, StringComparison.OrdinalIgnoreCase))
            {
                // Retornar apenas a parte após o diretório base
                return normalizedFullPath.Substring(normalizedBasePath.Length);
            }

            // Se não estiver dentro do diretório base, retornar o caminho completo
            return normalizedFullPath;
        }

        private static List<MyFileInfo> ProcessFiles(string[] files, CommandLineOptions options)
        {
            Console.WriteLine("[HASH]   Calculando hashes dos arquivos...");

            var filesToCheck = new List<MyFileInfo>();
            var processed = 0;
            var errors = 0;

            foreach (var file in files)
            {
                try
                {
                    filesToCheck.Add(new MyFileInfo(file));
                    processed++;

                    if (!options.SilentMode && processed % 100 == 0)
                    {
                        var percentage = (processed * 100.0) / files.Length;
                        Console.Write($"\r[WAIT]   Processados: {processed:N0}/{files.Length:N0} ({percentage:F1}%)");
                    }
                }
                catch (Exception ex)
                {
                    errors++;
                    if (options.VerboseMode)
                    {
                        Console.WriteLine($"\n[ERROR]  Erro processando {file}: {ex.Message}");
                    }
                }
            }

            if (!options.SilentMode)
            {
                Console.WriteLine($"\r[OK]     Processamento concluído: {processed:N0} sucessos, {errors:N0} erros");
            }

            return filesToCheck;
        }

        private static void ShowScanResults(List<EqualFiles> duplicates, int totalFiles, TimeSpan elapsed, LiteDB.ObjectId scanId)
        {
            Console.WriteLine();
            Console.WriteLine("[STATS]   RESULTADOS DO SCAN:");
            Console.WriteLine("".PadRight(80, '='));
            Console.WriteLine($"[TIME]   Tempo de processamento: {elapsed:mm:ss}");
            Console.WriteLine($"[DIR]    Total de arquivos analisados: {totalFiles:N0}");
            Console.WriteLine($"[SEARCH] Grupos de duplicatas encontrados: {duplicates.Count:N0}");
            Console.WriteLine($"[FILE]   Total de arquivos duplicados: {duplicates.Sum(d => d.Count):N0}");
            Console.WriteLine($"[DISK]   Espaço desperdiçado: {Utils.FormatFileSize(duplicates.Sum(d => d.BytesWasted))}");
            Console.WriteLine($"[ID]     ID do scan: {scanId.ToString()[^6..]}");
            Console.WriteLine();
        }

        private static void DisplayDuplicates(List<EqualFiles> duplicates, CommandLineOptions options)
        {
            if (duplicates.Count == 0)
            {
                Console.WriteLine("[SUCCESS] Nenhuma duplicata encontrada!");
                return;
            }

            Console.WriteLine("[SEARCH] DUPLICATAS ENCONTRADAS:");
            Console.WriteLine("".PadRight(80, '='));

            var maxToShow = Math.Min(duplicates.Count, options.MaxResults);

            for (int i = 0; i < maxToShow; i++)
            {
                var duplicate = duplicates[i];
                Console.WriteLine($"\n[LIST] Grupo {i + 1}/{duplicates.Count} - {duplicate.Count} arquivos - {Utils.FormatFileSize(duplicate.BytesWasted)} desperdiçados");
                Console.WriteLine($"[SIZE] Tamanho: {Utils.FormatFileSize(duplicate.Size)} cada");
                Console.WriteLine($"[HASH] Hash: {duplicate.HashFile[..16]}...");

                if (options.VerboseMode)
                {
                    foreach (var file in duplicate.File)
                    {
                        Console.WriteLine($"   [FILE] {file.FullPath}");
                    }
                }
                else
                {
                    // Mostrar apenas os primeiros arquivos
                    var filesToShow = Math.Min(3, duplicate.File.Count);
                    for (int j = 0; j < filesToShow; j++)
                    {
                        Console.WriteLine($"   [FILE] {duplicate.File[j].FullPath}");
                    }
                    if (duplicate.File.Count > filesToShow)
                    {
                        Console.WriteLine($"   ... e mais {duplicate.File.Count - filesToShow} arquivo(s)");
                    }
                }
            }

            if (duplicates.Count > maxToShow)
            {
                Console.WriteLine($"\n... e mais {duplicates.Count - maxToShow} grupos de duplicatas.");
                Console.WriteLine("Use --max-results <número> para ver mais ou --verbose para detalhes completos.");
            }
        }

        private static void ExportResults(List<EqualFiles> duplicates, string exportPath)
        {
            try
            {
                Console.WriteLine($"\n[EXPORT] Exportando resultados para: {exportPath}");

                var extension = Path.GetExtension(exportPath).ToLower();

                switch (extension)
                {
                    case ".json":
                        ExportToJson(duplicates, exportPath);
                        break;
                    case ".csv":
                        ExportToCsv(duplicates, exportPath);
                        break;
                    case ".txt":
                        ExportToText(duplicates, exportPath);
                        break;
                    default:
                        Console.WriteLine("[ERROR]  Formato de exportação não suportado. Use .json, .csv ou .txt");
                        return;
                }

                Console.WriteLine("[OK]     Exportação concluída!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]  Erro na exportação: {ex.Message}");
            }
        }

        private static void ExportToJson(List<EqualFiles> duplicates, string path)
        {
            var data = duplicates.Select(d => new
            {
                Hash = d.HashFile,
                d.Count,
                d.Size,
                d.BytesWasted,
                Files = d.File.Select(f => new
                {
                    f.FullPath,
                    f.Directory,
                    f.FileName,
                    f.Size
                })
            });

            var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        private static void ExportToCsv(List<EqualFiles> duplicates, string path)
        {
            using var writer = new StreamWriter(path);
            writer.WriteLine("Hash,GroupSize,FileSize,BytesWasted,FullPath,Directory,FileName");

            foreach (var duplicate in duplicates)
            {
                foreach (var file in duplicate.File)
                {
                    writer.WriteLine($"{ duplicate.HashFile}" +
                    $",{duplicate.Count},{duplicate.Size},{duplicate.BytesWasted},{ file.FullPath}" + 
                    $",{ file.Directory}" +
                    $",{ file.FileName}");
                }
            }
        }

        private static void ExportToText(List<EqualFiles> duplicates, string path)
        {
            using var writer = new StreamWriter(path);
            writer.WriteLine("RELATÓRIO DE ARQUIVOS DUPLICADOS");
            writer.WriteLine($"Gerado em: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            writer.WriteLine("".PadRight(80, '='));

            foreach (var duplicate in duplicates)
            {
                writer.WriteLine($"\nGrupo - {duplicate.Count} arquivos - {Utils.FormatFileSize(duplicate.BytesWasted)} desperdiçados");
                writer.WriteLine($"Tamanho: {Utils.FormatFileSize(duplicate.Size)} cada");
                writer.WriteLine($"Hash: {duplicate.HashFile}");

                foreach (var file in duplicate.File)
                {
                    writer.WriteLine($"  {file.FullPath}");
                }
            }
        }
    }
}
