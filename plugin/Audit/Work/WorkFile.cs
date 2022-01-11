using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;

namespace Audit.Work
{
    internal class WorkFile : AuditTaskWork
    {
        protected delegate void TargetFileAction(
            string path, Dictionary<string, string> dictaionry, int count);

        protected delegate void SrcDstFileAction(
            string source, string destination, Dictionary<string, string> dictaionry, int count);

        /// <summary>
        /// 対象ファイルに対するシーケンシャル処理
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="dictionary"></param>
        /// <param name="targetFileAction"></param>
        protected void TargetSequence(
            string[] paths, Dictionary<string, string> dictionary, TargetFileAction targetFileAction)
        {
            int count = 0;

            foreach (string path in paths)
            {
                if (Path.GetFileName(path).Contains("*"))
                {
                    //  対象ファイルの親フォルダーが存在しない場合
                    string parent = Path.GetDirectoryName(path);
                    if (!System.IO.Directory.Exists(parent))
                    {
                        Manager.WriteLog(LogLevel.Warn, "Parent on target is Missing. \"{0}\"", parent);
                        Success = false;
                        continue;
                    }

                    //  ワイルドカード指定
                    System.Text.RegularExpressions.Regex wildcard = IO.Lib.Wildcard.GetPattern(path);
                    System.IO.Directory.GetFiles(parent).
                        Where(x => wildcard.IsMatch(x)).
                        ToList().
                        ForEach(x => targetFileAction(x, dictionary, ++count));
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
                    targetFileAction(path, dictionary, ++count);
                }
            }
        }

        /// <summary>
        /// 対象ファイルに対するシーケンシャル処理。src/dst指定
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="destination"></param>
        /// <param name="dictionary"></param>
        /// <param name="srcDstFileAction"></param>
        protected void SrcDstSequence(
            string[] sources, string destination, Dictionary<string, string> dictionary, SrcDstFileAction srcDstFileAction)
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
                    System.IO.Directory.GetFiles(parent).
                        Where(x => wildcard.IsMatch(x)).
                        ToList().
                        ForEach(x => srcDstFileAction(x, destination, dictionary, ++count));
                }
                else
                {
                    //  Sourceが存在しない場合
                    if (!System.IO.File.Exists(source))
                    {
                        Manager.WriteLog(LogLevel.Error, "Source target is Missing. \"{0}\"", source);
                        Success = false;
                        continue;
                    }

                    srcDstFileAction(source, destination, dictionary, ++count);
                }
            }
        }
    }
}
