using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using System.Text.RegularExpressions;
using WinBM.PowerShell.Lib.TestWinBMYaml;

namespace WinBM.PowerShell.Cmdlet
{
    [Cmdlet(VerbsDiagnostic.Test, "WinBM")]
    public class TestWinBM : PSCmdlet
    {
        [Parameter(Position = 0), Alias("File")]
        public string RecipeFile { get; set; }

        [Parameter]
        public SwitchParameter Quiet { get; set; }

        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            //  ここにTestYaml処理系を追加
        }

        protected override void EndProcessing()
        {
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
