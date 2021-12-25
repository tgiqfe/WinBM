using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using System.Text.RegularExpressions;
using WinBM.PowerShell.Lib.TestWinBMYaml;

namespace WinBM.PowerShell.Cmdlet
{
    [Cmdlet(VerbsDiagnostic.Test, "WinBM")]
    public class TestWinBM : PSCmdlet
    {
        [Parameter(Position = 0), Alias("File")]
        public string RecipeFile { get; set; }

        [Parameter]
        public SwitchParameter Quiet { get; set; }

        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            List<string> fileList = new List<string>();
            if (File.Exists(RecipeFile))
            {
                fileList.Add(RecipeFile);
            }
            else if (Directory.Exists(RecipeFile))
            {
                foreach (string filePath in Directory.GetFiles(RecipeFile))
                {
                    fileList.Add(filePath);
                }
            }

            //  全Yamlのデシリアライズチェック
            List<WinBM.Recipe.Page> pageList = new List<WinBM.Recipe.Page>();
            bool isSuccess = false;
            try
            {
                fileList.ForEach(x =>
                {
                    pageList.AddRange(WinBM.Recipe.Page.Deserialize(x));
                });
                isSuccess = true;
            }
            catch { }

            if (Quiet)
            {
                WriteObject(isSuccess);
                return;
            }
            else if (isSuccess)
            {
                //  Envのみ読み込み
                var manager = new SessionManager()
                {
                    Cmdlet = this,
                    Setting = GlobalSetting.Load(),
                };
                var rancher = new Rancher(manager);
                rancher.EnvProcess(pageList.
                    Where(x => x.Kind == WinBM.Recipe.Page.EnumKind.Env).ToList());

                WriteObject(pageList);
                return;
            }

            //  WinBMYamlインスタンスを取得
            WinBMYamlCollection collection = new WinBMYamlCollection(fileList);

            string viewFilePath = "";
            foreach (var winBMYaml in collection)
            {
                if (viewFilePath != winBMYaml.FilePath)
                {
                    viewFilePath = winBMYaml.FilePath;
                    Console.WriteLine(viewFilePath);
                }
                bool ret = winBMYaml.TestDeserialize();

                if (ret)
                {
                    Console.Write($"  Page {winBMYaml.PageIndex} : [");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("Success");
                    Console.ResetColor();
                    Console.WriteLine("]");
                }
                else
                {
                    Console.Write($"  Page {winBMYaml.PageIndex} : [");
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("Failed");
                    Console.ResetColor();
                    Console.WriteLine("]");

                    winBMYaml.TestParameter();
                }
            }
        }

        protected override void EndProcessing()
        {
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
