using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using System.Text.RegularExpressions;

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
            var fileList = new List<string>();
            if (File.Exists(RecipeFile))
            {
                fileList.Add(RecipeFile);
            }
            else if (Directory.Exists(RecipeFile))
            {
                foreach (string filePath in Directory.GetFiles(RecipeFile))
                {
                    string extension = Path.GetExtension(filePath).ToLower();
                    if (extension == ".yml" || extension == ".yaml")
                    {
                        fileList.Add(filePath);
                    }
                }
            }

            //  Recipe全体を一括デシリアライズ可否チェック
            /*
            List<WinBM.Recipe.Page> list = new List<WinBM.Recipe.Page>();
            try
            {
                fileList.ForEach(x =>
                {
                    using (var sr = new StreamReader(x, Encoding.UTF8))
                    {
                        list.AddRange(WinBM.Recipe.Page.Deserialize(sr));
                    }
                });

                //  何事も問題なくRecipeが取得できたら終了
                if (Quiet)
                {
                    WriteObject(true);
                    return;
                }
                WriteObject(list);
                return;
            }
            catch
            {
                if (Quiet)
                {
                    WriteObject(false);
                    return;
                }
            }
            */

            List<WinBM.Recipe.Page> list = TestAllYaml(fileList);
            if (Quiet)
            {
                WriteObject(list != null);
                return;
            }
            else if (list != null)
            {
                WriteObject(list);
                return;
            }

 

            //  ファイル/Page単位でチェック
            Regex ymlDelimiter = new Regex(@"---\r?\n");

            string[] read(string filePath)
            {
                using (var sr = new StreamReader(filePath, Encoding.UTF8))
                {
                    return ymlDelimiter.Split(sr.ReadToEnd());
                }
            }
            foreach (string filePath in fileList)
            {
                Console.WriteLine(filePath);
                int index = 0;
                foreach (string pageText in read(filePath))
                {
                    if (pageText.Trim() == "")
                    {
                        continue;
                    }

                    Console.Write($"  Page {++index} : ");
                    try
                    {
                        using (var sr = new StringReader(pageText))
                        {
                            var page = WinBM.Recipe.Page.Deserialize(sr);
                        }
                        Console.Write("[");
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write("Success");
                        Console.ResetColor();
                        Console.WriteLine("]");
                    }
                    catch
                    {
                        //  シリアライズ失敗時の処理
                        Console.Write("[");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write("Failed");
                        Console.ResetColor();
                        Console.WriteLine("]");
                    }
                }
            }
        }

        protected override void EndProcessing()
        {
            Environment.CurrentDirectory = _currentDirectory;
        }

        #region Test Yaml file

        private List<WinBM.Recipe.Page> TestAllYaml(List<string> fileList)
        {
            List<WinBM.Recipe.Page> list = new List<WinBM.Recipe.Page>();
            try
            {
                fileList.ForEach(x =>
                {
                    using (var sr = new StreamReader(x, Encoding.UTF8))
                    {
                        list.AddRange(WinBM.Recipe.Page.Deserialize(sr));
                    }
                });
                return list;
            }
            catch { }

            return null;
        }


        #endregion
    }
}
