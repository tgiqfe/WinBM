using System;
using WinBM;
using WinBM.Task;
using System.Collections.Generic;
using WinBM.Recipe;
using System.IO;
using System.Text;
using System.Linq;
using WinBM.Cmd.Arguments;

namespace WinBM.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            var param = new ArgumentParameter(args);
            if (param.Enabled)
            {
                //string manifestFile = param.RecipeFile.Path;

                var manager = new SessionManager()
                {
                    Setting = GlobalSetting.Load(),
                    Stepable = true,
                    Interactive = true,
                };

                var rancher = new Rancher(manager);

                List<Page> list = null;
                foreach (string path in param.RecipeFile.Paths)
                {
                    using (var sr = new StreamReader(path, Encoding.UTF8))
                    {
                        list.AddRange(Page.Deserialize(sr));
                    }
                }

                /*
                if (File.Exists(manifestFile))
                {
                    using (var sr = new StreamReader(manifestFile, Encoding.UTF8))
                    {
                        list = Page.Deserialize(sr);
                    }
                }
                else if (Directory.Exists(manifestFile))
                {
                    foreach (string file in Directory.GetFiles(manifestFile))
                    {
                        string extension = Path.GetExtension(file).ToLower();
                        if (extension == ".yml" || extension == ".yaml")
                        {
                            using (var sr = new StreamReader(file, Encoding.UTF8))
                            {
                                list.AddRange(Page.Deserialize(sr));
                            }
                        }
                    }
                }
                */

                rancher.ConfigProcess(list.
                    Where(x => x.Kind == WinBM.Recipe.Page.EnumKind.Config).ToList());
                rancher.OutputProcess(list.
                    Where(x => x.Kind == WinBM.Recipe.Page.EnumKind.Output).ToList());
                rancher.JobProcess(list.
                    Where(x => x.Kind == WinBM.Recipe.Page.EnumKind.Job).ToList());
                rancher.PostPageProcess();
            }

            Console.ReadLine();
        }
    }
}
