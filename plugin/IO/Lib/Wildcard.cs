using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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
    }
}
