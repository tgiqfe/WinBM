using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;

namespace OSVersion.Require.Windows
{
    internal class Within : TaskJob
    {
        [TaskParameter(Mandatory = true, EqualSign = '~', Delimiter = '\n')]
        [Keys("range", "ranges")]
        protected Dictionary<string, string> _Range { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            //  _Rangeをstring配列Listに変換。
            List<string[]>  rangeList = 
                _Range.Select(x => new string[2] { x.Key, x.Value }).ToList();





        }
    }
}
