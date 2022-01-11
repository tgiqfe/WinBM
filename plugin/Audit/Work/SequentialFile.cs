using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Work
{
    internal class SequentialFile : AuditTaskWork
    {
        protected delegate void TargetFileAction(string path, Dictionary<string, string> dictaionry, int count);

        protected delegate void SrcDstFileAction(string source, string destination, Dictionary<string, string> dictaionry, int count);


        /*
         * ここにシーケンシャル用メソッドを実装予定
         */
    }
}
