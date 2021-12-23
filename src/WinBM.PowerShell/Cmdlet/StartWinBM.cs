using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using WinBM.Task;
using WinBM.Recipe;
using WinBM;

namespace WinBM.PowerShell.Cmdlet
{
    [Cmdlet(VerbsLifecycle.Start, "WinBM")]
    public class StartWinBM : PSCmdlet
    {
        [Parameter(Position = 0), Alias("File")]
        public string RecipeFile { get; set; }

        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            if (!string.IsNullOrEmpty(RecipeFile))
            {
                //string recipeFile = RecipeFile;

                var manager = new SessionManager()
                {
                    Cmdlet = this,
                    Setting = GlobalSetting.Load(),
                    Stepable = true,
                    Interactive = true,
                };
                var rancher = new Rancher(manager);

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
                            /*
                            using (var sr = new StreamReader(filePath, Encoding.UTF8))
                            {
                                list ??= new List<Page>();
                                list.AddRange(WinBM.Recipe.Page.Deserialize(sr));
                            }
                            */
                            list ??= new List<Page>();
                            list.AddRange(WinBM.Recipe.Page.Deserialize(filePath));
                        }
                    }
                }

                rancher.ConfigProcess(list.
                    Where(x => x.Kind == WinBM.Recipe.Page.EnumKind.Config).ToList());
                rancher.OutputProcess(list.
                    Where(x => x.Kind == WinBM.Recipe.Page.EnumKind.Output).ToList());
                rancher.JobProcess(list.
                    Where(x => x.Kind == WinBM.Recipe.Page.EnumKind.Job).ToList());
                rancher.PostPageProcess();
            }
        }

        protected override void EndProcessing()
        {
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
