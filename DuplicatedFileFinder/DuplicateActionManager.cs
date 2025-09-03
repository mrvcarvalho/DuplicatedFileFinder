using System.Diagnostics;

namespace DuplicatedFileFinder
{


public class DuplicateActionManager
    {
        public class ActionResult
        {
            public MyFileInfo File { get; set; }
            public bool Success { get; set; }
            public string Error { get; set; } = string.Empty;
            public TimeSpan ExecutionTime { get; set; }
        }

        public static List<ActionResult> ExecuteActions(List<MyFileInfo> lFiles, bool dryRun = false)
        {
            var results = new List<ActionResult>();

            Console.WriteLine($"[EXEC] Executando ações em {lFiles.Count} arquivos (Dry Run: {dryRun})");

            foreach (MyFileInfo myFileInfo in lFiles.Where(f => f.Action != MyFileInfo.eDuplicateAction.None))
            {
                var stopwatch = Stopwatch.StartNew();
                var result = new ActionResult { File = myFileInfo };

                try
                {
                    if (dryRun)
                    {
                        result.Success = true;
                        Console.WriteLine($"  [DRY RUN] {myFileInfo.Action}: {myFileInfo.FullPath}");
                    }
                    else
                    {
                        result.Success = myFileInfo.ExecuteAction();
                        if (!result.Success)
                        {
                            result.Error = myFileInfo.ActionError;
                        }

                        Console.WriteLine($"  {(result.Success ? "Done" : "Error")} {myFileInfo.Action}: {myFileInfo.FullPath}");
                        if (!result.Success)
                        {
                            Console.WriteLine($"     Erro: {result.Error}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = ex.Message;
                    Console.WriteLine($"  [ERROR] {myFileInfo.Action}: {myFileInfo.FullPath} - {ex.Message}");
                }

                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
                results.Add(result);
            }

            return results;
        }

        public static void ApplySmartActions(List<EqualFiles> duplicateGroups)
        {
            Console.WriteLine("[SMART] Aplicando ações inteligentes...");

            foreach (var group in duplicateGroups)
            {
                ApplySmartActionToGroup(group);
            }
        }

        private static void ApplySmartActionToGroup(EqualFiles group)
        {
            if (group.EqualFileList.Count <= 1) return;

            // Ordenar por prioridade (maior prioridade primeiro)
            var sortedFiles = group.EqualFileList
                .OrderByDescending(f => (int)f.Priority)
                .ThenBy(f => f.FileLocation == MyFileInfo.eFileLocation.SystemDrive ? 0 : 1)
                .ThenByDescending(f => f.LastWriteTime)
                .ToList();

            // Manter o primeiro (maior prioridade)
            var fileToKeep = sortedFiles.First();
            fileToKeep.SetAction(MyFileInfo.eDuplicateAction.Keep, "Arquivo com maior prioridade");

            // Definir ações para os demais
            foreach (var file in sortedFiles.Skip(1))
            {
                if (file.IsProtected || file.Priority == MyFileInfo.eFilePriority.Protected)
                {
                    file.SetAction(MyFileInfo.eDuplicateAction.Keep, "Arquivo protegido");
                }
                else if (file.FileLocation == MyFileInfo.eFileLocation.RemovableDrive)
                {
                    file.SetAction(MyFileInfo.eDuplicateAction.Delete, "Arquivo em mídia removível");
                }
                else if (file.FileLocation == MyFileInfo.eFileLocation.CloudSync)
                {
                    file.SetAction(MyFileInfo.eDuplicateAction.Keep, "Arquivo em sincronização na nuvem");
                }
                else
                {
                    file.SetAction(MyFileInfo.eDuplicateAction.Recycle, "Duplicata detectada automaticamente");
                }
            }
        }

        public static void ShowActionSummary(List<EqualFiles> duplicateGroups)
        {
            Console.WriteLine("\n[STATUS] RESUMO DAS AÇÕES PLANEJADAS:");
            Console.WriteLine("".PadRight(80, '='));

            var actionCounts = new Dictionary<MyFileInfo.eDuplicateAction, int>();
            var totalSpaceToFree = 0L;

            foreach (var group in duplicateGroups)
            {
                foreach (var file in group.EqualFileList)
                {
                    if (!actionCounts.ContainsKey(file.Action))
                        actionCounts[file.Action] = 0;

                    actionCounts[file.Action]++;

                    if (file.Action == MyFileInfo.eDuplicateAction.Delete || file.Action == MyFileInfo.eDuplicateAction.Recycle)
                    {
                        totalSpaceToFree += file.Size;
                    }
                }
            }

            foreach (var kvp in actionCounts.OrderByDescending(x => x.Value))
            {
                var actionIcon = kvp.Key switch
                {
                    MyFileInfo.eDuplicateAction.Keep    => "Manter",
                    MyFileInfo.eDuplicateAction.Delete  => "Deletar",
                    MyFileInfo.eDuplicateAction.Recycle => "Reciclar",
                    MyFileInfo.eDuplicateAction.Move    => "Mover",
                    MyFileInfo.eDuplicateAction.Rename  => "Renomear",
                    _ => "❓"
                };

                Console.WriteLine($"  {actionIcon} {kvp.Key}: {kvp.Value} arquivos");
            }

            Console.WriteLine($"\n[DISK] Espaço a ser liberado: {Utils.FormatFileSize(totalSpaceToFree)}");
        }
    }
}