using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;

namespace Audit.Work
{
    internal class WorkFile : AuditTaskWork
    {
        protected delegate void TargetFileAction(
            string path, Dictionary<string, string> dictaionry, int count);

        protected delegate void SrcDstFileAction(
            string source, string destination, Dictionary<string, string> dictaionry, int count);

        protected void TargetSequence(
            string[] paths, Dictionary<string, string> dictaionry, int count,
            TargetFileAction targetFileAction)
        {
            //  ここにシーケンシャル処理を
        }

        protected void SrcDstSequence(
            string[] sources, string destination, Dictionary<string, string> dictaionry, int count,
            SrcDstFileAction srcDstFileAction)
        {
            //  ここにシーケンシャル処理を
        }
    }
}
