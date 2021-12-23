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
    [Cmdlet(VerbsData.Compress, "WinBMRecipe")]
    public class ComparessWinBMRecipe : PSCmdlet
    {
        [Parameter(ValueFromPipeline = true)]
        public WinBM.Recipe.Page[] Page { get; set; }

        [Parameter]
        public string Path { get; set; }

        protected override void ProcessRecord()
        {
            WinBM.Recipe.Page.Save(this.Page, this.Path);
        }
    }
}
