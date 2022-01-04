using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Standard.Lib
{
    internal class CheckHostName
    {
        private string _currentHostName = null;

        public CheckHostName()
        {
            this._currentHostName = Environment.MachineName;
        }

        /// <summary>
        /// ホスト名と一致チェック
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public bool IsMatch(string hostName)
        {
            return IsMatch(new string[1] { hostName });
        }

        /// <summary>
        /// ホスト名と一致チェック
        /// </summary>
        /// <param name="hostNames"></param>
        /// <returns></returns>
        public bool IsMatch(string[] hostNames)
        {
            bool ret = false;
            foreach (string hostName in hostNames)
            {
                if (string.IsNullOrEmpty(hostName)) { continue; }
                foreach (string name in hostName.Split(',').Select(x => x.Trim()).ToArray())
                {
                    if (name.Contains("*"))
                    {
                        ret |= WildcardMatch(name);
                    }
                    else if (name.Contains("~"))
                    {
                        ret |= RangeMatch(name);
                    }
                    else
                    {
                        ret |= FullMatch(name);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// ワイルドカードを含む名前でチェック
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool WildcardMatch(string name)
        {
            string patternString = Regex.Replace(name, ".",
                x =>
                {
                    string y = x.Value;
                    if (y.Equals("?")) { return "."; }
                    else if (y.Equals("*")) { return ".*"; }
                    else { return Regex.Escape(y); }
                });
            if (!patternString.StartsWith("*")) { patternString = "^" + patternString; }
            if (!patternString.EndsWith("*")) { patternString = patternString + "$"; }
            Regex tempReg = new Regex(patternString, RegexOptions.IgnoreCase);

            return tempReg.IsMatch(_currentHostName);
        }

        /// <summary>
        /// 範囲指定チェック
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool RangeMatch(string name)
        {
            string endText = name.Substring(name.IndexOf("~") + 1);
            string startText = name.Substring(name.IndexOf("~") - endText.Length, endText.Length);
            string baseText = name.Substring(0, name.Length - endText.Length - startText.Length - 1);
            if (int.TryParse(startText, out int startNum) && int.TryParse(endText, out int endNum))
            {
                string currentNumText = _currentHostName.Substring(_currentHostName.Length - endText.Length);
                string currentBaseText = _currentHostName.Substring(0, _currentHostName.Length - endText.Length);
                if (int.TryParse(currentNumText, out int currentNum) &&
                    currentBaseText.Equals(baseText, StringComparison.OrdinalIgnoreCase))
                {
                    return currentNum >= startNum && currentNum <= endNum;
                }
            }

            return false;
        }

        /// <summary>
        /// 完全一致チェック
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool FullMatch(string name)
        {
            return name.Equals(_currentHostName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
