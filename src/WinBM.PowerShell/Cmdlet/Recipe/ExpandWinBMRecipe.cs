using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using WinBM.Recipe;

namespace WinBM.PowerShell.Cmdlet.Recipe
{
    [Cmdlet(VerbsData.Expand, "WinBMRecipe")]
    public class ExpandWinBMRecipe : PSCmdlet
    {
        [Parameter(Position = 0)]
        public string Path { get; set; }

        protected override void ProcessRecord()
        {
            var list = WinBM.Recipe.Page.Load(this.Path);

            WriteObject(list);
        }
    }
}
