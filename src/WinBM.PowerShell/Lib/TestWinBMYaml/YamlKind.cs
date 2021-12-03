using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WinBM.PowerShell.Lib.TestWinBMYaml
{
    internal class YamlKind
    {
        public string Kind { get; set; }

        /// <summary>
        /// インスタンス作成用メソッド
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static YamlKind Create(string content)
        {
            var spec = new YamlKind();

            using (var sr = new StringReader(content))
            {
                string readLine = "";
                while ((readLine = sr.ReadLine()) != null)
                {
                    if (readLine.StartsWith("kind:"))
                    {
                        spec.Kind = readLine.Substring(readLine.IndexOf(":") + 1).Trim();
                        break;
                    }
                }
            }

            return spec;
        }

        public string SearchIllegal()
        {
            switch (this.Kind.ToLower())
            {
                case "config":
                case "output":
                case "job":
                    return null;
                default:
                    return $"[Illegal] {this.Kind}";
            }
        }
    }
}
