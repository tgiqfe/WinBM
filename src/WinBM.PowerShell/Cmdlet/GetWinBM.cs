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

        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            List<WinBM.Recipe.Page> list = null;
            if (File.Exists(RecipeFile))
            {
                /*
                using (var sr = new StreamReader(RecipeFile, Encoding.UTF8))
                {
                    list = WinBM.Recipe.Page.Deserialize(sr);
                }
                */
                list = WinBM.Recipe.Page.Deserialize(RecipeFile);
            }
            else if (Directory.Exists(RecipeFile))
            {
                foreach (string filePath in Directory.GetFiles(RecipeFile))
                {
                    string extension = Path.GetExtension(filePath).ToLower();
                    if (extension == ".yml" || extension == ".yaml")
                    {
                        using (var sr = new StreamReader(filePath, Encoding.UTF8))
                        {
                            list ??= new List<WinBM.Recipe.Page>();
                            list.AddRange(WinBM.Recipe.Page.Deserialize(sr));
                        }
                    }
                }
            }

            WriteObject(list);
        }

        protected override void EndProcessing()
        {
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
