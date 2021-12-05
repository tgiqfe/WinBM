using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace WinBM.PowerShell.Lib.TestWinBMYaml
{
    internal class IllegalParam
    {
        public IllegalType IllegalType { get; set; }
        public int Line { get; set; }
        public string Message { get; set; }

        private static readonly Regex _newline_char = new Regex(@"\r?\n");

        /// <summary>
        /// スペース×6, [IllegalType] line:行数 content:Illegal記述内容
        ///       [Value] line:50 aaaaaaaaaaaaaaaaaaaaaaa...
        /// </summary>
        /// <returns></returns>
        private string GetText()
        {
            string text = _newline_char.IsMatch(Message) ?
                _newline_char.Replace(Message, "\\n") :
                Message;
            int contentLeft = 6 + 1 + (this.IllegalType.ToString().Length) + 1 + 4 + (this.Line.ToString().Length) + 1;
            if (contentLeft + text.Length > Console.WindowWidth)
            {
                text = text.Substring(0, Console.WindowWidth - contentLeft - 5) + "...";
            }
            return text;
        }

        public void View()
        {
            Console.Write("      [");
            switch (IllegalType)
            {
                case IllegalType.Key:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(IllegalType.Key);
                    break;
                case IllegalType.Value:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write(IllegalType.Value);
                    break;
                case IllegalType.Dll:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.Write(IllegalType.Dll);
                    break;
                default:
                    Console.Write(IllegalType.None);
                    break;
            }
            Console.ResetColor();
            Console.WriteLine("] line:{0} {1}", this.Line, GetText());
        }
    }
}
