using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM.Task;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using WinBM;
using IO.Lib;

namespace IO.Work.File
{
    internal class Copy : WorkFile
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("sourcepath", "srcpath", "src", "source", "sourcefile", "srcfile", "path", "filepath")]
        protected string[] _SourcePath { get; set; }

        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("destinationpath", "dstpath", "dst", "destination", "destinationfile", "dstfile")]
        protected string _DestinationPath { get; set; }

        [TaskParameter]
        [Keys("force", "forse")]
        protected bool _Force { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            if (_SourcePath.Length > 1 || _SourcePath[0].Contains("*"))
            {
                _DestinationPath = _DestinationPath.TrimEnd('\\') + "\\";
            }

            SrcDstFileProcess(_SourcePath, _DestinationPath, CopyFileAction);
        }

        private void CopyFileAction(string source, string destination)
        {
            try
            {
                //  コピー先の指定がフォルダーだった場合
                //  ワイルドカードや複数指定の場合、コピー候補の数だけコピー先パス修正アクションが発生するが、許容する。
                if (System.IO.Directory.Exists(destination) || destination.EndsWith("\\"))
                {
                    destination = Path.Combine(destination, Path.GetFileName(source));
                    Manager.WriteLog(LogLevel.Info, "Change destination path. \"{0}\"", destination);
                }

                //  コピー先の親フォルダーが無い場合、作成
                string parent = Path.GetDirectoryName(destination);
                if (!System.IO.Directory.Exists(parent))
                {
                    Manager.WriteLog(LogLevel.Info, "Create parent folder. \"{0}\"", parent);
                    System.IO.Directory.CreateDirectory(parent);
                }

                FileSystem.CopyFile(source, destination, _Force);
            }
            catch (UnauthorizedAccessException uae)
            {
                Manager.WriteLog(LogLevel.Debug, "{0} {1}", this.TaskName, uae.Message);

                //  読み取り専用ファイルを上書きコピー
                if (_Force)
                {
                    Manager.WriteLog(LogLevel.Info, "Remove readonly attribute \"{0}\"", destination);
                    new FileInfo(destination).IsReadOnly = false;
                    FileSystem.CopyFile(source, destination, _Force);
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
