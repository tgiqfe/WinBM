using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Lib
{
    /// <summary>
    /// 日付範囲比較用
    /// </summary>
    internal class MinMaxDate
    {
        public DateTime Min { get; set; }
        public DateTime Max { get; set; }
        public bool Enabled { get; set; }

        private const char DELIMITER = '~';

        public MinMaxDate(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (DateTime.TryParse(text, out DateTime dd))
                {
                    this.Min = dd;
                    this.Max = dd;
                    this.Enabled = true;
                }
                else if (text.Contains(DELIMITER))
                {
                    string minStr = text.Substring(0, text.IndexOf(DELIMITER));
                    string maxStr = text.Substring(text.IndexOf(DELIMITER) + 1);
                    DateTime? tempMin = minStr == "" ?
                        DateTime.MinValue :
                        DateTime.TryParse(minStr, out DateTime outTempMin) ?
                            outTempMin :
                            null;
                    DateTime? tempMax = maxStr == "" ?
                        DateTime.MaxValue :
                        DateTime.TryParse(maxStr, out DateTime outTempMax) ?
                            outTempMax :
                            null;

                    if (tempMin != null && tempMax != null)
                    {
                        this.Enabled = true;
                        this.Min = (DateTime)tempMin;
                        this.Max = (DateTime)tempMax;
                    }
                }
            }
        }

        public bool Within(DateTime dt)
        {
            return Enabled && Min <= dt && Max >= dt;
        }

        public override string ToString()
        {
            if (Enabled)
            {
                return string.Format("{0}{1}{2}",
                    Min == DateTime.MinValue ? "" : Min.ToString("yyyy/MM/dd HH:mm:ss"),
                    DELIMITER,
                    Max == DateTime.MaxValue ? "" : Max.ToString("yyyy/MM/dd HH:mm:ss"));
            }
            return null;
        }
    }
}
