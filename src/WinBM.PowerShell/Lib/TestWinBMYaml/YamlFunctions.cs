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
        private readonly static Regex _comment_hash = new Regex(@"(?<=(('[^']*){2})*)\s*#.*$");

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

        public static List<YamlNodeCollection> GetNodeCollections(AdvancedStringReader asr, LineType type)
        {
            var list = new List<YamlNodeCollection>();

            string readLine = "";
            int? indent = null;

            YamlNodeCollection collection = null;
            while ((readLine = asr.ReadLine()) != null)
            {
                if (readLine.Trim() == "") { continue; }

                if (collection == null || readLine.Trim().StartsWith("- "))
                {
                    readLine = _param_start.Replace(readLine, "  ");
                    collection = new YamlNodeCollection();
                    list.Add(collection);
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
                        collection.Add(
                            asr.Line,
                            type,
                            readLine.Substring(0, readLine.IndexOf(":")).Trim(),
                            readLine.Substring(readLine.IndexOf(":") + 1).Trim());
                    }
                    else
                    {
                        if (collection.Count == 0)
                        {
                            break;
                        }
                        collection.AppendValue(Environment.NewLine + readLine.Trim());
                    }
                }
                else if (nowIndent > indent)
                {
                    if (collection.Count == 0)
                    {
                        break;
                    }
                    collection.AppendValue(Environment.NewLine + readLine.Trim());
                }
            }

            return list;
        }

        /// <summary>
        /// コメント行/コメント部分を削除
        /// </summary>
        /// <param name="readLine"></param>
        /// <returns></returns>
        public static string RemoveComment(string readLine)
        {
            return readLine = _comment_hash.Replace(readLine, "");
        }
    }
}
