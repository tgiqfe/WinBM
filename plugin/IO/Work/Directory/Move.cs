using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using IO.Lib;
using Microsoft.VisualBasic.FileIO;

namespace IO.Work.Directory
{
    internal class Move : WorkDirectory
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

        public override void MainProcess()
        {
            this.Success = true;

            //string destination = _DestinationPath;
            if (_SourcePath.Length > 1 || _SourcePath[0].Contains("*"))
            {
                _DestinationPath = _DestinationPath.TrimEnd('\\') + "\\";
            }

            SrcDstDirectoryProcess(_SourcePath, _DestinationPath, MoveDirectoryAction);
        }

        private void MoveDirectoryAction(string source, string destination)
        {
            try
            {
                //  移動先の指定の最後が「\」の場合。
                //  ワイルドカードや複数指定の場合、移動候補の数だけコピー先パス修正アクションが発生するが、許容する。
                if (destination.EndsWith("\\"))
                {
                    destination = Path.Combine(destination, Path.GetFileName(source));
                    Manager.WriteLog(LogLevel.Info, "Change destination path. \"{0}\"", destination);
                }

                //  移動先が存在し、force=falseの場合
                if (System.IO.Directory.Exists(destination) && !_Force)
                {
                    Manager.WriteLog(LogLevel.Warn, "Destination path is already exists. \"{0}\"", destination);
                    return;
                }

                //  移動先の親フォルダーが無い場合、作成
                string parent = Path.GetDirectoryName(destination);
                if (!System.IO.Directory.Exists(parent))
                {
                    Manager.WriteLog(LogLevel.Info, "Create parent folder. \"{0}\"", parent);
                    System.IO.Directory.CreateDirectory(parent);
                }

                FileSystem.MoveDirectory(source, destination, _Force);
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
