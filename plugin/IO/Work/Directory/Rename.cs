using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace IO.Work.Directory
{
    internal class Rename : TaskJob
    {
        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string _Path { get; set; }

        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("newname", "new")]
        protected string _NewName { get; set; }


        public override void MainProcess()
        {
            this.Success = true;

            //  _Pathはワイルドカード指定も複数指定も無し

            if (_NewName.Contains("\\"))
            {
                _NewName = Path.GetFileName(_NewName);
            }
            RenameDirectoryAction(_Path);
        }

        private void RenameDirectoryAction(string target)
        {
            try
            {
                string newPath = Path.Combine(Path.GetDirectoryName(target), _NewName);

                //  変更後の名前のファイルが存在する場合
                if (System.IO.Directory.Exists(newPath))
                {
                    Manager.WriteLog(LogLevel.Info, "New name is already exists. \"{0}\"", newPath);
                    return;
                }

                FileSystem.RenameDirectory(_Path, _NewName);
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
