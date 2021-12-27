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
                var manager = new SessionManager()
                {
                    //Setting = GlobalSetting.Load(),
                    Stepable = true,
                    Interactive = true,
                };

                var rancher = new Rancher(manager);

                List<Page> list = null;
                foreach (string path in param.RecipeFile.Paths)
                {
                    list.AddRange(Page.Deserialize(path));
                }

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
