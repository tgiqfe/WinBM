﻿using System;
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
        #region Compare method

        public static bool CompareFile(ComparePath compare, Dictionary<string, string> dictionary, int serial, string fileA, string fileB)
        {
            if (compare.IsSize ?? false)
            {
                string pathType = "file";
                string checkTarget = "Size";

                long ret_fileA = new System.IO.FileInfo(fileA).Length;
                long ret_fileB = new System.IO.FileInfo(fileB).Length;

                dictionary[$"{pathType}A_{checkTarget}_{serial}"] = string.Format("{0} ({1})", ret_fileA, ToReadable(ret_fileA));
                dictionary[$"{pathType}B_{checkTarget}_{serial}"] = string.Format("{0} ({1})", ret_fileB, ToReadable(ret_fileB));
                return ret_fileA == ret_fileB;
            }
            return true;
        }

        #endregion
        #region Watch method

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
                    if (watch.Size != null)
                    {
                        string pathType = "file";
                        string checkTarget = "Size";
                        dictionary[$"{pathType}_{checkTarget}_{serial}"] = ret ?
                            string.Format("{0} -> {1} ({2})", watch.Size, ret_long, ToReadable(ret_long)) :
                            string.Format("{0} ({1})", ret_long, ToReadable(ret_long));
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
