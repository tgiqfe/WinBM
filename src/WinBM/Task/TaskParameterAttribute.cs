using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBM.Task
{
    public class TaskParameterAttribute : Attribute
    {
        /// <summary>
        /// 必須パラメータ。これがtrueならば問答無用で必須
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// このパラメータが1以上のパラメータのいずれかが指定されていたらOK。
        /// 値が同じ場合は、同じ値の中で全て指定されている必要あり
        /// </summary>
        public int MandatoryAny { get; set; }

        /// <summary>
        /// 環境変数を解決するかどうか
        /// </summary>
        public bool Resolv { get; set; }

        /// <summary>
        /// string配列の場合のデリミタ
        /// </summary>
        public char Delimiter { get; set; } = ',';

        /// <summary>
        /// 左辺/右辺を分ける記号
        /// </summary>
        public char EqualSign { get; set; }

        /// <summary>
        /// 負の数を許可しないかどうか。trueの場合、「-」不許可にするので正しいパラメータとして認識しない
        /// </summary>
        public bool Unsigned { get; set; }

        //  MaxValuとMinMalueも設定しても良いかも
    }
}