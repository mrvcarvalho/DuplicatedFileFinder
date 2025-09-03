using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using static DuplicatedFileFinder.MyFileInfo;

namespace DuplicatedFileFinder
{
    

    public class MyFileInfo
    {
        private const eHashAlgorithmType _MyPreferredHashAlgorithm = eHashAlgorithmType.MD5;

        // Propriedades básicas existentes
        public string Directory { get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }
        private string CalculatedHashFile { get; set; } = "?";
        private bool _hashCalculated = false;
        public string FullPath { get; set; }
        public eHashAlgorithmType HashAlgorithm { get; set; } = _MyPreferredHashAlgorithm;
        public TimeSpan HashCalculationTime { get; set; }



        // Propriedades de data e atributos
        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public FileAttributes Attributes { get; set; }



        // Propriedades de controle de duplicatas
        public eDuplicateAction Action { get; set; } = eDuplicateAction.None;
        public eFilePriority Priority { get; set; } = eFilePriority.Normal;
        public string Reason { get; set; } = string.Empty;
        public string ActionReason { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = false;
        public bool IsProtected { get; set; } = false;
        public bool IsReadOnly { get; set; }


        // Propriedades de localização e contexto
        public eFileLocation FileLocation { get; set; }
        public string DriveType { get; set; } = string.Empty;
        public long FreeSpaceOnDrive { get; set; }


        // Propriedades de ação
        public string TargetPath { get; set; } = string.Empty; // Para Move/Rename
        public DateTime ActionTimestamp { get; set; }
        public bool ActionExecuted { get; set; } = false;
        public string ActionError { get; set; } = string.Empty;


        // Propriedades de metadados
        public Dictionary<string, object> Metadata { get; set; } = new();
        public string Notes { get; set; } = string.Empty;


        // Propriedades calculadas
        public string FileExtension => Path.GetExtension(FileName).ToLower();
        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FileName);
        public int DirectoryDepth => FullPath.Split(Path.DirectorySeparatorChar).Length;
        public bool IsInSystemDirectory => IsSystemPath(Directory);
        public double ThroughputMBps => GetThroughputMBps();

        public enum eDuplicateAction
        {
            None,           // Nenhuma ação definida
            Keep,           // Manter este arquivo
            Delete,         // Deletar este arquivo
            Move,           // Mover para outro local
            Recycle,        // Enviar para lixeira
            Rename,         // Renomear arquivo
            CreateLink,     // Criar link simbólico
            Compress,       // Comprimir arquivo
            Archive,        // Arquivar arquivo
            Review          // Marcar para revisão manual
        }

        public enum eFilePriority
        {
            VeryLow = 1,    // Candidato principal para exclusão
            Low = 2,        // Baixa prioridade
            Normal = 3,     // Prioridade normal
            High = 4,       // Alta prioridade para manter
            VeryHigh = 5,   // Manter sempre
            Protected = 6   // Nunca deletar
        }

        public enum eFileLocation
        {
            Unknown,
            SystemDrive,
            ExternalDrive,
            NetworkDrive,
            RemovableDrive,
            CloudSync,
            TempDirectory,
            UserProfile,
            ProgramFiles,
            SystemDirectory
        }

        public enum eHashAlgorithmType
        {
            MD5,
            SHA1,
            SHA256,
            SHA512
        }

        public MyFileInfo(string fullFileName, eHashAlgorithmType hashAlgorithm = _MyPreferredHashAlgorithm)
        {
            if (string.IsNullOrEmpty(fullFileName))
            {
                throw new ArgumentException("File name cannot be null or empty", nameof(fullFileName));
            }
            if (!File.Exists(fullFileName))
            {
                throw new FileNotFoundException($"File not found: {fullFileName}");
            }

            FullPath = Path.GetFullPath(fullFileName);
            Directory = Path.GetDirectoryName(FullPath) ?? string.Empty;
            FileName = Path.GetFileName(FullPath);
            HashAlgorithm = hashAlgorithm;

            LoadFileInfo();
            AnalyzeFileLocation();
            CalculateInitialPriority();
        }

        public string HashFile
        {
            get
            {
                if (!_hashCalculated)
                {
                    CalculatedHashFile = CalculateHash();
                }
                return CalculatedHashFile;
            }
        }

        private void LoadFileInfo()
        {
            try
            {
                var fileInfo = new FileInfo(FullPath);
                Size = fileInfo.Length;
                CreationTime = fileInfo.CreationTime;
                LastWriteTime = fileInfo.LastWriteTime;
                LastAccessTime = fileInfo.LastAccessTime;
                IsReadOnly = fileInfo.IsReadOnly;

                // Informações da unidade
                var drive = new DriveInfo(Path.GetPathRoot(FullPath) ?? "C:");
                DriveType = drive.DriveType.ToString();
                FreeSpaceOnDrive = drive.AvailableFreeSpace;
            }
            catch (Exception ex)
            {
                // Log error but continue
                Reason = $"Error loading file info: {ex.Message}";
            }
        }

        private void AnalyzeFileLocation()
        {
            var rootPath = Path.GetPathRoot(FullPath)?.ToUpper() ?? "";
            var systemRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows).Substring(0, 3);

            if (FullPath.Contains("\\Temp", StringComparison.OrdinalIgnoreCase) ||
                FullPath.Contains("\\tmp", StringComparison.OrdinalIgnoreCase))
            {
                FileLocation = eFileLocation.TempDirectory;
            }
            else if (FullPath.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), StringComparison.OrdinalIgnoreCase) ||
                     FullPath.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), StringComparison.OrdinalIgnoreCase))
            {
                FileLocation = eFileLocation.ProgramFiles;
            }
            else if (FullPath.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.Windows), StringComparison.OrdinalIgnoreCase))
            {
                FileLocation = eFileLocation.SystemDirectory;
            }
            else if (FullPath.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), StringComparison.OrdinalIgnoreCase))
            {
                FileLocation = eFileLocation.UserProfile;
            }
            else if (rootPath == systemRoot)
            {
                FileLocation = eFileLocation.SystemDrive;
            }
            else
            {
                try
                {
                    var drive = new DriveInfo(rootPath);
                    FileLocation = drive.DriveType switch
                    {
                        System.IO.DriveType.Network => eFileLocation.NetworkDrive,
                        System.IO.DriveType.Removable => eFileLocation.RemovableDrive,
                        System.IO.DriveType.Fixed => eFileLocation.ExternalDrive,
                        _ => eFileLocation.Unknown
                    };
                }
                catch
                {
                    FileLocation = eFileLocation.Unknown;
                }
            }
        }

        private void CalculateInitialPriority()
        {
            // Algoritmo para calcular prioridade inicial baseado em vários fatores
            var score = 3; // Normal

            // Fatores que aumentam prioridade (manter arquivo)
            if (FileLocation == eFileLocation.UserProfile) score += 1;
            if (LastWriteTime > DateTime.Now.AddDays(-30)) score += 1; // Modificado recentemente
            if (DirectoryDepth <= 3) score += 1; // Próximo à raiz
            if (Size > 100 * 1024 * 1024) score += 1; // Arquivo grande (> 100MB)

            // Fatores que diminuem prioridade (candidato à exclusão)
            if (FileLocation == eFileLocation.TempDirectory) score -= 2;
            if (FileName.Contains("copy", StringComparison.OrdinalIgnoreCase)) score -= 1;
            if (FileName.Contains("backup", StringComparison.OrdinalIgnoreCase)) score -= 1;
            if (LastAccessTime < DateTime.Now.AddDays(-365)) score -= 1; // Não acessado há 1 ano
            if (DirectoryDepth > 8) score -= 1; // Muito aninhado

            // Proteções especiais
            if (FileLocation == eFileLocation.SystemDirectory ||
                FileLocation == eFileLocation.ProgramFiles)
            {
                Priority = eFilePriority.Protected;
                return;
            }

            Priority = Math.Max(1, Math.Min(6, score)) switch
            {
                1 => eFilePriority.VeryLow,
                2 => eFilePriority.Low,
                3 => eFilePriority.Normal,
                4 => eFilePriority.High,
                5 => eFilePriority.VeryHigh,
                6 => eFilePriority.Protected,
                _ => eFilePriority.Normal
            };
        }

        public string CalculateHash()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                byte[] hashBytes;

                using (var stream = File.OpenRead(FullPath))
                {
                    hashBytes = HashAlgorithm switch
                    {
                        eHashAlgorithmType.MD5 => MD5.Create().ComputeHash(stream),
                        eHashAlgorithmType.SHA1 => SHA1.Create().ComputeHash(stream),
                        eHashAlgorithmType.SHA256 => SHA256.Create().ComputeHash(stream),
                        eHashAlgorithmType.SHA512 => SHA512.Create().ComputeHash(stream),
                        _ => SHA256.Create().ComputeHash(stream)
                    };
                }

                stopwatch.Stop();
                HashCalculationTime = stopwatch.Elapsed;

                _hashCalculated = true;

                return $"{HashAlgorithm}:{Convert.ToHexString(hashBytes)}";
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                throw new InvalidOperationException($"Error calculating {HashAlgorithm} hash for file: {FullPath}", ex);
            }
        }

        // Métodos de controle de ação
        public void SetAction(eDuplicateAction action, string reason = "")
        {
            if (IsProtected && (action == eDuplicateAction.Delete || action == eDuplicateAction.Recycle))
            {
                throw new InvalidOperationException($"Cannot set destructive action on protected file: {FullPath}");
            }

            Action = action;
            ActionReason = reason;
            ActionTimestamp = DateTime.Now;
            ActionExecuted = false;
            ActionError = string.Empty;
        }

        public void SetTargetPath(string targetPath)
        {
            if (Action != eDuplicateAction.Move && Action != eDuplicateAction.Rename)
            {
                throw new InvalidOperationException($"Target path only valid for Move or Rename actions. Current action: {Action}");
            }

            TargetPath = targetPath;
        }

        public void ClearAction()
        {
            Action = eDuplicateAction.None;
            ActionReason = string.Empty;
            TargetPath = string.Empty;
            ActionExecuted = false;
            ActionError = string.Empty;
        }

        // Métodos de execução de ação
        public bool ExecuteAction()
        {
            if (Action == eDuplicateAction.None || ActionExecuted)
                return true;

            if (IsProtected && (Action == eDuplicateAction.Delete || Action == eDuplicateAction.Recycle))
            {
                ActionError = "File is protected from destructive actions";
                return false;
            }

            try
            {
                var result = Action switch
                {
                    eDuplicateAction.Delete => ExecuteDelete(),
                    eDuplicateAction.Recycle => ExecuteRecycle(),
                    eDuplicateAction.Move => ExecuteMove(),
                    eDuplicateAction.Rename => ExecuteRename(),
                    eDuplicateAction.CreateLink => ExecuteCreateLink(),
                    eDuplicateAction.Compress => ExecuteCompress(),
                    _ => true
                };

                if (result)
                {
                    ActionExecuted = true;
                    ActionTimestamp = DateTime.Now;
                }

                return result;
            }
            catch (Exception ex)
            {
                ActionError = ex.Message;
                return false;
            }
        }

        private bool ExecuteDelete()
        {
            if (!File.Exists(FullPath)) return true;

            File.Delete(FullPath);
            return !File.Exists(FullPath);
        }

        private bool ExecuteRecycle()
        {
            if (!File.Exists(FullPath)) return true;

            // Usar Microsoft.VisualBasic.FileIO.FileSystem para enviar para lixeira
            // Ou implementar via Shell32
            try
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                    FullPath,
                    Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                    Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                return true;
            }
            catch
            {
                // Fallback para deleção normal
                return ExecuteDelete();
            }
        }

        private bool ExecuteMove()
        {
            if (string.IsNullOrEmpty(TargetPath)) return false;
            if (!File.Exists(FullPath)) return true;

            var targetDir = Path.GetDirectoryName(TargetPath);
            if (!string.IsNullOrEmpty(targetDir) && !System.IO.Directory.Exists(targetDir))
            {
                System.IO.Directory.CreateDirectory(targetDir);
            }

            File.Move(FullPath, TargetPath);
            FullPath = TargetPath;
            Directory = Path.GetDirectoryName(TargetPath) ?? string.Empty;
            FileName = Path.GetFileName(TargetPath);

            return File.Exists(TargetPath);
        }

        private bool ExecuteRename()
        {
            if (string.IsNullOrEmpty(TargetPath)) return false;

            var newPath = Path.Combine(Directory, TargetPath);
            return ExecuteMove();
        }

        private bool ExecuteCreateLink()
        {
            if (string.IsNullOrEmpty(TargetPath)) return false;

            // Criar link simbólico (requer Windows Vista+ e privilégios)
            try
            {
                // Implementar criação de link simbólico
                // File.CreateSymbolicLink(TargetPath, FullPath); // .NET 6+
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ExecuteCompress()
        {
            // Implementar compressão do arquivo
            // Pode usar System.IO.Compression
            return true;
        }

        // Propriedades calculadas
        public string ActionStatus
        {
            get
            {
                if (Action == eDuplicateAction.None) return "Nenhuma ação";
                if (ActionExecuted) return "Executada";
                if (!string.IsNullOrEmpty(ActionError)) return $"Erro: {ActionError}";
                return "Pendente";
            }
        }
        public void MarkAsProtected(string reason = "User protected")
        {
            IsProtected = true;
            Priority = eFilePriority.Protected;
            ActionReason = reason;
        }

        // Propriedades calculadas para ToString()
        public string PriorityDescription => Priority switch
        {
            eFilePriority.VeryLow => "Priority: Very Low",
            eFilePriority.Low => "Priority: Low",
            eFilePriority.Normal => "Priority: Normal",
            eFilePriority.High => "Priority: High",
            eFilePriority.VeryHigh => "Priority: Very High",
            eFilePriority.Protected => "Priority: Protected",
            _ => "Priority: Unknown"
        };

        public string LocationDescription => FileLocation switch
        {
            eFileLocation.SystemDrive => "System Drive",
            eFileLocation.ExternalDrive => "External Drive",
            eFileLocation.NetworkDrive => "Network Drive",
            eFileLocation.RemovableDrive => "Removable Drive",
            eFileLocation.CloudSync => "Cloud Sync",
            eFileLocation.TempDirectory => "Temp Directory",
            eFileLocation.UserProfile => "User Profile",
            eFileLocation.ProgramFiles => "Program Files",
            eFileLocation.SystemDirectory => "System Directory",
            _ => "Unknown Location"
        };


        public void SetPriority(eFilePriority priority, string reason = "")
        {
            Priority = priority;
            if (!string.IsNullOrEmpty(reason))
            {
                Reason = string.IsNullOrEmpty(Reason) ? reason : $"{Reason}; {reason}";
            }
        }


        public string ActionDescription => Action switch
        {
            eDuplicateAction.None => "",
            eDuplicateAction.Keep => "KEEP",
            eDuplicateAction.Delete => "DELETE",
            eDuplicateAction.Move => "MOVE",
            eDuplicateAction.Rename => "RENAME",
            eDuplicateAction.CreateLink => "LINK",
            eDuplicateAction.Compress => "COMPRESS",
            eDuplicateAction.Archive => "ARCHIVE",
            eDuplicateAction.Review => "REVIEW",
            _ => Action.ToString().ToUpper()
        };

        private static bool IsSystemPath(string path)
        {
            var systemPaths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Environment.GetFolderPath(Environment.SpecialFolder.System)
            };

            return systemPaths.Any(sysPath => path.StartsWith(sysPath, StringComparison.OrdinalIgnoreCase));
        }

        public double GetThroughputMBps()
        {
            if (HashCalculationTime.TotalSeconds == 0) return 0;
            return (Size / 1024.0 / 1024.0) / HashCalculationTime.TotalSeconds;
        }

        // ToString() melhorado sem emojis
        public override string ToString()
        {
            var parts = new List<string>();

            // Nome do arquivo e tamanho
            parts.Add($"{FileName} ({Utils.FormatFileSize(Size)})");

            // Localização
            parts.Add($"Location: {LocationDescription}");

            // Prioridade
            parts.Add(PriorityDescription);

            // Ação (se definida)
            if (Action != eDuplicateAction.None)
            {
                parts.Add($"Action: {ActionDescription}");
            }

            // Informações de data
            parts.Add($"Modified: {LastWriteTime:yyyy-MM-dd HH:mm}");

            // Performance do hash
            if (HashCalculationTime.TotalMilliseconds > 0)
            {
                parts.Add($"Hash: {HashCalculationTime.TotalMilliseconds:F1}ms ({ThroughputMBps:F1} MB/s)");
            }

            // Razão (se especificada)
            if (!string.IsNullOrEmpty(Reason))
            {
                parts.Add($"Reason: {Reason}");
            }

            return string.Join(" | ", parts);
        }

        // ToString() detalhado para relatórios
        public string ToDetailedString()
        {
            return $"""
                File: {FileName}
                Full Path: {FullPath}
                Size: {Utils.FormatFileSize(Size)}
                Location: {LocationDescription}
                Priority: {PriorityDescription}
                Action: {(Action == eDuplicateAction.None ? "None" : ActionDescription)}
                Created: {CreationTime:yyyy-MM-dd HH:mm:ss}
                Modified: {LastWriteTime:yyyy-MM-dd HH:mm:ss}
                Accessed: {LastAccessTime:yyyy-MM-dd HH:mm:ss}
                Read Only: {IsReadOnly}
                Drive Type: {DriveType}
                Free Space: {Utils.FormatFileSize(FreeSpaceOnDrive)}
                Hash Algorithm: {HashAlgorithm}
                Hash Time: {HashCalculationTime.TotalMilliseconds:F1}ms
                Throughput: {ThroughputMBps:F2} MB/s
                Directory Depth: {DirectoryDepth}
                Hash: {CalculatedHashFile}
                Reason: {(string.IsNullOrEmpty(Reason) ? "None" : Reason)}
                """;
        }

        // ToString() compacto para listas
        public string ToCompactString()
        {
            var actionInfo = Action != eDuplicateAction.None ? $" [{ActionDescription}]" : "";
            var priorityInfo = Priority != eFilePriority.Normal ? $" ({Priority})" : "";

            return $"{FileName} ({Utils.FormatFileSize(Size)}) - {LocationDescription}{priorityInfo}{actionInfo}";
        }

        // Métodos de comparação e igualdade existentes...
        public override bool Equals(object? obj)
        {
            if (obj is MyFileInfo other)
            {
                return CalculatedHashFile == other.CalculatedHashFile; //&& Size == other.Size;
            }
            return false;
        }

        //public override int GetHashCode()
        //{
        //    return HashCode.Combine(CalculatedHashFile, Size);
        //}

        public static bool operator ==(MyFileInfo left, MyFileInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MyFileInfo left, MyFileInfo right)
        {
            return !Equals(left, right);
        }
    }
}