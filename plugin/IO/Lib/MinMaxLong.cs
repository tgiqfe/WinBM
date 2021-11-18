using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Lib
{
    /// <summary>
    /// long型の範囲比較用
    /// </summary>
    internal class MinMaxLong
    {
        public long Min { get; set; }
        public long Max { get; set; }
        public bool Enabled { get; set; }

        private const char DELIMITER = '~';

        public MinMaxLong(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (long.TryParse(text, out long lng))
                {
                    this.Min = lng;
                    this.Max = lng;
                    this.Enabled = true;
                }
                else if (text.Contains(DELIMITER))
                {
                    string minStr = text.Substring(0, text.IndexOf(DELIMITER));
                    string maxStr = text.Substring(text.IndexOf(DELIMITER) + 1);
                    long? tempMin = minStr == "" ?
                        long.MinValue :
                        long.TryParse(minStr, out long outTempMin) ?
                            outTempMin :
                            null;
                    long? tempMax = maxStr == "" ?
                        long.MaxValue :
                        long.TryParse(maxStr, out long outTempMax) ?
                            outTempMax :
                            null;

                    if (tempMin != null && tempMax != null)
                    {
                        this.Enabled = true;
                        this.Min = (long)tempMin;
                        this.Max = (long)tempMax;
                    }
                }
            }
        }

        public bool Within(long dt)
        {
            return Enabled && Min <= dt && Max >= dt;
        }

        public override string ToString()
        {
            if (Enabled)
            {
                return string.Format("{0}{1}{2}",
                    Min == long.MinValue ? "" : Min.ToString(),
                    DELIMITER,
                    Max == long.MaxValue ? "" : Max.ToString());
            }
            return null;
        }
    }
}
