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
        [Parameter(Position = 0)]
        public Page[] Page { get; set; }

        [Parameter(Position = 1)]
        public string Path { get; set; }

        protected override void ProcessRecord()
        {
            if (this.Page != null)
            {
                string[] candidate_db = { ".db", "dat", ".recipe" };
                string extension = System.IO.Path.GetExtension(this.Path);
                if (candidate_db.Any(x => x.Equals(extension, StringComparison.OrdinalIgnoreCase)))
                {
                    WinBM.Recipe.Page.Save(this.Page, this.Path);
                }
            }
        }
    }
}
