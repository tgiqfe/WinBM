using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using IO.Lib;

namespace IO.Work.Directory
{
    /// <summary>
    /// 新規フォルダー作成
    /// ランダムフォルダー名にも対応
    /// 作成したフォルダーの名前とパスについて、環境変数にセットも可。
    /// ※但し、複数フォルダーを作成する場合、環境変数にセットされるのは最後の1つだけ(毎回上書くので)
    /// </summary>
    internal class Create : TaskJob
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath", "dirpath", "directorypath")]
        protected string[] _Path { get; set; }

        [TaskParameter]
        [Keys("random", "rdm", "randam")]
        protected bool? _Random { get; set; }

        [TaskParameter]
        [Keys("envsetpath", "envpath", "envset", "env")]
        protected string _EnvSetPath { get; set; }

        [TaskParameter]
        [Keys("envsetname", "envname")]
        protected string _EnvSetName { get; set; }

        /// <summary>
        /// 環境変数の適用範囲のスコープ。無指定の場合は[Process]
        /// 以下のスコープのみ指定可能
        /// - Process
        /// - File
        /// - Page
        /// </summary>
        [TaskParameter]
        [Keys("target", "envtarget", "targetenv", "scope", "targetscope", "envscope")]
        [Values("process,proc,proces", "file,recipefile", "page,pag,pege")]
        protected TargetScope _Scope { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            if (_Random ?? false)
            {
                //  Random指定の場合、Pathをフォルダーとして解釈し、その配下にランダムフォルダーを作成。
                _Path = _Path.Select(x => Path.Combine(x, Path.GetRandomFileName())).ToArray();
            }
            _Path.ToList().ForEach(x => CreateDirectoryAction(x));
        }

        private void CreateDirectoryAction(string target)
        {
            SetEnv(target);

            try
            {
                //  ワイルドカード指定は対応しない方針
                if (target.Contains("*"))
                {
                    Manager.WriteLog(LogLevel.Warn, "The path contains a wildcard.");
                    return;
                }

                //  新規作成の対象がすでに存在する場合
                if (System.IO.Directory.Exists(target))
                {
                    Manager.WriteLog(LogLevel.Info, "New target is already exists. \"{0}\"", target);
                    return;
                }

                Manager.WriteLog(LogLevel.Debug, "Create directory: \"{0}\"", target);
                System.IO.Directory.CreateDirectory(target);
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
                this.Success = false;
            }
        }

        /// <summary>
        /// 環境変数セット
        /// </summary>
        /// <param name="path"></param>
        private void SetEnv(string path)
        {
            if (!string.IsNullOrEmpty(_EnvSetPath))
            {
                string envKey = _EnvSetPath;
                string envVal = path;
                switch (_Scope)
                {
                    case TargetScope.File:
                        WinBM.Lib.FileScope.Add(this.FilePath, envKey, envVal);
                        break;
                    case TargetScope.Page:
                        Environment.SetEnvironmentVariable(envKey, envVal, EnvironmentVariableTarget.Process);
                        this.IsPostPage = true;
                        break;
                    case TargetScope.Process:
                    default:
                        Environment.SetEnvironmentVariable(envKey, envVal, EnvironmentVariableTarget.Process);
                        this.IsPostRecipe = true;
                        break;
                }
            }
            if (!string.IsNullOrEmpty(_EnvSetName))
            {
                string envKey = _EnvSetName;
                string envVal = System.IO.Path.GetFileName(path);
                switch (_Scope)
                {
                    case TargetScope.File:
                        WinBM.Lib.FileScope.Add(this.FilePath, envKey, envVal);
                        break;
                    case TargetScope.Page:
                        Environment.SetEnvironmentVariable(envKey, envVal, EnvironmentVariableTarget.Process);
                        this.IsPostPage = true;
                        break;
                    case TargetScope.Process:
                    default:
                        Environment.SetEnvironmentVariable(envKey, envVal, EnvironmentVariableTarget.Process);
                        this.IsPostRecipe = true;
                        break;
                }
            }
        }

        /// <summary>
        /// スコープ[Page]でセットした場合、Page内の処理終了後に削除
        /// </summary>
        public override void PostSpec()
        {
            if (!string.IsNullOrEmpty(_EnvSetPath))
            {
                Environment.SetEnvironmentVariable(_EnvSetPath, null, EnvironmentVariableTarget.Process);
            }
            if (!string.IsNullOrEmpty(_EnvSetName))
            {
                Environment.SetEnvironmentVariable(_EnvSetName, null, EnvironmentVariableTarget.Process);
            }
        }

        /// <summary>
        /// スコープ[Process]でセットした場合、全Page終了後に削除
        /// </summary>
        public override void PostPage()
        {
            if (!string.IsNullOrEmpty(_EnvSetPath))
            {
                Environment.SetEnvironmentVariable(_EnvSetPath, null, EnvironmentVariableTarget.Process);
            }
            if (!string.IsNullOrEmpty(_EnvSetName))
            {
                Environment.SetEnvironmentVariable(_EnvSetName, null, EnvironmentVariableTarget.Process);
            }
        }
    }

    internal class New : Create
    {
        protected override bool IsAlias { get { return true; } }
    }
}
