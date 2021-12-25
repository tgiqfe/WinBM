using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualBasic.FileIO;
using IO.Lib;

namespace IO.Work.Directory
{
    internal class Delete : IOTaskWork
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath", "dirpath", "directorypath")]
        protected string[] _Path { get; set; }

        [TaskParameter]
        [Keys("force", "forse")]
        protected bool _Force { get; set; }

        [TaskParameter]
        [Keys("recycle", "recyclebin", "trash", "trashbox")]
        protected bool _Recycle { get; set; }

        [TaskParameter]
        [Keys("clear", "crear", "claer")]
        protected bool _Clear { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            TargetDirectoryProcess(_Path, DeleteDirectoryAction);
        }

        private void DeleteDirectoryAction(string target)
        {
            //  削除フォルダーが存在しない場合
            if (!System.IO.Directory.Exists(target))
            {
                Manager.WriteLog(LogLevel.Error, "Target folder is Missing. \"{0}\"", target);
                return;
            }

            if (_Force)
            {
                Manager.WriteLog(LogLevel.Debug, "Directory delete force.");

                string emptyDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                System.IO.Directory.CreateDirectory(emptyDirPath);
                using (var proc = new Process())
                {
                    proc.StartInfo.FileName = "robocopy.exe";
                    proc.StartInfo.Arguments = $"\"{emptyDirPath}\" \"{target}\" /MIR /COPY:DAT /XJD /XJF /R:0 /W:0 /NP";
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.Start();
                    proc.WaitForExit();
                }
                System.IO.Directory.Delete(emptyDirPath);
                if (!_Clear)
                {
                    System.IO.Directory.Delete(target);
                }
            }
            else
            {
                Manager.WriteLog(LogLevel.Debug, "Directory delete normal.");

                try
                {
                    if (_Clear)
                    {
                        if (Manager.Interactive && _Recycle)
                        {
                            foreach (string filePath in System.IO.Directory.GetFiles(target))
                            {
                                FileSystem.DeleteFile(
                                    filePath,
                                    UIOption.OnlyErrorDialogs,
                                    RecycleOption.SendToRecycleBin,
                                    UICancelOption.DoNothing);
                            }
                            foreach (string dirPath in System.IO.Directory.GetDirectories(target))
                            {
                                FileSystem.DeleteFile(
                                    dirPath,
                                    UIOption.OnlyErrorDialogs,
                                    RecycleOption.SendToRecycleBin,
                                    UICancelOption.DoNothing);
                            }
                        }
                        else
                        {
                            foreach (string filePath in System.IO.Directory.GetFiles(target))
                            {
                                System.IO.File.Delete(filePath);
                            }
                            foreach (string dirPath in System.IO.Directory.GetDirectories(target))
                            {
                                System.IO.Directory.Delete(dirPath, recursive: true);
                            }
                        }
                    }
                    else
                    {
                        if (Manager.Interactive && _Recycle)
                        {
                            FileSystem.DeleteDirectory(
                                target,
                                UIOption.OnlyErrorDialogs,
                                RecycleOption.SendToRecycleBin,
                                UICancelOption.DoNothing);
                        }
                        else
                        {
                            System.IO.Directory.Delete(target, recursive: true);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                    Manager.WriteLog(LogLevel.Debug, e.ToString());
                    this.Success = false;
                }
            }
        }
    }

    /// <summary>
    /// Deleteへのエイリアス
    /// </summary>
    internal class Remove : Delete
    {
        protected override bool IsAlias { get { return true; } }
    }
}
