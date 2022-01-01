using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using WinBM.PowerShell.Lib.MDtoRecipe;
using System.IO;

namespace WinBM.PowerShell.Cmdlet.Recipe
{
    [Cmdlet(VerbsData.ConvertFrom, "WinBMinMarkdown")]
    public class ConvertFromWinBMinMarkdown : PSCmdlet
    {
        [Parameter(Mandatory = true), Alias("Source")]
        public string SourceDirectory { get; set; }

        [Parameter(Mandatory = true), Alias("Destination")]
        public string DestinationDirectory { get; set; }

        [Parameter, Alias("Exclude")]
        public string[] ExcludePaths { get; set; }

        protected override void ProcessRecord()
        {
            PageCollection pages = new PageCollection();
            foreach (string filePath in Directory.GetFiles(SourceDirectory, "*.md", SearchOption.AllDirectories))
            {
                if (ExcludePaths?.Any(x => x.Equals(filePath, StringComparison.OrdinalIgnoreCase)) ?? false)
                {
                    continue;
                }
                pages.Add(filePath);
            }
            foreach (var recipe in pages.ToRecipeFileList())
            {
                recipe.Write(DestinationDirectory);
            }
        }
    }
}
