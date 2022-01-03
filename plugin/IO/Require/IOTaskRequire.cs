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

        protected delegate void TargetDirectoryAction(string path);

        /// <summary>
        /// 対象ファイルに対するシーケシャル処理
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="targetFileAction"></param>
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
                    }

                    targetFileAction(path);
                }
            }
        }

        /// <summary>
        /// 対象フォルダーに対するシーケンシャル処理
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="targetDirectoryAction"></param>
        protected void TargetDirectoryProcess(string[] paths, TargetDirectoryAction targetDirectoryAction)
        {
            foreach (string path in paths)
            {
                if (Path.GetFileName(path).Contains("*"))
                {
                    Manager.WriteLog(LogLevel.Info, "{0} Wildcard path.", this.TaskName);

                    //  対象フォルダーの親フォルダーが存在しない場合
                    string parent = System.IO.Path.GetDirectoryName(path);
                    if (!System.IO.Directory.Exists(parent))
                    {
                        Manager.WriteLog(LogLevel.Warn, "Parent on target is Missing. \"{0}\"", parent);
                        Success = false;
                        continue;
                    }

                    //  ワイルドカード指定
                    System.Text.RegularExpressions.Regex wildcard = Wildcard.GetPattern(path);
                    System.IO.Directory.GetDirectories(parent).
                        Where(x => wildcard.IsMatch(x)).
                        ToList().
                        ForEach(x => targetDirectoryAction(x));
                }
                else
                {
                    //  対象フォルダーが存在しない場合
                    if (!System.IO.Directory.Exists(path))
                    {
                        Manager.WriteLog(LogLevel.Warn, "Target is Missing. \"{0}\"", path);
                        Success = false;
                        continue;
                    }

                    targetDirectoryAction(path);
                }
            }
            return;
        }
    }
}
