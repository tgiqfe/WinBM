using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

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
        /// treuの場合、Config/Output/Require/Work単位で、pauseさせる。
        /// </summary>
        [YamlMember(Alias = "step")]
        public bool? Step { get; set; }

        /// <summary>
        /// 優先順位。値が低いほど優先順位が高い
        /// Config, Output ⇒ spec内の各宣言で、taskが重複した場合に、Proprityが低いほうを使用
        /// Job ⇒ Priorityの昇順の順番で実行。
        /// </summary>
        [YamlMember(Alias = "priority")]
        public string Priority { get; set; }

        public int GetPriority()
        {
            return CalculateData.ComputeInt(this.Priority);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
