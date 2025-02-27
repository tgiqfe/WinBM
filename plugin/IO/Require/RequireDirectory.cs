﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;

namespace IO.Require
{
    internal class RequireDirectory : IOTaskRequire
    {
        protected delegate void TargetDirectoryAction(string path);

        protected delegate void SrcDstDirectoryAction(string source, string destination);

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
                    System.Text.RegularExpressions.Regex wildcard = IO.Lib.Wildcard.GetPattern(path);
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
        }

        /// <summary>
        /// 対象フォルダーに対するシーケンス処理。src/dst指定
        /// </summary>
        /// <param name="sourcePaths"></param>
        /// <param name="destinationPath"></param>
        /// <param name="srcDstDirectoryAction"></param>
        protected void SrcDstDirectoryProcess(string[] sourcePaths, string destinationPath, SrcDstDirectoryAction srcDstDirectoryAction)
        {
            foreach (string source in sourcePaths)
            {
                if (Path.GetFileName(source).Contains("*"))
                {
                    Manager.WriteLog(LogLevel.Info, "{0} Wildcard path.", this.TaskName);

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
                        ForEach(x => srcDstDirectoryAction(x, destinationPath));
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

                    srcDstDirectoryAction(source, destinationPath);
                }
            }
        }
    }
}
