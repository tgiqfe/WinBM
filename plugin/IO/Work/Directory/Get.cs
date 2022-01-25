using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using IO.Lib;
using System.Security.Cryptography;

namespace IO.Work.Directory
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Get : WorkDirectory
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath", "dirpath", "directorypath")]
        protected string[] _Path { get; set; }

        [TaskParameter]
        [Keys("tolog", "log")]
        protected bool _ToLog { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            TargetSequence(_Path, GetDirectoryAction);
        }

        private void GetDirectoryAction(string target)
        {
            DirectoryInfo dInfo = new System.IO.DirectoryInfo(target);
            DirectorySecurity security = dInfo.GetAccessControl();
            AuthorizationRuleCollection rules = security.GetAccessRules(true, false, typeof(NTAccount));

            string name = Path.GetFileName(target);
            string fullPath = Path.GetFullPath(target);

            string access = string.Join('/',
                AccessRuleSummary.FromAccessRules(rules, PathType.Directory).
                    Select(x => x.ToString()));

            string owner = security.GetOwner(typeof(NTAccount)).Value;
            string inherited = (!security.AreAccessRulesProtected).ToString();

            FileAttributes attr = System.IO.File.GetAttributes(target);
            string attributes = string.Format("[{0}]ReadOnly [{1}]Hidden [{2}]System",
                (attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ? "x" : " ",
                (attr & FileAttributes.Hidden) == FileAttributes.Hidden ? "x" : " ",
                (attr & FileAttributes.System) == FileAttributes.System ? "x" : " ");

            long size = dInfo.GetFiles("*", SearchOption.AllDirectories).
                Select(x => x.Length).
                Sum();
            int fileCount = dInfo.GetFiles("*", SearchOption.AllDirectories).Length;
            int directoryCount = dInfo.GetDirectories("*", SearchOption.AllDirectories).Length;

            var sb = new StringBuilder();
            sb.AppendLine($"{this.TaskName} Directory summary");
            sb.AppendLine($"  Name           : {name}");
            sb.AppendLine($"  Path           : {fullPath}");
            sb.AppendLine($"  Access         : {access}");
            sb.AppendLine($"  Owner          : {owner}");
            sb.AppendLine($"  Inherited      : {inherited}");
            sb.AppendLine($"  Attributes     : {attributes}");
            sb.AppendLine($"  Size           : {size}");
            sb.AppendLine($"  FileCount      : {fileCount}");
            sb.Append($"  DirectoryCount : {directoryCount}");

            if (_ToLog)
            {
                Manager.WriteLog(LogLevel.Info, sb.ToString());
            }
            else
            {
                Manager.WriteStandard(sb.ToString());
            }
        }
    }
}
