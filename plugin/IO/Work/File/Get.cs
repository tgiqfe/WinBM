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

namespace IO.Work.File
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Get : WorkFile
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        [TaskParameter]
        [Keys("tolog", "log")]
        protected bool _ToLog { get; set; }

        [TaskParameter]
        [Keys("binary", "bin")]

        public override void MainProcess()
        {
            this.Success = true;

            TargetFileProcess(_Path, GetFileAction);
        }

        private void GetFileAction(string target)
        {
            FileInfo fInfo = new System.IO.FileInfo(target);
            FileSecurity security = fInfo.GetAccessControl();
            AuthorizationRuleCollection rules = security.GetAccessRules(true, false, typeof(NTAccount));

            string name = Path.GetFileName(target);
            string fullPath = Path.GetFullPath(target);

            string access = string.Join('/',
                AccessRuleSummary.FromAccessRules(rules, PathType.File).
                    Select(x => x.ToString()));

            string owner = security.GetOwner(typeof(NTAccount)).Value;
            string inherited = (!security.AreAccessRulesProtected).ToString();

            FileAttributes attr = System.IO.File.GetAttributes(target);
            string attributes = string.Format("[{0}]ReadOnly [{1}]Hidden [{2}]System",
                (attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ? "x" : " ",
                (attr & FileAttributes.Hidden) == FileAttributes.Hidden ? "x" : " ",
                (attr & FileAttributes.System) == FileAttributes.System ? "x" : " ");

            string hash = "";
            using (var fs = new System.IO.FileStream(target, FileMode.Open, FileAccess.Read))
            {
                var sha256 = SHA256.Create();

                hash = BitConverter.ToString(sha256.ComputeHash(fs)).Replace("-", "");
                sha256.Clear();
            }
            bool isSecurityBlock = FileControl.CheckSecurityBlock(target);
            long size = fInfo.Length;

            var sb = new StringBuilder();
            sb.AppendLine($"{this.TaskName} File summary");
            sb.AppendLine($"  Name           : {name}");
            sb.AppendLine($"  Path           : {fullPath}");
            sb.AppendLine($"  Access         : {access}");
            sb.AppendLine($"  Owner          : {owner}");
            sb.AppendLine($"  Inherited      : {inherited}");
            sb.AppendLine($"  Attributes     : {attributes}");
            sb.AppendLine($"  SHA256Hash     : {hash}");
            sb.AppendLine($"  SecurityBlock  : {isSecurityBlock}");
            sb.Append($"  Size           : {size}");

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
