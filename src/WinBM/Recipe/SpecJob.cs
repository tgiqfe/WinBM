using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace WinBM.Recipe
{
    public class SpecJob : SpecBase
    {
        public enum FailedAction
        {
            Stop, Abort, Continue
        };

        /// <summary>
        /// 失敗時の動作。
        ///   Stop: 現在のPageを終了し次のPageへ進む。Requireの場合のデフォルト値
        ///   Abort: 現在のPageを終了。以降の全Pageも終了。
        ///   Continue: 終了せず、次のReceipへ進む。Workの場合のデフォルト値
        /// </summary>
        [YamlMember(Alias = "failed")]
        public FailedAction? Failed { get; set; }

        [YamlMember(Alias = "progress")]
        public bool? Progress { get; set; }
    }
}
