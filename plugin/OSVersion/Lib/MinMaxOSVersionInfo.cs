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
    internal class MinMaxOSVersionInfo
    {
        public OSVersionInfo Min { get; set; }
        public OSVersionInfo Max { get; set; }
        public bool Enabled { get; set; }

        private const char DELIMITER = '~';

        private OSFamily _family = OSFamily.Windows;

        public MinMaxOSVersionInfo(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (OSVersionInfo.TryParse(text, _family, out OSVersionInfo info))
                {
                    this.Min = info;
                    this.Max = info;
                    this.Enabled = true;
                }
                else if (text.Contains(DELIMITER))
                {
                    string minStr = text.Substring(0, text.IndexOf(DELIMITER));
                    string maxStr = text.Substring(text.IndexOf(DELIMITER) + 1);
                    OSVersionInfo tempMin = minStr == "" ?
                        OSVersionInfo.GetMinVersion() :
                        OSVersionInfo.TryParse(minStr, _family, out OSVersionInfo outTempMin) ?
                            outTempMin :
                            null;
                    OSVersionInfo tempMax = maxStr == "" ?
                        OSVersionInfo.GetMaxVersion() :
                        OSVersionInfo.TryParse(maxStr, _family, out OSVersionInfo outTempMax) ?
                            outTempMax :
                            null;

                    if (tempMin is not null && tempMax is not null)
                    {
                        this.Enabled = true;
                        this.Min = tempMin;
                        this.Max = tempMax;
                    }
                }
            }
        }

        public bool Within(OSVersionInfo info)
        {
            string name = info?.Name;
            if (name == this.Min?.Name && name == this.Max?.Name)
            {
                return Enabled && Min <= info && Max >= info;
            }
            return false;
        }

        public override string ToString()
        {
            if (Enabled)
            {
                return string.Format("{0}{1}{2}",
                    Min == OSVersionInfo.GetMinVersion() ? "" : Min.ToString(),
                    DELIMITER,
                    Max == OSVersionInfo.GetMaxVersion() ? "" : Max.ToString());
            }
            return null;
        }
    }
}
