using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace WinBM.PowerShell.Lib.MDtoRecipe
{
    internal class PageCollection : Dictionary<string, List<PagePart>>
    {
        private int _Index { get; set; }

        private Regex pattern_ymlStart = new Regex(@"```ya?ml[\s:].+\.ya?ml$");
        private string pattern_ymlEnd = "```";
        private Regex pattern_yamlFileName = new Regex(@"(?<=```ya?ml[\s:]).+");

        public PageCollection() { }

        public void Add(string filePath)
        {
            using (var sr = new StreamReader(filePath, Encoding.UTF8))
            {
                string readLine = "";
                string tempFileName = "";
                PagePart part = null;
                while ((readLine = sr.ReadLine()) != null)
                {
                    if (pattern_ymlStart.IsMatch(readLine))
                    {
                        tempFileName = pattern_yamlFileName.Match(readLine).Value;
                        part = new PagePart()
                        {
                            Index = ++_Index,
                            SourceFile = filePath,
                        };

                        StringBuilder sb = new StringBuilder();
                        string codeBlockLine = "";
                        while ((codeBlockLine = sr.ReadLine()) != null)
                        {
                            if (codeBlockLine == pattern_ymlEnd)
                            {
                                break;
                            }
                            sb.AppendLine(codeBlockLine);
                        }
                        part.Content = sb.ToString();

                        if (!this.ContainsKey(tempFileName))
                        {
                            this[tempFileName] = new List<PagePart>();
                        }
                        this[tempFileName].Add(part);
                    }
                }
            }
        }

        public List<RecipeFile> ToRecipeFileList()
        {
            List<RecipeFile> list = new List<RecipeFile>();

            foreach (var pair in this)
            {
                StringBuilder sb = new StringBuilder();
                foreach (PagePart part in pair.Value.OrderBy(x => x.Index))
                {
                    sb.AppendLine(part.Content);
                }
                list.Add(new RecipeFile()
                {
                    FileName = pair.Key,
                    Content = sb.ToString(),
                    RecipeFiles = pair.Value.Select(x => x.SourceFile).Distinct().ToList(),
                });
            }

            return list;
        }
    }
}
