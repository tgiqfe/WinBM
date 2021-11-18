using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using WinBM.PowerShell.Manifest;
using System.Reflection;
using System.Threading;

namespace WinBM.PowerShell.Manifest
{
    [Cmdlet(VerbsData.Save, "Manifest")]
    public class SaveManifest : PSCmdlet
    {
        private ProgressRecord _progress = null;
        private int _total = 6;
        int _count = 0;

        protected override void ProcessRecord()
        {
            this._progress = new ProgressRecord(
                1,
                Assembly.GetExecutingAssembly().GetName().Name,
                "init");

            WriteProgress();
            var proj = new Project();

            WriteProgress();
            proj.CreateManifestFile();

            WriteProgress();
            proj.CopyProjectDir();

            WriteProgress();
            proj.CopyScriptDir();

            WriteProgress();
            proj.CopyFormatDir();

            WriteProgress();
            proj.CopyHelpDir();
        }

        private void WriteProgress()
        {
            _progress.PercentComplete = (int)(100.0 / _total * _count++);
            _progress.StatusDescription = $"進捗 {_progress.PercentComplete} %";
            WriteProgress(_progress);
            Thread.Sleep(10);
        }
    }
}
