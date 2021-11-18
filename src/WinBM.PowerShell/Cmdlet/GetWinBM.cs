using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;

namespace WinBM.PowerShell.Cmdlet.Recipe
{
    [Cmdlet(VerbsCommon.Get, "WinBM")]
    public class GetWinBM : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("File")]
        public string RecipeFile { get; set; }

        protected override void ProcessRecord()
        {
            string recipeFile = RecipeFile;

            List<WinBM.Recipe.Page> list = null;
            if (File.Exists(recipeFile))
            {
                using (var sr = new StreamReader(recipeFile, Encoding.UTF8))
                {
                    list = WinBM.Recipe.Page.Deserialize(sr);
                }
            }
            else if (Directory.Exists(recipeFile))
            {
                foreach (string file in Directory.GetFiles(recipeFile))
                {
                    string extension = Path.GetExtension(file).ToLower();
                    if (extension == ".yml" || extension == ".yaml")
                    {
                        using (var sr = new StreamReader(file, Encoding.UTF8))
                        {
                            list ??= new List<WinBM.Recipe.Page>();
                            list.AddRange(WinBM.Recipe.Page.Deserialize(sr));
                        }
                    }
                }
            }

            WriteObject(list);
        }
    }
}
