using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace IO.Lib
{
    class Wildcard
    {
        /// <summary>
        /// ワイルドカードマッチング用Regexを取得
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Regex GetPattern(string text)
        {
            string patternString = Regex.Replace(text, ".",
                x =>
                {
                    string y = x.Value;
                    if (y.Equals("?")) { return "."; }
                    else if (y.Equals("*")) { return ".*"; }
                    else { return Regex.Escape(y); }
                });
            if (!patternString.StartsWith("*")) { patternString = "^" + patternString; }
            if (!patternString.EndsWith("*")) { patternString = patternString + "$"; }
            return new Regex(patternString, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// ワイルドカード指定したファイル/フォルダーを取得
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="isDirectory"></param>
        /// <returns></returns>
        public static string[] GetPaths(string[] paths, bool isDirectory)
        {
            var list = new List<string>();
            if (paths?.Length > 0)
            {
                foreach (string path in paths)
                {
                    if (Path.GetFileName(path).Contains("*"))
                    {
                        string parent = Path.GetDirectoryName(path);
                        if (Directory.Exists(parent))
                        {
                            Regex wildcard = Wildcard.GetPattern(path);
                            list.AddRange(isDirectory ?
                                Directory.GetDirectories(parent).Where(x => wildcard.IsMatch(x)) :
                                Directory.GetFiles(parent).Where(x => wildcard.IsMatch(x)));
                        }
                    }
                    else
                    {
                        list.Add(path);
                    }
                }
            }
            return list.ToArray();
        }
    }
}
