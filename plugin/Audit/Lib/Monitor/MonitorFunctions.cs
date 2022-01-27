using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Security.Cryptography;
using IO.Lib;

namespace Audit.Lib.Monitor
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class MonitorFunctions
    {
        #region Attributes

        public static bool[] GetAttributes(string path)
        {
            FileAttributes attr = File.GetAttributes(path);
            return new bool[]
            {
                (attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly,
                (attr & FileAttributes.Hidden) == FileAttributes.Hidden,
                (attr & FileAttributes.System) == FileAttributes.System
            };
        }

        public static string ToReadableAttributes(bool[] ret_bools)
        {
            return string.Format(
                "[{0}]Readonly [{1}]Hidden [{2}]System",
                ret_bools[0] ? "x" : " ",
                ret_bools[1] ? "x" : " ",
                ret_bools[2] ? "x" : " ");
        }

        #endregion
        #region ChildCount

        public static int[] GetDirectoryChildCount(string path)
        {
            var children = DirectoryControl.GetAllChildren(path);
            int directoryCount = children.Directories.Count;
            int fileCount = children.Files.Count;

            return new int[2]
            {
                children.Directories.Count,
                children.Files.Count,
                //Directory.GetDirectories(path, "*", SearchOption.AllDirectories).Length,
                //Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length
        };
        }

        public static int[] GetRegistryKeyChildCount(RegistryKey regKey)
        {
            string[] childKeys = regKey.GetSubKeyNames();
            int[] ret_childCounts = new int[]
            {
                childKeys.Length,
                regKey.GetValueNames().Length
            };

            foreach (string childKey in childKeys)
            {
                using (RegistryKey regChildKey = regKey.OpenSubKey(childKey, false))
                {
                    int[] childCounts = GetRegistryKeyChildCount(regChildKey);
                    ret_childCounts[0] += childCounts[0];
                    ret_childCounts[1] += childCounts[1];
                }
            }

            return ret_childCounts;
        }

        public static string ToReadableChildCount(int[] ret_bools, bool isDirectory)
        {
            return isDirectory ?
                string.Format("Directory:{0} / File:{1}", ret_bools[0], ret_bools[1]) :
                string.Format("RegistryKey:{0} / Value:{1}", ret_bools[0], ret_bools[1]);

        }

        #endregion
        #region Hash

        public static string GetHash(string filePath, HashAlgorithm hash)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                string text = BitConverter.ToString(hash.ComputeHash(fs)).Replace("-", "");
                hash.Clear();
                return text;
            }
        }

        public static string GetHash(RegistryKey regKey, string name, HashAlgorithm hash)
        {
            byte[] bytes = RegistryControl.RegistryValueToBytes(regKey, name, null, true);
            string text = BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "");
            hash.Clear();
            return text;
        }

        #endregion
        #region Size

        public static string ToReadableSize(long size)
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
        #region TimeStamp

        public static string GetCreationTime(FileSystemInfo info, bool isDateOnly, bool isTimeOnly)
        {
            return DateToString(info.CreationTime, isDateOnly, isTimeOnly);
        }

        public static string GetLastWriteTime(FileSystemInfo info, bool isDateOnly, bool isTimeOnly)
        {
            return DateToString(info.LastWriteTime, isDateOnly, isTimeOnly);
        }

        public static string GetLastAccessTime(FileSystemInfo info, bool isDateOnly, bool isTimeOnly)
        {
            return DateToString(info.LastAccessTime, isDateOnly, isTimeOnly);
        }

        private static string DateToString(DateTime date, bool isDateOnly, bool isTimeOnly)
        {
            if (isDateOnly)
                return date.ToString("yyyy/MM/dd");
            else if (isTimeOnly)
                return date.ToString("HH:mm:ss");
            else
                return date.ToString("yyyy/MM/dd HH:mm:ss");
        }

        #endregion

    }
}
