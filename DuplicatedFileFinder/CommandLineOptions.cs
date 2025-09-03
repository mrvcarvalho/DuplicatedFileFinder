using System.Text;

namespace DuplicatedFileFinder
{
    public class CommandLineOptions
    {
        public string Directory { get; set; } = string.Empty;
        public bool ShowHelp { get; set; }
        public bool ListScans { get; set; }
        public string LoadScanId { get; set; } = string.Empty;
        public long MinFileSize { get; set; } = 0;
        public long MaxFileSize { get; set; } = 0;
        public int MaxResults { get; set; } = -1;   // sem limite por padrão
        public List<string>? FileExtensions { get; set; }
        public List<string>? ExcludePatterns { get; set; }
        public bool SilentMode { get; set; }
        public bool VerboseMode { get; set; }
        public bool NoPause { get; set; }
        public string ExportPath { get; set; } = string.Empty;

        public LogLevel LogLevel { get; set; } = LogLevel.None;
        public bool LogToFile { get; set; } = false;
        public string LogFilePath { get; set; } = "";
        public bool VerboseLogging { get; set; } = false;

        // Propriedades derivadas
        public bool IsLoggingEnabled => LogLevel > LogLevel.None;
        public bool IsDebugLogging => LogLevel >= LogLevel.Debug;
        public bool IsTraceLogging => LogLevel >= LogLevel.Trace;



        public override string ToString()
        {
            var output = new StringBuilder();

            // Cabeçalho
            output.AppendLine("=== CONFIGURAÇÃO DO SCAN ===");

            // Diretório (sempre presente)
            if (!string.IsNullOrEmpty(Directory))
            {
                output.AppendLine($"[DIR]    Diretório: {Directory}");
            }
            else
            {
                output.AppendLine($"[DIR]    Diretório: <não especificado>");
            }

            // Filtros de arquivo
            if (FileExtensions?.Any() == true)
            {
                output.AppendLine($"[FILE]   Extensões: {string.Join(", ", FileExtensions)}");
            }
            else
            {
                output.AppendLine($"[FILE]   Extensões: todas");
            }

            // Filtros de tamanho
            if (MinFileSize > 0 && MaxFileSize > 0)
            {
                output.AppendLine($"[SIZE]   Tamanho: {Utils.FormatFileSize(MinFileSize)} - {Utils.FormatFileSize(MaxFileSize)}");
            }
            else if (MinFileSize > 0)
            {
                output.AppendLine($"[SIZE]   Tamanho mínimo: {Utils.FormatFileSize(MinFileSize)}");
            }
            else if (MaxFileSize > 0)
            {
                output.AppendLine($"[SIZE]   Tamanho máximo: {Utils.FormatFileSize(MaxFileSize)}");
            }
            else
            {
                output.AppendLine($"[SIZE]   Tamanho: sem limite");
            }

            // Padrões de exclusão
            if (ExcludePatterns?.Any() == true)
            {
                output.AppendLine($"[EXCL]   Excluir: {string.Join(", ", ExcludePatterns)}");
            }

            // Limites de resultados
            if (MaxResults == -1)
                output.AppendLine($"[LIMIT]  Máximo de resultados: Sem Limite");
            else
                output.AppendLine($"[LIMIT]  Máximo de resultados: {MaxResults:N0}");


            // Configurações de modo
            var modes = new List<string>();
            if (SilentMode) modes.Add("Silencioso");
            if (VerboseMode) modes.Add("Verboso");
            if (NoPause) modes.Add("Sem pausa");

            if (modes.Any())
            {
                output.AppendLine($"[MODE]   Modo: {string.Join(", ", modes)}");
            }

            // Configurações de logging
            if (IsLoggingEnabled)
            {
                output.AppendLine($"[LOG]    Nível: {LogLevel}");

                if (LogToFile && !string.IsNullOrEmpty(LogFilePath))
                {
                    output.AppendLine($"[LOG]    Arquivo: {LogFilePath}");
                }

                if (VerboseLogging)
                {
                    output.AppendLine($"[LOG]    Logging verboso: Ativado");
                }
            }
            else
            {
                output.AppendLine($"[LOG]    Logging: Desabilitado");
            }

            // Configurações de scan específico
            if (!string.IsNullOrEmpty(LoadScanId))
            {
                output.AppendLine($"[SCAN]   Carregar scan: {LoadScanId}");
            }

            // Exportação
            if (!string.IsNullOrEmpty(ExportPath))
            {
                output.AppendLine($"[EXPORT] Caminho: {ExportPath}");
            }

            // Flags especiais
            if (ShowHelp)
            {
                output.AppendLine($"[HELP]   Mostrar ajuda: Sim");
            }

            if (ListScans)
            {
                output.AppendLine($"[LIST]   Listar scans: Sim");
            }

            output.AppendLine("================================");

            return output.ToString();
        }


        public string ToSummary()
        {
            var parts = new List<string>();

            // Diretório
            if (!string.IsNullOrEmpty(Directory))
            {
                parts.Add($"Dir: {Path.GetFileName(Directory)}");
            }

            // Filtros
            if (FileExtensions?.Any() == true)
            {
                parts.Add($"Ext: {FileExtensions.Count}");
            }

            if (MinFileSize > 0 || MaxFileSize > 0)
            {
                parts.Add("Size: Filtered");
            }

            if (ExcludePatterns?.Any() == true)
            {
                parts.Add($"Excl: {ExcludePatterns.Count}");
            }

            // Modo
            if (VerboseMode) parts.Add("Verbose");
            if (SilentMode) parts.Add("Silent");
            if (IsLoggingEnabled) parts.Add($"Log: {LogLevel}");

            return $"[CONFIG] {string.Join(" | ", parts)}";
        }

        public string ToDebugString()
        {
            var output = new StringBuilder();

            output.AppendLine("=== DEBUG CONFIGURATION ===");
            output.AppendLine($"Directory: '{Directory}' (Exists: {Directory.Length > 0 && System.IO.Directory.Exists(Directory)})");
            output.AppendLine($"ShowHelp: {ShowHelp}");
            output.AppendLine($"ListScans: {ListScans}");
            output.AppendLine($"LoadScanId: '{LoadScanId}'");
            output.AppendLine($"MinFileSize: {MinFileSize:N0} bytes ({Utils.FormatFileSize(MinFileSize)})");
            output.AppendLine($"MaxFileSize: {MaxFileSize:N0} bytes ({Utils.FormatFileSize(MaxFileSize)})");
            output.AppendLine($"MaxResults: {MaxResults:N0} (-1 significa sem Limite)");

            output.AppendLine($"FileExtensions: {(FileExtensions?.Any() == true ? $"[{string.Join(", ", FileExtensions)}]" : "null")}");
            output.AppendLine($"ExcludePatterns: {(ExcludePatterns?.Any() == true ? $"[{string.Join(", ", ExcludePatterns)}]" : "null")}");

            output.AppendLine($"SilentMode: {SilentMode}");
            output.AppendLine($"VerboseMode: {VerboseMode}");
            output.AppendLine($"NoPause: {NoPause}");
            output.AppendLine($"ExportPath: '{ExportPath}'");

            output.AppendLine($"LogLevel: {LogLevel}");
            output.AppendLine($"LogToFile: {LogToFile}");
            output.AppendLine($"LogFilePath: '{LogFilePath}'");
            output.AppendLine($"VerboseLogging: {VerboseLogging}");

            output.AppendLine($"IsLoggingEnabled: {IsLoggingEnabled}");
            output.AppendLine($"IsDebugLogging: {IsDebugLogging}");
            output.AppendLine($"IsTraceLogging: {IsTraceLogging}");

            output.AppendLine("============================");

            return output.ToString();
        }
    }
}
