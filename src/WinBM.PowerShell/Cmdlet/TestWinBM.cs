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
            TestParPage(fileList);
        }

        protected override void EndProcessing()
        {
            Environment.CurrentDirectory = _currentDirectory;
        }

        #region Test Yaml file

        /// <summary>
        /// Recipe全体を一括デシリアライズ可否チェック
        /// </summary>
        /// <param name="fileList"></param>
        /// <returns></returns>
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

        private void TestParPage(List<string> fileList)
        {
            Regex ymlDelimiter = new Regex(@"---\r?\n");
            Func<string, string[]> splitPage = (string filePath) =>
            {
                using (var sr = new StreamReader(filePath, Encoding.UTF8))
                {
                    return ymlDelimiter.Split(sr.ReadToEnd());
                }
            };

            foreach (string filePath in fileList)
            {
                Console.WriteLine(filePath);
                int index = 0;
                foreach (string pageText in splitPage(filePath))
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

                        TestPageDetail(pageText);
                    }
                }
            }
        }

        private enum Stage
        {
            top,
            metadata,
            config,
            config_spec,
            output,
            output_spec,
            job,
            job_require,
            job_require_task,
            job_work,
            job_work_task,
        }

        private void TestPageDetail(string pageText)
        {
            Stage stage = Stage.top;

            using (var sr = new StringReader(pageText))
            {
                string readLine = "";
                while ((readLine = sr.ReadLine()) != null)
                {
                    switch (stage)
                    {
                        case Stage.top:
                            stageTop(readLine);
                            break;
                        case Stage.metadata:
                            stageMetadata(readLine);
                            break;
                        case Stage.config:
                            break;

                        default:
                            return;
                    }
                }
            }

            void stageTop(string readLine)
            {
                switch (readLine)
                {
                    case string s when s.StartsWith("kind:"):
                        Console.WriteLine("    kind: {0}", s.Substring(s.IndexOf(":") + 1).Trim());
                        break;
                    case "metadata:":
                        stage = Stage.metadata;
                        break;
                    case "config:":
                        stage = Stage.config;
                        break;
                    case "output:":
                        stage = Stage.output;
                        break;
                    case "job:":
                        stage = Stage.job;
                        break;
                    default:
                        Console.WriteLine("    Failed line => {0}", readLine);
                        break;
                }
            }

            void stageMetadata(string readLine)
            {
                switch (readLine)
                {
                    case string s when s.StartsWith("  name:"):
                        Console.WriteLine("    name: {0}", s.Substring(s.IndexOf(":") + 1).Trim());
                        break;
                    case string s when s.StartsWith("  description:"):
                        Console.WriteLine("    description: {0}", s.Substring(s.IndexOf(":") + 1).Trim());
                        break;
                    case string s when s.StartsWith("  skip:"):
                        Console.WriteLine("    skip: {0}", s.Substring(s.IndexOf(":") + 1).Trim());
                        break;
                    case string s when s.StartsWith("  step:"):
                        Console.WriteLine("    step: {0}", s.Substring(s.IndexOf(":") + 1).Trim());
                        break;
                    case string s when s.StartsWith("  priority:"):
                        Console.WriteLine("    priority: {0}", s.Substring(s.IndexOf(":") + 1).Trim());
                        break;
                    default:
                        Console.Write("    failed metadata => ");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(readLine.Trim());
                        Console.ResetColor();
                        break;
                }
            }

            

        }

        #endregion
    }
}
