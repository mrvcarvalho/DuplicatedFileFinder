using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicatedFileFinder
{
    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4,
        Trace = 5
    }

    public static class Logger
    {
        private static LogLevel _currentLevel = LogLevel.None;
        private static bool _logToFile = false;
        private static string _logFilePath = string.Empty;
        private static readonly object _lockObject = new object();

        public static void Initialize(LogLevel level, bool logToFile = false, string logFilePath = "")
        {
            _currentLevel = level;
            _logToFile = logToFile;

            if (logToFile && !string.IsNullOrEmpty(logFilePath))
            {
                _logFilePath = logFilePath;

                // Criar diretório se não existir
                var directory = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Escrever cabeçalho do log
                WriteToFile($"=== LOG INICIADO EM {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            }
        }

        public static void Error(string message, Exception ex = null)
        {
            if (_currentLevel >= LogLevel.Error)
            {
                var logMessage = FormatMessage("ERROR", message, ex);
                WriteLog(logMessage, ConsoleColor.Red);
            }
        }

        public static void Warning(string message)
        {
            if (_currentLevel >= LogLevel.Warning)
            {
                var logMessage = FormatMessage("WARN", message);
                WriteLog(logMessage, ConsoleColor.Yellow);
            }
        }

        public static void Info(string message)
        {
            if (_currentLevel >= LogLevel.Info)
            {
                var logMessage = FormatMessage("INFO", message);
                WriteLog(logMessage, ConsoleColor.White);
            }
        }

        public static void Debug(string message)
        {
            if (_currentLevel >= LogLevel.Debug)
            {
                var logMessage = FormatMessage("DEBUG", message);
                WriteLog(logMessage, ConsoleColor.Gray);
            }
        }

        public static void Trace(string message)
        {
            if (_currentLevel >= LogLevel.Trace)
            {
                var logMessage = FormatMessage("TRACE", message);
                WriteLog(logMessage, ConsoleColor.DarkGray);
            }
        }

        private static string FormatMessage(string level, string message, Exception ex = null)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var threadId = Thread.CurrentThread.ManagedThreadId;

            var logMessage = $"[{timestamp}] [{level}] [T{threadId:D2}] {message}";

            if (ex != null)
            {
                logMessage += $"\n    Exception: {ex.GetType().Name}: {ex.Message}";
                if (_currentLevel >= LogLevel.Debug)
                {
                    logMessage += $"\n    StackTrace: {ex.StackTrace}";
                }
            }

            return logMessage;
        }

        private static void WriteLog(string message, ConsoleColor color)
        {
            lock (_lockObject)
            {
                // Escrever no console
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine("\n"+message);
                Console.ForegroundColor = originalColor;

                // Escrever no arquivo se habilitado
                if (_logToFile && !string.IsNullOrEmpty(_logFilePath))
                {
                    WriteToFile(message);
                }
            }
        }

        private static void WriteToFile(string message)
        {
            try
            {
                File.AppendAllText(_logFilePath, message + Environment.NewLine);
            }
            catch
            {
                // Ignorar erros de escrita no log para não quebrar a aplicação
            }
        }

        public static void LogException(string context, Exception ex)
        {
            Error($"Exception in {context}", ex);
        }

        public static void LogMethodEntry(string methodName, params object[] parameters)
        {
            if (_currentLevel >= LogLevel.Trace)
            {
                var paramStr = parameters?.Length > 0
                    ? string.Join(", ", parameters.Select(p => p?.ToString() ?? "null"))
                    : "no parameters";
                Trace($"Entering {methodName}({paramStr})");
            }
        }

        public static void LogMethodExit(string methodName, object result = null)
        {
            if (_currentLevel >= LogLevel.Trace)
            {
                var resultStr = result != null ? $" -> {result}" : "";
                Trace($"Exiting {methodName}{resultStr}");
            }
        }
    }
}
