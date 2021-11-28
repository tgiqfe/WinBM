using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSVersion.Lib
{
    /// <summary>
    /// OSバージョン比較用
    /// </summary>
    internal class MinMaxOSVersion
    {
        public OSInfo Min { get; set; }
        public OSInfo Max { get; set; }
        public bool Enabled { get; set; }

        private const char DELIMITER = '~';

        private OSFamily _family = OSFamily.Windows;

        public MinMaxOSVersion(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (OSInfo.TryParse(text, _family, out OSInfo info))
                {
                    this.Min = info;
                    this.Max = info;
                    this.Enabled = true;
                }
                else if (text.Contains(DELIMITER))
                {
                    string minStr = text.Substring(0, text.IndexOf(DELIMITER));
                    string maxStr = text.Substring(text.IndexOf(DELIMITER) + 1);
                    OSInfo tempMin = minStr == "" ?
                        OSInfo.GetMinVersion(_family) :
                        OSInfo.TryParse(minStr, _family, out OSInfo outTempMin) ?
                            outTempMin :
                            null;
                    OSInfo tempMax = maxStr == "" ?
                        OSInfo.GetMaxVersion(_family) :
                        OSInfo.TryParse(maxStr, _family, out OSInfo outTempMax) ?
                            outTempMax :
                            null;

                    if (tempMin is not null && tempMax is not null)
                    {
                        this.Enabled = true;
                        this.Min = (OSInfo)tempMin;
                        this.Max = (OSInfo)tempMax;
                    }
                }
            }
        }

        public bool Within(OSInfo info)
        {
            return Enabled && Min <= info && Max >= info;
        }

        public override string ToString()
        {
            if (Enabled)
            {
                return string.Format("{0}{1}{2}",
                    Min == OSInfo.GetMinVersion(_family) ? "" : Min.ToString(),
                    DELIMITER,
                    Max == OSInfo.GetMaxVersion(_family) ? "" : Max.ToString());
            }
            return null;
        }
    }
}
