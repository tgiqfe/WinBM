using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;

namespace Audit.Work
{
    internal class WorkDirectory : AuditTaskWork
    {
        protected delegate void TargetDirectoryAction(string path, Dictionary<string, string> dictaionry, int count);

        protected delegate void SrcDstDirectoryAction(string source, string destination, Dictionary<string, string> dictaionry, int count);

        /// <summary>
        /// 対象フォルダーに対するシーケンシャル処理
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="dictionary"></param>
        /// <param name="targetDirectoryAction"></param>
        protected void TargetSequence(
            string[] paths, Dictionary<string, string> dictionary, TargetDirectoryAction targetDirectoryAction)
        {
            int count = 0;

            foreach (string path in paths)
            {
                if (Path.GetFileName(path).Contains("*"))
                {
                    //  対象フォルダーの親フォルダーが存在しない場合
                    string parent = System.IO.Path.GetDirectoryName(path);
                    if (!System.IO.Directory.Exists(parent))
                    {
                        Manager.WriteLog(LogLevel.Warn, "Parent on target is Missing. \"{0}\"", parent);
                        Success = false;
                        continue;
                    }

                    //  ワイルドカード指定
                    System.Text.RegularExpressions.Regex wildcard = IO.Lib.Wildcard.GetPattern(path);
                    System.IO.Directory.GetDirectories(parent).
                        Where(x => wildcard.IsMatch(x)).
                        ToList().
                        ForEach(x => targetDirectoryAction(x, dictionary, ++count));
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

                    targetDirectoryAction(path, dictionary, ++count);
                }
            }
        }

        /// <summary>
        /// 対象フォルダーに対するシーケンシャル処理。src/dst指定
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="destination"></param>
        /// <param name="dictionary"></param>
        /// <param name="srcDstDirectoryAction"></param>
        protected void SrcDstSequence(
            string[] sources, string destination, Dictionary<string, string> dictionary, SrcDstDirectoryAction srcDstDirectoryAction)
        {
            int count = 0;

            foreach (string source in sources)
            {
                if (Path.GetFileName(source).Contains("*"))
                {
                    //  Sourceの親フォルダーが存在しない場合
                    string parent = Path.GetDirectoryName(source);
                    if (!System.IO.Directory.Exists(parent))
                    {
                        Manager.WriteLog(LogLevel.Warn, "Parent on target is Missing. \"{0}\"", parent);
                        Success = false;
                        continue;
                    }

                    System.Text.RegularExpressions.Regex wildcard = IO.Lib.Wildcard.GetPattern(source);
                    System.IO.Directory.GetDirectories(parent).
                        Where(x => wildcard.IsMatch(x)).
                        ToList().
                        ForEach(x => srcDstDirectoryAction(x, destination, dictionary, ++count));
                }
                else
                {
                    //  Sourceが存在しない場合
                    if (!System.IO.Directory.Exists(source))
                    {
                        Manager.WriteLog(LogLevel.Error, "Source target is Missing. \"{0}\"", source);
                        Success = false;
                        continue;
                    }

                    srcDstDirectoryAction(source, destination, dictionary, ++count);
                }
            }
        }
    }
}
