using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management.Automation;

namespace WinBM.PowerShell.Cmdlet.Binary
{
    [Cmdlet(VerbsCommunications.Write, "WinBMFileBinary")]
    public class WriteWinBMFileBinary : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Text { get; set; }


    }
}
