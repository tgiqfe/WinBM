using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Lib
{
    /// <summary>
    /// コンソールウィンドウ上に文字のみでプログレスバーを表現する為のクラス
    /// </summary>
    class ConsoleProgress
    {
        #region ClassParameter

        public string StartText = "[";
        public string EndText = "]";
        public char ProgressChar = '#';
        public int MaxLength = 80;

        #endregion

        private int _currentPoint = 0;

        public ConsoleProgress() { }

        /// <summary>
        /// 進捗表示開始
        /// </summary>
        public void ProgressStart()
        {
            Console.Write(StartText);
        }

        /// <summary>
        /// 進捗表示の更新
        /// </summary>
        /// <param name="percentage"></param>
        public void Progress(double percentage)
        {
            double newPercent = percentage switch
            {
                double d when (d <= 1) => MaxLength * percentage,
                double d when (d > 1 && d <= 100) => MaxLength * percentage / 100,
                _ => 100.0
            };

            int newPoint = (int)Math.Round(newPercent, MidpointRounding.AwayFromZero);
            int diffChar = newPoint - _currentPoint;
            Console.Write(new string(ProgressChar, diffChar));
            _currentPoint = newPoint;
        }

        /// <summary>
        /// 進捗表示の終了
        /// </summary>
        public void ProgressEnd()
        {
            this.Progress(1.0);
            Console.WriteLine(EndText);
        }
    }
}
