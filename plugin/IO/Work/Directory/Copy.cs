using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using IO.Lib;
using System.Diagnostics;

namespace IO.Work.Directory
{
    internal class Copy : WorkDirectory
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("sourcepath", "srcpath", "src", "source", "sourcedirectory", "sourcefolder", "sourcedir", "srcdir", "path", "directorypath", "folderpath")]
        protected string[] _SourcePath { get; set; }

        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("destinationpath", "dstpath", "dst", "destination", "destinationdirectory", "destinationfolder", "destinationdir", "dstdir")]
        protected string _DestinationPath { get; set; }

        [TaskParameter]
        [Keys("force", "forse")]
        protected bool _Force { get; set; }

        [TaskParameter(Resolv = true)]
        [Keys("excludefile", "excludefiles", "exfile", "exfiles", "xfile", "xfiles", "xf")]
        protected string[] _ExcludeFile { get; set; }

        [TaskParameter(Resolv = true)]
        [Keys("excludedirectory", "excludedirectories", "excludefolder", "excludefolders", "excludedir", "excludedirs",
            "exdirectory", "exdirectories", "exfolder", "exfolders", "exdir", "exdirs", 
            "xdirectory", "xdirectories", "xfolder", "xfolders", "xdir", "xdirs", "xd")]
        protected string[] _ExcludeDirectory { get; set; }

        [TaskParameter]
        [Keys("copyall", "all")]
        protected bool _CopyAll { get; set; }

        [TaskParameter]
        [Keys("copymode", "mode")]
        [Values("mirror,mir,miror", "merge,mrg,marge", Default = "Mirror")]
        [ValidateEnumSet("Mirror", "Merge")]
        protected CopyMode _CopyMode { get; set; }

        protected enum CopyMode
        {
            Mirror = 0,
            Merge = 1,
        }

        public override void MainProcess()
        {
            this.Success = true;

            if (_SourcePath.Length > 1 || _SourcePath[0].Contains("*"))
            {
                _DestinationPath = _DestinationPath.TrimEnd('\\') + "\\";
            }

            SrcDstSequence(_SourcePath, _DestinationPath, CopyDirectoryAction);
        }

        private void CopyDirectoryAction(string source, string destination)
        {
            try
            {
                //  コピー先の指定の最後が「\」の場合。
                //  ワイルドカードや複数指定の場合、コピー候補の数だけコピー先パス修正アクションが発生するが、許容する。
                if (destination.EndsWith("\\"))
                {
                    destination = Path.Combine(destination, Path.GetFileName(source));
                    Manager.WriteLog(LogLevel.Info, "Change destination path. \"{0}\"", destination);
                }

                //  コピー先が存在し、force=falseの場合
                if (System.IO.Directory.Exists(destination) && !_Force)
                {
                    Manager.WriteLog(LogLevel.Warn, "Destination path is already exists. \"{0}\"", destination);
                    return;
                }

                using (var proc = new Process())
                {
                    proc.StartInfo.FileName = "robocopy.exe";

                    string copyFlag = _CopyAll ? "/COPYALL" : "/COPY:DAT";      //  CopyAllは管理者実行でない場合はコピーされないので注意
                    string copyMode = _CopyMode switch
                    {
                        CopyMode.Mirror => "/MIR",
                        CopyMode.Merge => "/E",
                        _ => "",
                    };
                    /*
                    string exFiles = _ExcludeFile?.Length > 0 ?
                        " /XF \"" + string.Join("\" \"", _ExcludeFile) + "\"" :
                        "";
                    string exDirs = _ExcludeDirectory?.Length > 0 ?
                        " /XD \"" + string.Join("\" \"", _ExcludeDirectory) + "\"" :
                        "";
                    */
                    string[] tempExFiles = Wildcard.GetPaths(_ExcludeFile, isDirectory: false);
                    string[] tempExDirs = Wildcard.GetPaths(_ExcludeDirectory, isDirectory: true);
                    string exFiles = tempExFiles?.Length > 0 ?
                        " /XF \"" + string.Join("\" \"", tempExFiles) + "\"" :
                        "";
                    string exDirs = tempExDirs?.Length > 0 ?
                        " /XD \"" + string.Join("\" \"", tempExDirs) + "\"" :
                        "";

                    proc.StartInfo.Arguments = string.Format("\"{0}\" \"{1}\" {2} {3} /XJD /XJF /R:0 /W:0 /NP{4}{5}",
                        source,
                        destination,
                        copyFlag,
                        copyMode,
                        exFiles,
                        exDirs);

                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.Start();
                    proc.WaitForExit();

                    Console.WriteLine(proc.StartInfo.Arguments);
                }
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
                this.Success = false;
            }
        }
    }
}
