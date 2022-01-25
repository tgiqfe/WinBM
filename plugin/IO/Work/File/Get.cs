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
using System.IO.Compression;

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

        //  ########################

        [TaskParameter]
        [Keys("isbinary", "binary", "bin")]
        protected bool? _IsBinary { get; set; }

        [TaskParameter]
        [Keys("textblock", "block")]
        protected int? _TextBlock { get; set; }

        [TaskParameter]
        [Keys("compress")]
        protected bool? _Compress { get; set; }

        const int BUFF_SIZE = 4096;

        public override void MainProcess()
        {
            this.Success = true;

            TargetSequence(_Path, GetFileAction);
        }

        private void GetFileAction(string target)
        {
            string outputText = _IsBinary ?? false ?
                ReadFileBinary(target) :
                GetFileInfo(target);

            if (_ToLog)
            {
                Manager.WriteLog(LogLevel.Info, outputText);
            }
            else
            {
                Manager.WriteStandard(outputText);
            }
        }

        /// <summary>
        /// ファイル情報取得
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private string GetFileInfo(string target)
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

            return sb.ToString();
        }

        /// <summary>
        /// バイナリモードでの読み込み
        /// </summary>
        /// <param name="target"></param>
        private string ReadFileBinary(string target)
        {
            string retText = "";

            using (var fs = new FileStream(target, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            using (var ms = new MemoryStream())
            {
                if (fs.Length < int.MaxValue)
                {
                    byte[] buffer = new byte[BUFF_SIZE];
                    int readed = 0;

                    if (_Compress ?? false)
                    {
                        //  圧縮する場合
                        using (var gs = new GZipStream(ms, CompressionMode.Compress))
                        {
                            while ((readed = br.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                gs.Write(buffer, 0, readed);
                            }
                        }
                    }
                    else
                    {
                        //  非圧縮
                        while ((readed = br.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, readed);
                        }
                    }
                    retText = BitConverter.ToString(ms.ToArray()).Replace("-", "");
                }
                else
                {
                    Console.Error.WriteLine("Size Over.");
                    return null;
                }
            }

            //  テキストブロック化
            if ((this._TextBlock ?? 0) > 0)
            {
                int count = (int)_TextBlock;
                StringBuilder sb = new StringBuilder();
                using (var sr = new StringReader(retText))
                {
                    int readed = 0;
                    char[] buffer = new char[count];
                    while ((readed = sr.Read(buffer, 0, count)) > 0)
                    {
                        sb.AppendLine(new string(buffer, 0, readed));
                    }
                }
                retText = sb.ToString();
            }

            return retText;
        }
    }
}
