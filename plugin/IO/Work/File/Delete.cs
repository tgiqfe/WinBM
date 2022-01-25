using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using IO.Lib;

namespace IO.Work.File
{
    /// <summary>
    /// ファイル削除
    /// Success条件⇒ファイルの削除試行でtrue。
    /// 空ファイルパスしか指定していない場合(empty or null)はfalse。
    ///   (但し、普通は空指定に場合はCheckParam()で失敗する為、そういうパターンは無いはず)
    /// 削除処理に失敗(例外発生)でfalse
    /// 指定したファイルパスが存在しない場合はtrue
    /// </summary>
    internal class Delete : WorkFile
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        [TaskParameter]
        [Keys("force", "forse")]
        protected bool _Force { get; set; }

        [TaskParameter]
        [Keys("recycle", "recyclebin", "trash", "trashbox")]
        protected bool _Recycle { get; set; }

        [TaskParameter(Resolv = true)]
        [Keys("exclude", "excludepath", "excludes", "excludepaths", "expath", "expaths")]
        protected string[] _Exclude { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            TargetSequence(_Path, DeleteFileAction);
        }

        private void DeleteFileAction(string target)
        {
            //  Excludeに絶対パスorファイル名が一致していたらスキップ
            if (_Exclude?.Length > 0)
            {
                if (_Exclude.Any(x =>
                     (x.Contains("\\") && x.Equals(target, StringComparison.OrdinalIgnoreCase)) ||
                     (!x.Contains("\\") && x.Equals(Path.GetFileName(target), StringComparison.OrdinalIgnoreCase))))
                {
                    return;
                }
            }

            try
            {
                if (Manager.Interactive && _Recycle)
                {
                    //  ゴミ箱に移動する場合。(読み取り専用でも移動可)
                    FileSystem.DeleteFile(
                        target,
                        UIOption.OnlyErrorDialogs,
                        RecycleOption.SendToRecycleBin,
                        UICancelOption.DoNothing);
                }
                else
                {
                    //  ゴミ箱に移動しない場合
                    System.IO.File.Delete(target);
                }
            }
            catch (UnauthorizedAccessException uae)
            {
                Manager.WriteLog(LogLevel.Debug, "{0} {1}", this.TaskName, uae.Message);

                if (_Force)
                {
                    Manager.WriteLog(LogLevel.Info, "Remove readonly attribute: \"{0}\"", target);
                    new FileInfo(target).IsReadOnly = false;

                    if (Manager.Interactive && _Recycle)
                    {
                        //  ゴミ箱に移動する場合。(読み取り専用でも移動可)
                        FileSystem.DeleteFile(
                            target,
                            UIOption.OnlyErrorDialogs,
                            RecycleOption.SendToRecycleBin,
                            UICancelOption.DoNothing);
                    }
                    else
                    {
                        //  ゴミ箱に移動しない場合
                        System.IO.File.Delete(target);
                    }
                }
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
                this.Success = false;
            }
        }
    }

    /// <summary>
    /// Deleteへのエイリアス
    /// </summary>
    internal class Remove : Delete
    {
        protected override bool IsAlias { get { return true; } }
    }
}
