using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Audit.Lib
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class MonitorSize
    {
        #region Watch method

        public static bool WatchFile(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, bool? isMonitor, FileInfo info)
        {
            if ((!isMonitor ?? true) && watch.Size == null) { return false; }

            bool ret = false;
            if (watch.Size == null)
            {
                ret = true;
                watch.Size = info.Length;
            }
            else
            {
                string pathType = "file";
                string checkTarget = "Size";

                long ret_long = info.Length;
                ret = ret_long != watch.Size;
                dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                    $"{watch.Size} -> {ret_long}" :
                    ret_long.ToString();

                watch.Size = ret_long;
            }
            return ret;
        }

        public static bool WatchFile(
            WatchPath watch, Dictionary<string, string> dictionary, int serial, FileInfo info)
        {
            bool ret = false;
            if (watch.IsSize ?? false)
            {
                if (info.Exists)
                {
                    long ret_long = info.Length;
                    ret = ret_long != watch.Size;
                    if(watch.Size != null)
                    {
                        string pathType = "file";
                        string checkTarget = "Size";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            $"{watch.Size} -> {ret_long}" :
                            ret_long.ToString();
                    }
                    watch.Size = ret_long;
                }
                else
                {
                    watch.Size = null;
                }
            }
            return ret;
        }

        #endregion
        #region Get method

        public static string ToReadable(long size)
        {
            if (size < 1024 * 0.9)
            {
                //  byte
                return $"{size} byte";
            }
            else if (size < 1024 * 1024 * 0.9)
            {
                //  KB
                return $"{Math.Round(size / 1024.0, 2, MidpointRounding.AwayFromZero)} KB";
            }
            else if (size < 1024 * 1024 * 1024 * 0.9)
            {
                //  MB
                return $"{Math.Round(size / 1024.0 / 1024.0, 2, MidpointRounding.AwayFromZero)} MB";
            }
            else if (size < (1024L * 1024L * 1024L * 1024L * 0.9))
            {
                //  GB
                return $"{Math.Round(size / 1024.0 / 1024.0 / 1024.0, 2, MidpointRounding.AwayFromZero)} GB";
            }
            else
            {
                //  TB
                return $"{Math.Round(size / 1024.0 / 1024.0 / 1024.0 / 1024.0, 2, MidpointRounding.AwayFromZero)} TB";
            }
        }

        #endregion
    }
}
