
// This code is a simple console application that searches for duplicate files in a specified folder.
// It retrieves all files from the folder, compares their names and sizes, and deletes duplicates.
// Make sure to test this code in a safe environment before running it on important files, as it will delete files permanently.
// Also, consider adding error handling and logging for a production-level application.
// 
// Note: The above code is a basic implementation for finding and deleting duplicate files based on their names and sizes.
// It does not handle exceptions or edge cases such as files with the same name but different content.
// For a more robust solution, consider using hash comparisons or fileInfo content checks.
// This code is a simple console application that searches for duplicate files in a specified folder.
// It retrieves all files from the folder, compares their names and sizes, and deletes duplicates.
// Make sure to test this code in a safe environment before running it on important files, as it will delete files permanently.
// Also, consider adding error handling and logging for a production-level application.
using DuplicatedFileFinderTest;
using DuplicatedFileFinderTest.CommonTestUtilities;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace DuplicatedFileFinder
{
    public class Program
    {
        private static readonly LiteDbPersistence _persistence = new("duplicates.db");

        public static void Main(string[] args)
        {
            try
            {
                //ConfigureConsole();
                ShowHeader();

                var options = ParseArguments(args);

                // Mostrar configuração
                // Ou versão compacta
                if (options.VerboseMode)
                {
                    Console.WriteLine(options.ToString());
                }
                else
                {
                    Console.WriteLine(options.ToSummary());
                }

                // Para debug (apenas se logging habilitado)
                if (options.IsDebugLogging)
                {
                    Console.WriteLine(options.ToDebugString());
                }

                // Inicializar sistema de logging
                InitializeLogging(options);

                Logger.Info("=== APLICAÇÃO INICIADA ===");
                Logger.Debug($"Argumentos recebidos: {string.Join(" ", args)}");

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

                Logger.Info("=== APLICAÇÃO FINALIZADA COM SUCESSO ===");
            }
            catch (Exception ex)
            {
                Logger.LogException("Main", ex);
                Console.WriteLine($"[ERROR]  Erro fatal: {ex.Message}");

                if (args.Contains("--debug") || args.Contains("--trace-log"))
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

        private static void InitializeLogging(CommandLineOptions options)
        {
            // Definir caminho padrão do log se não especificado
            var logPath = options.LogFilePath;
            if (options.LogToFile && string.IsNullOrEmpty(logPath))
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "DuplicatedFileFinder",
                    "Logs",
                    $"duplicate_scan_{timestamp}.log"
                );
                options.LogFilePath = logPath;
            }

            Logger.Initialize(options.LogLevel, options.LogToFile, logPath);

            if (options.IsLoggingEnabled)
            {
                Logger.Info("=== CONFIGURAÇÃO CARREGADA ===");
                Logger.Info(options.ToString());

                if (options.IsDebugLogging)
                {
                    Logger.Debug("=== DEBUG CONFIGURATION ===");
                    Logger.Debug(options.ToDebugString());
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
                args[i] = args[i].Replace("%20", " ").Trim();
            }

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

                    case "--max-size":
                        if (i + 1 < args.Length && long.TryParse(args[++i], out long maxSize))
                            options.MaxFileSize = maxSize;
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



                    case "--log-level":
                        if (i + 1 < args.Length)
                        {
                            if (Enum.TryParse<LogLevel>(args[++i], true, out var level))
                            {
                                options.LogLevel = level;
                            }
                            else
                            {
                                throw new ArgumentException($"Invalid log level: {args[i]}. Valid values: None, Error, Warning, Info, Debug, Trace");
                            }
                        }
                        break;

                    case "--log-file":
                        if (i + 1 < args.Length)
                        {
                            options.LogToFile = true;
                            options.LogFilePath = args[++i];
                        }
                        break;

                    case "--verbose-log":
                        options.VerboseLogging = true;
                        options.LogLevel = LogLevel.Debug;
                        break;

                    case "--trace-log":
                        options.VerboseLogging = true;
                        options.LogLevel = LogLevel.Trace;
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
            Console.WriteLine(@$"
[HELP] {AssemblyInfo.ProductTitle} {AssemblyInfo.ShortVersion}
{AssemblyInfo.Description}

[SINTAXE]
DuplicatedFileFinder.exe [OPÇÕES]

[MAIN] OPÇÕES PRINCIPAIS:
    -d, --directory <path>     Diretório para escanear
    --no-pause                 Não pausar no final
    -h, --help                 Mostrar esta ajuda

[FILTER] OPÇÕES DE FILTRO:
    -e, --extensions <list>    Extensões de arquivo (ex: jpg,png,pdf)
    --exclude <patterns>       Padrões para excluir (ex: temp,cache)
    --min-size <bytes>         Tamanho mínimo do arquivo em bytes
    --max-size <bytes>         Tamanho máximo do arquivo em bytes

[LOGS]
  -l, --log-level <level>    Nível de log (None, Error, Warning, Info, Debug, Trace)
  --log-file <path>          Salvar logs em arquivo
  --verbose-log              Ativar logging detalhado (Debug)
  --trace-log                Ativar logging máximo (Trace)

[STATS] OPÇÕES DE EXIBIÇÃO:
    --max-results <number>     Máximo de grupos a exibir (padrão: 50)
    -s, --silent               Modo silencioso (menos output)
    --verbose                  Modo verboso (mais detalhes)

[DISK] OPÇÕES DE DADOS:
    -l, --list                 Listar scans anteriores
    --load <id>                Carregar scan específico
    --export <path>            Exportar resultados para arquivo

[INFO] OUTRAS:
    -v, --version              Mostrar informações de versão

[EXAMPLE] EXEMPLOS:
    DuplicatedFileFinder.exe 'C:\Meus%20Documentos\'
    DuplicatedFileFinder.exe -d 'D:\Fotos' -e jpg,png --min-size 1024
    DuplicatedFileFinder.exe --list
    DuplicatedFileFinder.exe --load 12345

    # Scan básico
    DuplicatedFileFinder.exe -d 'C:\Users\Documents'
  
    # Com logging em arquivo
    DuplicatedFileFinder.exe -d 'C:\Users\Documents' --log-level Info --log-file 'scan.log'
  
    # Logging detalhado
    DuplicatedFileFinder.exe -d 'C:\Users\Documents' --verbose-log
  
    # Logging máximo com trace
    DuplicatedFileFinder.exe -d 'C:\Users\Documents' --trace-log --log-file 'detailed.log'

    NÍVEIS DE LOG:
      None     - Sem logging
      Error    - Apenas erros
      Warning  - Erros + avisos
      Info     - Erros + avisos + informações
      Debug    - Erros + avisos + informações + debug
      Trace    - Todos os logs (máximo detalhamento)

");
        }

        private static void ShowPreviousScans()
        {
            Console.WriteLine("[STATS] SCANS ANTERIORES:");
            Console.WriteLine("".PadRight(90, '='));

            try
            {
                var scans = _persistence.GetAllScans();

                if (!scans.Any())
                {
                    Console.WriteLine("[EMPTY]  Nenhum scan encontrado no banco de dados.");
                    return;
                }

                Console.WriteLine($"{"ID",-8} {"Data/Hora",-18} {"Diretório",-40} {"Grupos",-8} {"Desperdício",-15}");
                Console.WriteLine("".PadRight(90, '-'));

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
                DisplayDuplicates(duplicates, new CommandLineOptions { MaxResults = -1 }); // Sem limite ao carregar scan salvo
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

            if (options.MaxFileSize > 0)
                Console.WriteLine($"[SIZE]   Tamanho máximo: {Utils.FormatFileSize(options.MaxFileSize)}");

            Console.WriteLine("".PadRight(80, '='));

            try
            {
                // 1. Descobrir arquivos
                Logger.Trace($"> 1. Descobrir arquivos: ({options.FileExtensions})");
                var allFiles = DiscoverFiles(options);
                if (!allFiles.Any())
                {
                    Console.WriteLine("[EMPTY] Nenhum arquivo encontrado para processar.");
                    return;
                }

                // 2. Processar arquivos (FileSize)
                Logger.Trace($"> 2. Processar arquivos. Calcular FileSize:");
                var allFilesWithSize = ProcessFilesSizeVerification(allFiles, options);

                // 3. Filtrar Arquivos com FileSize > 0
                Logger.Trace($"> 3. Filtrar Arquivos com FileSize > 0:");
                var allFilesWithSizeGreatherThanZero = allFilesWithSize.Where(f => f.Size > 0).ToList();

                // 4. Localizar os arquivos que possuem o mesmo tamanho
                Logger.Trace($"> 4. Localizar os arquivos que possuem o mesmo tamanho:");
                var allFilesWithSameSize = allFilesWithSizeGreatherThanZero.GroupBy(f => f.Size)
                                                                              .Where(g => g.Count() > 1)
                                                                              .SelectMany(g => g)
                                                                              .ToList();    

                if (allFilesWithSameSize.Count == 0)
                {
                    Console.WriteLine("[EMPTY]  Nenhum arquivo válido para verificar duplicatas.");
                    Logger.Trace($"[EMPTY]  Nenhum arquivo válido para verificar duplicatas.");
                    return;
                }

                // 5. Processar arquivos (CheckSum)
                Logger.Trace($"> 5. Processar arquivos (CheckSum):");
                var filesToCheck = ProcessFiles(allFilesWithSameSize, options);
                if (filesToCheck.Count == 0)
                {
                    Console.WriteLine("[EMPTY]  Nenhum arquivo válido para verificar duplicatas.");
                    Logger.Trace($"[EMPTY]  Nenhum arquivo válido para verificar duplicatas.");
                    return;
                }

                // 6. Encontrar duplicatas
                Logger.Trace($"> 6. Encontrar duplicatas por CheckSum:");
                var finder = new DuplicatedFileFinder();
                var duplicates = finder.FindDuplicates(filesToCheck);

                stopwatch.Stop();

                // 7. Salvar resultados
                Logger.Trace($"> 7. Salvar resultados no DB:");
                var scanId = _persistence.SaveScanResults(options.Directory, filesToCheck, duplicates);

                // 8. Exibir resultados
                Logger.Trace($"> 8. Exibir resultados DUPLICADOS:");
                ShowScanResults(duplicates, filesToCheck.Count, stopwatch.Elapsed, scanId);
                DisplayDuplicates(duplicates, options);

                // 9. Exportar se solicitado
                if (!string.IsNullOrEmpty(options.ExportPath))
                {
                    Logger.Trace($"> 9. Exportar resultados DUPLICADOS:");
                    ExportResults(duplicates, options.ExportPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]  Erro durante o scan: {ex.Message}");
                Logger.Trace($"[ERROR]  Erro durante o scan: {ex.Message}");
                throw;
            }
        }


        private static string[] DiscoverFiles(CommandLineOptions options)
        {
            Logger.LogMethodEntry(nameof(DiscoverFiles), options.Directory);

            try
            {
                Console.WriteLine("[SEARCH] Descobrindo arquivos...");
                Logger.Info($"Iniciando descoberta de arquivos em: {options.Directory}");

                var stopwatch = Stopwatch.StartNew();
                var allFiles = new List<string>();
                var processedDirs = 0;

                var searchPattern = "*.*";
                Logger.Debug($"Padrão de busca: {searchPattern}");

                // Descobrir arquivos com progresso simples
                DiscoverFilesWithProgress(options.Directory, allFiles, ref processedDirs, stopwatch, options.SilentMode);
                
                Logger.Info($"Arquivos encontrados inicialmente: {allFiles.Count:N0}");

                // Aplicar filtros
                var filteredFiles = ApplyFilters(allFiles.ToArray(), options, stopwatch);

                stopwatch.Stop();

                Logger.Info($"Descoberta de arquivos concluída em {FormatTimeSpan(stopwatch.Elapsed)}");

                if (!options.SilentMode)
                {
                    Console.WriteLine($"[STATS]  Encontrados {allFiles.Count:N0} e filtrados {filteredFiles.Count():N0} arquivos para processar");
                }

                Logger.LogMethodExit(nameof(DiscoverFiles), $"{allFiles.Count} files and filtered {filteredFiles.Count():N0} files to process");

                return filteredFiles;
            }
            catch (Exception ex)
            {
                Logger.LogException(nameof(DiscoverFiles), ex);
                throw;
            }
        }

        private static void DiscoverFilesWithProgress(string currentDir, List<string> allFiles, ref int processedDirs, Stopwatch stopwatch, bool silentMode)
        {
            try
            {
                // Adicionar arquivos do diretório atual
                var files = Directory.GetFiles(currentDir, "*.*", SearchOption.TopDirectoryOnly);
                allFiles.AddRange(files);

                processedDirs++;

                // Mostrar progresso simples
                if (!silentMode && processedDirs % 50 == 0)
                {
                    var speed = stopwatch.Elapsed.TotalSeconds > 0 ? processedDirs / stopwatch.Elapsed.TotalSeconds : 0;
                    Console.Write($"\r[SEARCH] Processados: {processedDirs:N0} diretórios | {allFiles.Count:N0} arquivos | {speed:F1} dirs/s");
                }

                // Processar subdiretórios
                var subdirs = Directory.GetDirectories(currentDir);
                foreach (var subdir in subdirs)
                {
                    DiscoverFilesWithProgress(subdir, allFiles, ref processedDirs, stopwatch, silentMode);
                }
            }
            catch (UnauthorizedAccessException)
            {
                processedDirs++;
                if (!silentMode)
                {
                    Console.Write($"\r[SEARCH] Acesso negado: {Path.GetFileName(currentDir)} | {processedDirs:N0} dirs");
                }
            }
            catch (Exception)
            {
                processedDirs++;
            }
        }

        private static string[] ApplyFilters(string[] allFiles, CommandLineOptions options, Stopwatch stopwatch)
        {
            var originalCount = allFiles.Length;
            var currentFiles = allFiles.AsEnumerable();

            // Filtrar por extensões
            if (options.FileExtensions?.Any() == true)
            {
                Logger.Trace($"> Filtrar por extensões: ({options.FileExtensions})");

                currentFiles = currentFiles.Where(f =>
                    options.FileExtensions.Contains(Path.GetExtension(f).ToLower().TrimStart('.')));

                Logger.Info ($"Filtro de extensões aplicado: {originalCount:N0} → {allFiles.Length:N0} arquivos");
                Logger.Debug($"Extensões permitidas: {string.Join(", ", options.FileExtensions)}");

                if (!options.SilentMode)
                {
                    var afterExtFilter = currentFiles.Count();
                    Console.WriteLine($"\n[FILTER] Extensões: {originalCount:N0} → {afterExtFilter:N0} arquivos");
                }
            }

            // Filtrar por padrões de exclusão
            if (options.ExcludePatterns?.Any() == true)
            {
                Logger.Trace($"> Filtrar por padrões de exclusão: ({options.ExcludePatterns})");

                var beforeExclude = currentFiles.Count();

                currentFiles = currentFiles.Where(f =>
                {
                    var fileName = Path.GetFileName(f); //GetRelativePathFromBase(f, options.Directory);
                    var excluded = options.ExcludePatterns.Any(pattern =>
                        fileName.Contains(pattern, StringComparison.OrdinalIgnoreCase));
                        if (excluded)
                        {
                            Logger.Trace($"Arquivo excluído por padrões: {fileName}");
                        }
                        return !excluded;
                }).ToArray();

                Logger.Info($"Filtro de exclusão aplicado: {beforeExclude:N0} → {currentFiles.Count():N0} arquivos");
                Logger.Debug($"Padrões de exclusão: {string.Join(", ", options.ExcludePatterns)}");

                if (!options.SilentMode)
                {
                    Console.WriteLine($"\n[FILTER] Exclusões: {beforeExclude:N0} → {currentFiles.Count():N0} arquivos");
                }
            }

            // Filtrar por tamanho mínimo
            if (options.MinFileSize > 0)
            {
                Logger.Trace($"> Filtrar por tamanho mínimo: ({Utils.FormatFileSize(options.MinFileSize)})");

                var beforeFilter = currentFiles.Count();
                currentFiles = currentFiles.Where(f =>
                {
                    try
                    {
                        var size = new FileInfo(f).Length;
                        var included = size >= options.MinFileSize;

                        if (!included)
                        {
                            Logger.Trace($"Arquivo muito pequeno: {f} ({Utils.FormatFileSize(size)})");
                        }

                        return included;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"Erro ao verificar tamanho do arquivo {f}: {ex.Message}");
                        return false;
                    }
                }).ToArray();

                Logger.Info($"Filtro de tamanho mínimo aplicado: {beforeFilter:N0} → {currentFiles.Count():N0} arquivos (mín: {Utils.FormatFileSize(options.MinFileSize)})");

                if (!options.SilentMode)
                {
                    Console.WriteLine($"\n[FILTER] Exclusões: {beforeFilter:N0} → {currentFiles.Count():N0} arquivos");
                }
            }


            // Filtrar por tamanho máximo
            if (options.MaxFileSize > 0)
            {
                Logger.Trace($"> Filtrar por tamanho máximo: ({Utils.FormatFileSize(options.MaxFileSize)})");

                var beforeFilter = currentFiles.Count();
                currentFiles = currentFiles.Where(f =>
                {
                    try
                    {
                        var size = new FileInfo(f).Length;
                        var included = size <= options.MaxFileSize;

                        if (!included)
                        {
                            Logger.Trace($"Arquivo muito grande: {f} ({Utils.FormatFileSize(size)})");
                        }

                        return included;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"Erro ao verificar tamanho do arquivo {f}: {ex.Message}");
                        return false;
                    }
                }).ToArray();

                Logger.Info($"Filtro de tamanho máximo aplicado: {beforeFilter:N0} → {currentFiles.Count():N0} arquivos (máx: {Utils.FormatFileSize(options.MaxFileSize)})");

                if (!options.SilentMode)
                {
                    Console.WriteLine($"\n[FILTER] Exclusões: {beforeFilter:N0} → {currentFiles.Count():N0} arquivos");
                }
            }

            return currentFiles.ToArray();
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

        private static List<MyFileInfo> ProcessFilesSizeVerification(string[] files, CommandLineOptions options)
        {
            Logger.LogMethodEntry(nameof(ProcessFilesSizeVerification), $"{files.Length} files");

            try
            {
                Console.WriteLine("[FileSize]   Calculando o tamanho dos arquivos...");
                Logger.Info($"Iniciando processamento de {files.Length:N0} arquivos");

                var filesToCheck = new List<MyFileInfo>();
                var processed = 0;
                var errors = 0;
                var stopwatch = Stopwatch.StartNew();

                foreach (var file in files)
                {
                    try
                    {
                        Logger.Trace($"Processando arquivo: {file}");
                        var fileInfo = new MyFileInfo(file);
                        filesToCheck.Add(new MyFileInfo(file));
                        processed++;

                        Logger.Trace($"FileSize calculado para {Path.GetFileName(file)}: {Utils.FormatFileSize(fileInfo.Size)}...");
                        ShowProgress(processed, files.Length, stopwatch.Elapsed, options.SilentMode);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        errors++;
                        Logger.Warning($"Acesso negado: {file}");
                        if (options.VerboseMode)
                        {
                            Console.WriteLine($"\n[ERROR]  Erro processando {file}: {ex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        Logger.Error($"Erro processando arquivo: {file}", ex);
                        if (options.VerboseMode)
                        {
                            Console.WriteLine($"\n[ERROR] Erro processando {file}: {ex.Message}");
                        }
                    }
                }
                stopwatch.Stop();

                Logger.Info($"Processamento concluído: {processed:N0} sucessos, {errors:N0} erros em {FormatTimeSpan(stopwatch.Elapsed)}");

                Logger.LogMethodExit(nameof(ProcessFilesSizeVerification), $"{filesToCheck.Count} processed files");

                if (!options.SilentMode)
                {
                    ShowProgress(processed, files.Length, stopwatch.Elapsed, false);
                    Console.WriteLine(); // Nova linha após o progresso
                }

                return filesToCheck;
            }
            catch (Exception ex)
            {
                Logger.LogException(nameof(ProcessFilesSizeVerification), ex);
                throw;
            }
        }

        private static List<MyFileInfo> ProcessFiles(List<MyFileInfo> files, CommandLineOptions options)
        {
            Logger.LogMethodEntry(nameof(ProcessFiles), $"{files.Count} files");

            try
            {
                Console.WriteLine("[HASH]   Calculando hashes dos arquivos...");
                Logger.Info($"Iniciando processamento de {files.Count:N0} arquivos");

                var filesToCheck = new List<MyFileInfo>();
                var processed = 0;
                var errors = 0;
                var stopwatch = Stopwatch.StartNew();

                foreach (var file in files)
                {
                    try
                    {
                        Logger.Trace($"Processando arquivo: {file}");
                        filesToCheck.Add(file);
                        processed++;

                        Logger.Trace($"Hash calculado para {Path.GetFileName(file.FullPath)}: {file.HashFile[..8]}...");
                        ShowProgress(processed, files.Count, stopwatch.Elapsed, options.SilentMode);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        errors++;
                        Logger.Warning($"Acesso negado: {file.FullPath}");
                        if (options.VerboseMode)
                        {
                            Console.WriteLine($"\n[ERROR]  Erro processando {file.FullPath}: {ex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        Logger.Error($"Erro processando arquivo: {file.FullPath}", ex);
                        if (options.VerboseMode)
                        {
                            Console.WriteLine($"\n[ERROR] Erro processando {file.FullPath}: {ex.Message}");
                        }
                    }
                }
                stopwatch.Stop();

                Logger.Info($"Processamento concluído: {processed:N0} sucessos, {errors:N0} erros em {FormatTimeSpan(stopwatch.Elapsed)}");

                Logger.LogMethodExit(nameof(ProcessFiles), $"{filesToCheck.Count} processed files");

                if (!options.SilentMode)
                {
                    //Console.WriteLine($"\r[OK]     Processamento concluído: {processed:N0} sucessos, {errors:N0} erros");
                    ShowProgress(processed, files.Count, stopwatch.Elapsed, false);
                    Console.WriteLine(); // Nova linha após o progresso
                }

                // Mostrar resumo final
                ShowProcessingSummary(processed, errors, files.Count, stopwatch.Elapsed, options.SilentMode);

                return filesToCheck;
            }
            catch (Exception ex)
            {
                Logger.LogException(nameof(ProcessFiles), ex);
                throw;
            }
        }

        private static void ShowProgress(int processed, int total, TimeSpan elapsed, bool silentMode)
        {
            if (silentMode) return;

            var percentage = total > 0 ? (processed * 100.0) / total : 0;
            var speed = elapsed.TotalSeconds > 0 ? processed / elapsed.TotalSeconds : 0;

            // Calcular ETA apenas se temos dados suficientes
            var etaText = "";
            if (processed > 0 && processed < total && speed > 0)
            {
                var remainingFiles = total - processed;
                var eta = TimeSpan.FromSeconds(remainingFiles / speed);
                etaText = $" ETA: {FormatTimeSpan(eta)}";
            }

            // Barra de progresso visual
            var barLength = 30;
            var filled = (int)(percentage / 100.0 * barLength);
            var bar = new string('█', filled) + new string('░', barLength - filled);

            // Formatação da velocidade
            var speedText = speed >= 1 ? $"{speed:F1} arq/s" : $"{speed * 60:F1} arq/min";

            Console.Write($"\r[WAIT]   [{bar}] {processed:N0}/{total:N0} ({percentage:F1}%) {speedText}{etaText}");
        }

        private static string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
                return $"{timeSpan.Hours:D2}h{timeSpan.Minutes:D2}m";
            else if (timeSpan.TotalMinutes >= 1)
                return $"{timeSpan.Minutes:D2}m{timeSpan.Seconds:D2}s";
            else
                return $"{timeSpan.Seconds:D2}s";
        }

        private static void ShowProcessingSummary(int processed, int errors, int total, TimeSpan elapsed, bool silentMode)
        {
            if (silentMode) return;

            Console.WriteLine($"[OK]     Processamento concluído:");
            Console.WriteLine($"[TOTAL]  Total de arquivos: {total:N0}");
            Console.WriteLine($"[DONE]   Processados com sucesso: {processed:N0}");

            if (errors > 0)
            {
                Console.WriteLine($"[ERROR] Erros encontrados: {errors:N0}");
                var successRate = total > 0 ? (processed * 100.0) / total : 0;
                Console.WriteLine($"[CHECK] Taxa de sucesso: {successRate:F1}%");
            }

            Console.WriteLine($"[TIME]   Tempo total: {FormatTimeSpan(elapsed)}");

            if (processed > 0 && elapsed.TotalSeconds > 0)
            {
                var avgSpeed = processed / elapsed.TotalSeconds;
                Console.WriteLine($"[SPEED]  Velocidade média: {avgSpeed:F1} arquivos/segundo");

                var avgTimePerFile = elapsed.TotalMilliseconds / processed;
                Console.WriteLine($"[TIME]   Tempo médio por arquivo: {avgTimePerFile:F1}ms");
            }

            Console.WriteLine();
        }

        private static void ShowScanResults(List<EqualFiles> duplicates, int totalFiles, TimeSpan elapsed, LiteDB.ObjectId scanId)
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("[STATS]   RESULTADOS DO SCAN:");
                Console.WriteLine("".PadRight(80, '='));

                // Tempo - formatação manual para evitar problemas
                var timeString = $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                Console.WriteLine($"[TIME]   Tempo de processamento: {timeString}");

                // Arquivos analisados
                Console.WriteLine($"[DIR]    Total de arquivos analisados: {totalFiles:N0}");

                // Verificações seguras para duplicates
                if (duplicates == null)
                {
                    Console.WriteLine("[SEARCH] Grupos de duplicatas encontrados: 0");
                    Console.WriteLine("[FILE]   Total de arquivos duplicados: 0");
                    Console.WriteLine("[DISK]   Espaço desperdiçado: 0 bytes");
                }
                else
                {
                    var groupCount = duplicates.Count;
                    var totalDuplicateFiles = 0;
                    var totalBytesWasted = 0L;

                    // Calcular totais de forma segura
                    foreach (var duplicate in duplicates)
                    {
                        if (duplicate != null)
                        {
                            totalDuplicateFiles += duplicate.Count;
                            totalBytesWasted += duplicate.BytesWasted;
                        }
                    }

                    Console.WriteLine($"[SEARCH] Grupos de duplicatas encontrados: {groupCount:N0}");
                    Console.WriteLine($"[FILE]   Total de arquivos duplicados: {totalDuplicateFiles:N0}");
                    Console.WriteLine($"[DISK]   Espaço desperdiçado: {Utils.FormatFileSize(totalBytesWasted)}");
                }

                // ID do scan
                var scanIdDisplay = FormatScanId(scanId);
                Console.WriteLine($"[ID]     ID do scan: {scanIdDisplay}");

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]  Erro ao exibir resultados: {ex.Message}");
                Console.WriteLine();
            }
        }

        private static string FormatScanId(LiteDB.ObjectId scanId)
        {
            if (scanId == null)
                return "N/A";

            try
            {
                var idString = scanId.ToString();
                return idString?.Length >= 6 ? idString[^6..] : (idString ?? "N/A");
            }
            catch
            {
                return "ERROR";
            }
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

            var maxToShow = Math.Min(options.MaxResults, duplicates.Count);
            if (options.MaxResults == -1)   // Sem Limite
                maxToShow = duplicates.Count;

            for (int i = 0; i < maxToShow; i++)
            {
                var duplicate = duplicates[i];
                Console.WriteLine($"\n[LIST] Grupo {i + 1}/{duplicates.Count} - {duplicate.Count} arquivos - {Utils.FormatFileSize(duplicate.BytesWasted)} desperdiçados");
                Console.WriteLine($"[SIZE] Tamanho: {Utils.FormatFileSize(duplicate.Size)} cada");
                Console.WriteLine($"[HASH] Hash: {duplicate.HashFile}");

                foreach (var fileInfo in duplicate.EqualFileList)
                {
                    Console.WriteLine($"\n[FILE] {fileInfo.FullPath}");
                    Console.WriteLine("".PadRight(80, '-'));

                    // Diferentes formatos de saída
                    if (options.VerboseMode)
                    {
                        //Console.WriteLine("Compact: " + fileInfo.ToCompactString());
                        Console.WriteLine("Detailed:\n" + fileInfo.ToDetailedString());
                    } else { 
                        Console.WriteLine("Standard: " + fileInfo.ToString());
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
                Files = d.EqualFileList.Select(f => new
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
                foreach (var file in duplicate.EqualFileList)
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

                foreach (var file in duplicate.EqualFileList)
                {
                    writer.WriteLine($"  {file.FullPath}");
                }
            }
        }
    }
}
