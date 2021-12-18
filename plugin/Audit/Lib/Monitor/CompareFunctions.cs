using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Lib.Monitor
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class CompareFunctions
    {
        /// <summary>
        /// ファイルのCompareチェック
        /// </summary>
        /// <param name="targetA"></param>
        /// <param name="targetB"></param>
        /// <param name="dictionary"></param>
        /// <param name="serial"></param>
        /// <returns></returns>
        internal static bool CheckFile(MonitorTarget targetA, MonitorTarget targetB, Dictionary<string, string> dictionary, int serial)
        {
            bool ret = true;

            if (targetA.IsCreationTime ?? false)
            {
                targetA.CheckCreationTime();
                targetB.CheckCreationTime();
                dictionary[$"{serial}_{targetA.PathTypeName}_CreationTime"] = targetA.CreationTime;
                dictionary[$"{serial}_{targetB.PathTypeName}_CreationTime"] = targetB.CreationTime;
                ret &= targetA.CreationTime == targetB.CreationTime;
            }
            if (targetA.IsLastWriteTime ?? false)
            {
                targetA.CheckLastWriteTime();
                targetB.CheckLastWriteTime();
                dictionary[$"{serial}_{targetA.PathTypeName}_LastWriteTime"] = targetA.LastWriteTime;
                dictionary[$"{serial}_{targetB.PathTypeName}_LastWriteTime"] = targetB.LastWriteTime;
                ret &= targetA.LastWriteTime == targetB.LastWriteTime;
            }
            if (targetA.IsLastAccessTime ?? false)
            {
                targetA.CheckLastAccessTime();
                targetB.CheckLastAccessTime();
                dictionary[$"{serial}_{targetA.PathTypeName}_LastAccessTime"] = targetA.LastAccessTime;
                dictionary[$"{serial}_{targetB.PathTypeName}_LastAccessTime"] = targetB.LastAccessTime;
                ret &= targetA.LastAccessTime == targetB.LastAccessTime;
            }
            if (targetA.IsAccess ?? false)
            {
                targetA.CheckAccess();
                targetB.CheckAccess();
                dictionary[$"{serial}_{targetA.PathTypeName}_Access"] = targetA.Access;
                dictionary[$"{serial}_{targetB.PathTypeName}_Access"] = targetB.Access;
                ret &= targetA.Access == targetB.Access;
            }
            if (targetA.IsOwner ?? false)
            {
                targetA.CheckOwner();
                targetB.CheckOwner();
                dictionary[$"{serial}_{targetA.PathTypeName}_Owner"] = targetA.Owner;
                dictionary[$"{serial}_{targetB.PathTypeName}_Owner"] = targetB.Owner;
                ret &= targetA.Owner == targetB.Owner;
            }
            if (targetA.IsInherited ?? false)
            {
                targetA.CheckInherited();
                targetB.CheckInherited();
                dictionary[$"{serial}_{targetA.PathTypeName}_Inherited"] = targetA.Inherited.ToString();
                dictionary[$"{serial}_{targetB.PathTypeName}_Inherited"] = targetB.Inherited.ToString();
                ret &= targetA.Inherited == targetB.Inherited;
            }
            if (targetA.IsAttributes ?? false)
            {
                targetA.CheckAttributes();
                targetB.CheckAttributes();
                dictionary[$"{serial}_{targetA.PathTypeName}_Attributes"] = MonitorFunctions.ToReadableAttributes(targetA.Attributes);
                dictionary[$"{serial}_{targetB.PathTypeName}_Attributes"] = MonitorFunctions.ToReadableAttributes(targetB.Attributes);
                ret &= targetA.Attributes.SequenceEqual(targetB.Attributes);
            }
            if (targetA.IsMD5Hash ?? false)
            {
                targetA.CheckMD5Hash();
                targetB.CheckMD5Hash();
                dictionary[$"{serial}_{targetA.PathTypeName}_MD5Hash"] = targetA.MD5Hash;
                dictionary[$"{serial}_{targetB.PathTypeName}_MD5Hash"] = targetB.MD5Hash;
                ret &= targetA.MD5Hash == targetB.MD5Hash;
            }
            if (targetA.IsSHA256Hash ?? false)
            {
                targetA.CheckSHA256Hash();
                targetB.CheckSHA256Hash();
                dictionary[$"{serial}_{targetA.PathTypeName}_SHA256Hash"] = targetA.SHA256Hash;
                dictionary[$"{serial}_{targetB.PathTypeName}_SHA256Hash"] = targetB.SHA256Hash;
                ret &= targetA.SHA256Hash == targetB.SHA256Hash;
            }
            if (targetA.IsSHA512Hash ?? false)
            {
                targetA.CheckSHA512Hash();
                targetB.CheckSHA512Hash();
                dictionary[$"{serial}_{targetA.PathTypeName}_SHA512Hash"] = targetA.SHA512Hash;
                dictionary[$"{serial}_{targetB.PathTypeName}_SHA512Hash"] = targetB.SHA512Hash;
                ret &= targetA.SHA512Hash == targetB.SHA512Hash;
            }
            if (targetA.IsSize ?? false)
            {
                targetA.CheckSize();
                targetB.CheckSize();
                dictionary[$"{serial}_{targetA.PathTypeName}_Size"] = targetA.Size.ToString();
                dictionary[$"{serial}_{targetB.PathTypeName}_Size"] = targetB.Size.ToString();
                ret &= targetA.SHA512Hash == targetB.SHA512Hash;
            }

            return ret;
        }

        /// <summary>
        /// ディレクトリのCompareチェック
        /// </summary>
        /// <param name="targetA"></param>
        /// <param name="targetB"></param>
        /// <param name="dictionary"></param>
        /// <param name="serial"></param>
        /// <returns></returns>
        internal static bool CheckDirectory(MonitorTarget targetA, MonitorTarget targetB, Dictionary<string, string> dictionary, int serial, int depth)
        {
            bool ret = true;

            if (targetA.IsCreationTime ?? false)
            {
                targetA.CheckCreationTime();
                targetB.CheckCreationTime();
                dictionary[$"{serial}_{targetA.PathTypeName}_CreationTime"] = targetA.CreationTime;
                dictionary[$"{serial}_{targetB.PathTypeName}_CreationTime"] = targetB.CreationTime;
                ret &= targetA.CreationTime == targetB.CreationTime;
            }
            if (targetA.IsLastWriteTime ?? false)
            {
                targetA.CheckLastWriteTime();
                targetB.CheckLastWriteTime();
                dictionary[$"{serial}_{targetA.PathTypeName}_LastWriteTime"] = targetA.LastWriteTime;
                dictionary[$"{serial}_{targetB.PathTypeName}_LastWriteTime"] = targetB.LastWriteTime;
                ret &= targetA.LastWriteTime == targetB.LastWriteTime;
            }
            if (targetA.IsLastAccessTime ?? false)
            {
                targetA.CheckLastAccessTime();
                targetB.CheckLastAccessTime();
                dictionary[$"{serial}_{targetA.PathTypeName}_LastAccessTime"] = targetA.LastAccessTime;
                dictionary[$"{serial}_{targetB.PathTypeName}_LastAccessTime"] = targetB.LastAccessTime;
                ret &= targetA.LastAccessTime == targetB.LastAccessTime;
            }
            if (targetA.IsAccess ?? false)
            {
                targetA.CheckAccess();
                targetB.CheckAccess();
                dictionary[$"{serial}_{targetA.PathTypeName}_Access"] = targetA.Access;
                dictionary[$"{serial}_{targetB.PathTypeName}_Access"] = targetB.Access;
                ret &= targetA.Access == targetB.Access;
            }
            if (targetA.IsOwner ?? false)
            {
                targetA.CheckOwner();
                targetB.CheckOwner();
                dictionary[$"{serial}_{targetA.PathTypeName}_Owner"] = targetA.Owner;
                dictionary[$"{serial}_{targetB.PathTypeName}_Owner"] = targetB.Owner;
                ret &= targetA.Owner == targetB.Owner;
            }
            if (targetA.IsInherited ?? false)
            {
                targetA.CheckInherited();
                targetB.CheckInherited();
                dictionary[$"{serial}_{targetA.PathTypeName}_Inherited"] = targetA.Inherited.ToString();
                dictionary[$"{serial}_{targetB.PathTypeName}_Inherited"] = targetB.Inherited.ToString();
                ret &= targetA.Inherited == targetB.Inherited;
            }
            if (targetA.IsAttributes ?? false)
            {
                targetA.CheckAttributes();
                targetB.CheckAttributes();
                dictionary[$"{serial}_{targetA.PathTypeName}_Attributes"] = MonitorFunctions.ToReadableAttributes(targetA.Attributes);
                dictionary[$"{serial}_{targetB.PathTypeName}_Attributes"] = MonitorFunctions.ToReadableAttributes(targetB.Attributes);
                ret &= targetA.Attributes.SequenceEqual(targetB.Attributes);
            }
            if ((targetA.IsChildCount ?? false) && depth == 0)
            {
                targetA.CheckChildCount();
                targetB.CheckChildCount();
                dictionary[$"{serial}_{targetA.PathTypeName}_ChildCount"] = MonitorFunctions.ToReadableChildCount(targetA.ChildCount, targetA.PathType == IO.Lib.PathType.Directory);
                dictionary[$"{serial}_{targetB.PathTypeName}_ChildCount"] = MonitorFunctions.ToReadableChildCount(targetB.ChildCount, targetB.PathType == IO.Lib.PathType.Directory);
                ret &= targetA.ChildCount.SequenceEqual(targetB.ChildCount);
            }

            return ret;
        }

        /// <summary>
        /// レジストリキーのCompareチェック
        /// </summary>
        /// <param name="targetA"></param>
        /// <param name="targetB"></param>
        /// <param name="dictionary"></param>
        /// <param name="serial"></param>
        /// <returns></returns>
        internal static bool CheckRegistryKey(MonitorTarget targetA, MonitorTarget targetB, Dictionary<string, string> dictionary, int serial, int depth)
        {
            bool ret = true;

            if (targetA.IsAccess ?? false)
            {
                targetA.CheckAccess();
                targetB.CheckAccess();
                dictionary[$"{serial}_{targetA.PathTypeName}_Access"] = targetA.Access;
                dictionary[$"{serial}_{targetB.PathTypeName}_Access"] = targetB.Access;
                ret &= targetA.Access == targetB.Access;
            }
            if (targetA.IsOwner ?? false)
            {
                targetA.CheckOwner();
                targetB.CheckOwner();
                dictionary[$"{serial}_{targetA.PathTypeName}_Owner"] = targetA.Owner;
                dictionary[$"{serial}_{targetB.PathTypeName}_Owner"] = targetB.Owner;
                ret &= targetA.Owner == targetB.Owner;
            }
            if (targetA.IsInherited ?? false)
            {
                targetA.CheckInherited();
                targetB.CheckInherited();
                dictionary[$"{serial}_{targetA.PathTypeName}_Inherited"] = targetA.Inherited.ToString();
                dictionary[$"{serial}_{targetB.PathTypeName}_Inherited"] = targetB.Inherited.ToString();
                ret &= targetA.Inherited == targetB.Inherited;
            }
            if ((targetA.IsChildCount ?? false) && depth == 0)
            {
                targetA.CheckChildCount();
                targetB.CheckChildCount();
                dictionary[$"{serial}_{targetA.PathTypeName}_ChildCount"] = MonitorFunctions.ToReadableChildCount(targetA.ChildCount, targetA.PathType == IO.Lib.PathType.Directory);
                dictionary[$"{serial}_{targetB.PathTypeName}_ChildCount"] = MonitorFunctions.ToReadableChildCount(targetB.ChildCount, targetB.PathType == IO.Lib.PathType.Directory);
                ret &= targetA.ChildCount.SequenceEqual(targetB.ChildCount);
            }

            return ret;
        }

        /// <summary>
        /// レジストリ値のCompareチェック
        /// </summary>
        /// <param name="targetA"></param>
        /// <param name="targetB"></param>
        /// <param name="dictionary"></param>
        /// <param name="serial"></param>
        /// <returns></returns>
        internal static bool CheckRegistryValue(MonitorTarget targetA, MonitorTarget targetB, Dictionary<string, string> dictionary, int serial)
        {
            bool ret = true;

            if (targetA.IsMD5Hash ?? false)
            {
                targetA.CheckMD5Hash();
                targetB.CheckMD5Hash();
                dictionary[$"{serial}_{targetA.PathTypeName}_MD5Hash"] = targetA.MD5Hash;
                dictionary[$"{serial}_{targetB.PathTypeName}_MD5Hash"] = targetB.MD5Hash;
                ret &= targetA.MD5Hash == targetB.MD5Hash;
            }
            if (targetA.IsSHA256Hash ?? false)
            {
                targetA.CheckSHA256Hash();
                targetB.CheckSHA256Hash();
                dictionary[$"{serial}_{targetA.PathTypeName}_SHA256Hash"] = targetA.SHA256Hash;
                dictionary[$"{serial}_{targetB.PathTypeName}_SHA256Hash"] = targetB.SHA256Hash;
                ret &= targetA.SHA256Hash == targetB.SHA256Hash;
            }
            if (targetA.IsSHA512Hash ?? false)
            {
                targetA.CheckSHA512Hash();
                targetB.CheckSHA512Hash();
                dictionary[$"{serial}_{targetA.PathTypeName}_SHA512Hash"] = targetA.SHA512Hash;
                dictionary[$"{serial}_{targetB.PathTypeName}_SHA512Hash"] = targetB.SHA512Hash;
                ret &= targetA.SHA512Hash == targetB.SHA512Hash;
            }
            if (targetA.IsRegistryType ?? false)
            {
                targetA.CheckRegistryType();
                targetB.CheckRegistryType();
                dictionary[$"{serial}_{targetA.PathTypeName}_RegistryType"] = targetA.RegistryType;
                dictionary[$"{serial}_{targetB.PathTypeName}_RegistryType"] = targetB.RegistryType;
                ret &= targetA.RegistryType == targetB.RegistryType;
            }

            return ret;
        }
    }
}
