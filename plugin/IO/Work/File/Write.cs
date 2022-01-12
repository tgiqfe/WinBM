using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using IO.Lib;
using System.IO;

namespace IO.Work.File
{
    internal class Write : WorkFile
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        [TaskParameter(Mandatory = true)]
        [Keys("text", "txt")]
        protected string _Text { get; set; }

        [TaskParameter]
        [Keys("append", "apend", "apnd")]
        protected bool _Append { get; set; }

        [TaskParameter]
        [Keys("encoding", "encode", "enc")]
        protected string _Encoding { get; set; }

        private Encoding enc = null;

        public override void MainProcess()
        {
            this.Success = true;

            //  文字コードセット
            enc = FileEncoding.Get(_Encoding);

            TargetFileProcess(_Path, WriteFileAction);
        }

        private void WriteFileAction(string target)
        {
            //  対象ファイルの親フォルダーが存在しない場合
            string parent = System.IO.Path.GetDirectoryName(target);
            if (!System.IO.Directory.Exists(parent))
            {
                System.IO.Directory.CreateDirectory(parent);
            }

            try
            {
                using (var sw = new StreamWriter(target, _Append, enc))
                {
                    sw.WriteLine(_Text);
                }
                this.Success = true;
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
                this.Success = false;
            }
        }
    }
}
