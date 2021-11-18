using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using IO.Lib;
using System.IO;

namespace IO.Work.Directory
{
    internal class GetTree : IOTaskWork
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath", "dirpath", "directorypath")]
        protected string[] _Path { get; set; }

        [TaskParameter]
        [Keys("maxdepth", "depth", "maxdeepth", "depth")]
        protected int? _MaxDepth { get; set; }

        [TaskParameter]
        [Keys("tolog", "log")]
        protected bool _ToLog { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            //  MaxDeepth無指定の場合は[5]をセット
            _MaxDepth ??= 5;

            TargetDirectoryProcess(_Path, GetTreeDirectoryAction);
        }

        private void GetTreeDirectoryAction(string target)
        {
            var sb = new StringBuilder();

            sb.AppendLine(target);
            RecursiveTree(target, sb, 0);

            if (_ToLog)
            {
                Manager.WriteLog(LogLevel.Info, sb.ToString().TrimEnd('\r', '\n'));
            }
            else
            {
                Manager.WriteStandard(sb.ToString().TrimEnd('\r', '\n'));
            }
        }

        private void RecursiveTree(string targetDir, StringBuilder sb, int deepth)
        {
            foreach (string path in System.IO.Directory.GetDirectories(targetDir))
            {
                sb.Append(new string(' ', deepth * 2));
                sb.AppendLine(Path.GetFileName(path) + "\\");
                if (deepth < _MaxDepth)
                {
                    RecursiveTree(path, sb, deepth + 1);
                }
            }
            foreach (string path in System.IO.Directory.GetFiles(targetDir))
            {
                sb.Append(new string(' ', deepth * 2));
                sb.AppendLine(Path.GetFileName(path));
            }
        }
    }
}
