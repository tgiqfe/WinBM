using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace WinBM.PowerShell.Lib.TestWinBMYaml
{
    internal class IllegalParameter
    {
        public IllegalType IllegalType { get; set; }
        public WinBMType Type { get; set; }
        public int Lines { get; set; }
        public string Text { get; set; }

        private static readonly Regex _newline_char = new Regex(@"\r?\n");

        /// <summary>
        /// スペース×6, [IllegalType] line:行数 content:Illegal記述内容
        ///       [Value] line:50 content:aaaaaaaaaaaaaaaaaaaaaaa...
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            string text = _newline_char.IsMatch(Text) ?
                _newline_char.Replace(Text, "\\n") :
                Text;
            int contentLeft = 6 + 1 + (this.IllegalType.ToString().Length) + 1 + 4 + (this.Lines.ToString().Length) + 1 + 8;
            if (contentLeft + text.Length > Console.WindowWidth)
            {
                text = text.Substring(0, Console.WindowWidth - 4) + "...";
            }
            return text;
        }
    }
}
