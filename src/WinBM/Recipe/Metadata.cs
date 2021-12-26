using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using WinBM.Lib;

namespace WinBM.Recipe
{
    public class Metadata
    {
        /// <summary>
        /// Pageの名前 (必須)
        /// </summary>
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Pageの説明 (任意)
        /// </summary>
        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        /// <summary>
        /// trueの場合、このPageは読み込まない。デバッグ用
        /// </summary>
        [YamlMember(Alias = "skip")]
        public bool? Skip { get; set; }

        /// <summary>
        /// treuの場合、Env/Config/Output/Require/Work単位で、pauseさせる。
        /// </summary>
        [YamlMember(Alias = "step")]
        public bool? Step { get; set; }

        /// <summary>
        /// 優先順位。値が低いほど優先順位が高い
        /// Config, Output ⇒ spec内の各宣言で、taskが重複した場合に、Proprityが低いほうを使用
        /// Env, Job ⇒ Priorityの昇順の順番で実行。
        /// </summary>
        [YamlMember(Alias = "priority")]
        public string Priority { get; set; }

        public override string ToString()
        {
            return this.Name;
        }

        #region Calc priority

        /// <summary>
        /// Priority値を取得
        /// </summary>
        /// <returns></returns>
        public int GetPriority()
        {
            if (string.IsNullOrEmpty(this.Priority))
            {
                return 0;
            }

            string priority = this.Priority;
            for (int i = 0; i < 5 && priority.Contains("%"); i++)
            {
                FileScope.
                    FileScopeList?.
                    Where(x => x.IsMathPath(_filePath)).
                    ToList().
                    ForEach(x => x.Resolv(ref priority));
                priority = Environment.ExpandEnvironmentVariables(priority);
            }

            return CalculateText.ToInt(priority);
        }

        /// <summary>
        /// Fileスコープ内の変数を使用してPriority値を計算する為に必要な、ファイルパス
        /// </summary>
        /// <param name="filePath"></param>
        private string _filePath = null;

        /// <summary>
        /// ファイルパスをセット。デシリアライズ時にセット。
        /// </summary>
        /// <param name="filePath"></param>
        public void SetFilePath(string filePath)
        {
            this._filePath = filePath;
        }

        #endregion
    }
}
