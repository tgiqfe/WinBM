using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace WinBM.PowerShell.Lib.TestWinBMYaml
{
    internal class YamlFunctions
    {
        private readonly static Regex _indent_space = new Regex(@"^\s*");
        private readonly static Regex _param_start = new Regex(@"(?<=\s*)- ");

        /// <summary>
        /// 行のインデントの深さを取得
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int GetIndentDepth(string text)
        {
            Match match = _indent_space.Match(text);
            return match.Value.Length;
        }

        /// <summary>
        /// 子要素パラメータをリスト化して返す
        /// </summary>
        /// <param name="sr"></param>
        /// <returns></returns>
        public static List<Dictionary<string, string>> GetParameters(StringReader sr)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();

            string key = "";
            string readLine = "";
            int? indent = null;

            Dictionary<string, string> parameter = null;

            while ((readLine = sr.ReadLine()) != null)
            {
                if (readLine.Trim() == "")
                {
                    continue;
                }

                if (parameter == null || readLine.StartsWith("- "))
                {
                    readLine = _param_start.Replace(readLine, "  ");
                    parameter = new Dictionary<string, string>();
                    list.Add(parameter);
                }

                indent ??= GetIndentDepth(readLine);
                int nowIndent = GetIndentDepth(readLine);
                if (nowIndent < indent)
                {
                    break;
                }
                else if (nowIndent == indent)
                {
                    if (readLine.Contains(":"))
                    {
                        key = readLine.Substring(0, readLine.IndexOf(":")).Trim();
                        parameter[key] = readLine.Substring(readLine.IndexOf(":") + 1).Trim();
                    }
                    else
                    {
                        parameter[key] += (Environment.NewLine + readLine.Trim());
                    }
                }
                else if (nowIndent > indent)
                {
                    parameter[key] += (Environment.NewLine + readLine.Trim());
                }
            }

            return list;
        }


    }
}
