using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO.Lib;
using WinBM;
using WinBM.Task;

namespace IO.Require
{
    internal class IOTaskRequire : TaskJob
    {
        protected delegate void TargetFileAction(string path);

        protected delegate void SrcDstFileAction(string source, string destination);

        protected delegate void TargetDirectoryAction(string path);

        protected delegate void SrcDstDirectoryAction(string source, string destination);

        protected void TargetFileProcess(string[] paths, TargetFileAction targetFileAction)
        {
            foreach (string path in paths)
            {
                if (Path.GetFileName(path).Contains("*"))
                {
                    Manager.WriteLog(LogLevel.Info, "{0} Wildcard path.", this.TaskName);

                    //  対象ファイルの親フォルダーが存在しない場合
                    string parent = Path.GetDirectoryName(path);
                    if (!System.IO.Directory.Exists(parent))
                    {
                        Manager.WriteLog(LogLevel.Warn, "Parent on target is Missing. \"{0}\"", parent);
                        Success = false;
                        continue;
                        //  ↑returnにするかを検討中。恐らくこのままだが、一応変更する可能性があるのでコメントだけ残す。
                    }

                    //  ワイルドカード指定
                    System.Text.RegularExpressions.Regex wildcard = Wildcard.GetPattern(path);
                    System.IO.Directory.GetFiles(parent).
                        Where(x => wildcard.IsMatch(x)).
                        ToList().
                        ForEach(x => targetFileAction(x));
                }
                else
                {
                    //  対象ファイルが存在しない場合
                    if (!System.IO.File.Exists(path))
                    {
                        Manager.WriteLog(LogLevel.Warn, "Target is Missing. \"{0}\"", path);
                        Success = false;
                        continue;
                        //  ↑returnにするかを検討中。恐らくこのままだが、一応変更する可能性があるのでコメントだけ残す。
                    }

                    targetFileAction(path);
                }
            }
        }





    }
}
