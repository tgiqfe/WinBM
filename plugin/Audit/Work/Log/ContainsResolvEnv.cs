using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using IO.Lib;

namespace Audit.Work.Log
{
    /// <summary>
    /// 基本的にContainsのコピー。
    /// _ContainsTextを環境変数解決に対応する以外は同じ。
    /// </summary>
    internal class ContainsResolvEnv : AuditTaskWork
    {
        /// <summary>
        /// 読み込み対象のログファイルへのパス
        /// </summary>
        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string _Path { get; set; }

        /// <summary>
        /// ログファイルに含まれる文字列
        /// </summary>
        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("contains", "contain", "containstext", "text")]
        protected string _ContainsText { get; set; }

        /// <summary>
        /// 大文字/小文字を無視
        /// </summary>
        [TaskParameter]
        [Keys("ignorecase")]
        protected bool _IgnoreCase { get; set; }

        /// <summary>
        /// 監査対象のログファイルの文字コード。
        /// 監査結果を出力するファイルは、UTF-8固定。
        /// </summary>
        [TaskParameter]
        [Keys("encoding", "encode", "enc")]
        protected string _Encoding { get; set; }

        private Encoding enc = null;

        /// <summary>
        /// 結果を反転
        /// </summary>
        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            //  前回読み込みした場所を取得
            int position = LoadSinceDB(_Path);

            //  結果情報
            var dictionary = new Dictionary<string, string>();
            dictionary["path"] = _Path;
            dictionary["contains"] = _ContainsText;

            //  ログファイルを読み込み
            enc = FileEncoding.Get(_Encoding);
            try
            {
                using (var sr = new StreamReader(_Path, enc))
                {
                    int count = 0;
                    string readline = "";
                    var comparer = _IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

                    while ((readline = sr.ReadLine()) != null)
                    {
                        count++;
                        if (count <= position) { continue; }

                        //  ログ内容に特定の文字列が含まれているかチェック
                        if (readline.Contains(_ContainsText, comparer))
                        {
                            this.Success = true;
                            dictionary["line"] = count.ToString();
                            dictionary["log"] = readline;
                            break;
                        }
                    }
                    SaveSinceDB(_Path, count);
                }
            }
            catch
            {
                string exceptionMessage = string.Format("Error occurd when during read LOG. \"{0}\"", _Path);
                dictionary["exception"] = exceptionMessage;
                Manager.WriteLog(LogLevel.Error, exceptionMessage);
            }
            AddAudit(dictionary, this._Invert);
        }
    }
}
