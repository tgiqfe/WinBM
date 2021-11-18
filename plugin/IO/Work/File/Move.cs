using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using IO.Lib;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace IO.Work.File
{
    internal class Move : IOTaskWork
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("sourcepath", "srcpath", "src", "source", "sourcefile", "srcfile", "path", "filepath")]
        protected string[] _SourcePath { get; set; }

        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("destinationpath", "dstpath", "dst", "destination", "destinationfile", "dstfile")]
        protected string _DestinationPath { get; set; }

        [TaskParameter]
        [Keys("force", "forse")]
        protected bool _Force { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            //string destination = _DestinationPath;
            if (_SourcePath.Length > 1 || _SourcePath[0].Contains("*"))
            {
                _DestinationPath = _DestinationPath.TrimEnd('\\') + "\\";
            }

            SrcDstFileProcess(_SourcePath, _DestinationPath, MoveFileAction);
        }

        private void MoveFileAction(string source, string destination)
        {
            try
            {
                //  移動先の指定がフォルダーだった場合
                //  ワイルドカードや複数指定の場合、移動候補の数だけコピー先パス修正アクションが発生するが、許容する。
                if (System.IO.Directory.Exists(destination) || destination.EndsWith("\\"))
                {
                    destination = Path.Combine(destination, Path.GetFileName(source));
                    Manager.WriteLog(LogLevel.Info, "Change destination path. \"{0}\"", destination);
                }

                //  移動先の親フォルダーが無い場合、作成
                string parent = Path.GetDirectoryName(destination);
                if (!System.IO.Directory.Exists(parent))
                {
                    Manager.WriteLog(LogLevel.Info, "Create parent folder. \"{0}\"", parent);
                    System.IO.Directory.CreateDirectory(parent);
                }

                FileSystem.MoveFile(source, destination, _Force);
            }
            catch (UnauthorizedAccessException uae)
            {
                Manager.WriteLog(LogLevel.Debug, "{0} {1}", this.TaskName, uae.Message);

                //  読み取り専用ファイルを上書きコピー
                if (_Force)
                {
                    Manager.WriteLog(LogLevel.Info, "Remove readonly attribute \"{0}\"", destination);
                    new FileInfo(destination).IsReadOnly = false;
                    FileSystem.MoveFile(source, destination, _Force);
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
