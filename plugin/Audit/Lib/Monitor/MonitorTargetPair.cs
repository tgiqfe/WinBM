using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO.Lib;

namespace Audit.Lib.Monitor
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class MonitorTargetPair
    {
        public MonitorTarget TargetA { get; set; }
        public MonitorTarget TargetB { get; set; }

        public bool? IsCreationTime { get; set; }
        public bool? IsLastWriteTime { get; set; }
        public bool? IsLastAccessTime { get; set; }
        public bool? IsAccess { get; set; }
        public bool? IsOwner { get; set; }
        public bool? IsInherited { get; set; }
        public bool? IsAttributes { get; set; }
        public bool? IsMD5Hash { get; set; }
        public bool? IsSHA256Hash { get; set; }
        public bool? IsSHA512Hash { get; set; }
        public bool? IsSize { get; set; }
        public bool? IsChildCount { get; set; }
        public bool? IsRegistryType { get; set; }
        public bool? IsDateOnly { get; set; }
        public bool? IsTimeOnly { get; set; }

        public MonitorTargetPair(MonitorTarget targetA, MonitorTarget targetB)
        {
            this.TargetA = targetA;
            this.TargetB = targetB;
        }

        #region Check method

        public bool CheckFile(Dictionary<string, string> dictionary, int serial)
        {
            if (TargetA.PathType != PathType.File || TargetB.PathType != PathType.File)
            {
                return false;
            }

            bool ret = true;

            if (IsCreationTime ?? false)
            {
                TargetA.CreationTime = MonitorFunctions.GetCreationTime(TargetA.FileInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                TargetB.CreationTime = MonitorFunctions.GetCreationTime(TargetB.FileInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                dictionary[$"{serial}_{TargetA.PathTypeName}_CreationTime"] = TargetA.CreationTime;
                dictionary[$"{serial}_{TargetB.PathTypeName}_CreationTime"] = TargetB.CreationTime;
                ret &= TargetA.CreationTime == TargetB.CreationTime;
            }
            if (IsLastWriteTime ?? false)
            {
                TargetA.LastWriteTime = MonitorFunctions.GetLastWriteTime(TargetA.FileInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                TargetB.LastWriteTime = MonitorFunctions.GetLastWriteTime(TargetB.FileInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                dictionary[$"{serial}_{TargetA.PathTypeName}_LastWriteTime"] = TargetA.LastWriteTime;
                dictionary[$"{serial}_{TargetB.PathTypeName}_LastWriteTime"] = TargetB.LastWriteTime;
                ret &= TargetA.LastWriteTime == TargetB.LastWriteTime;
            }
            if (IsLastAccessTime ?? false)
            {
                TargetA.LastAccessTime = MonitorFunctions.GetLastAccessTime(TargetA.FileInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                TargetB.LastAccessTime = MonitorFunctions.GetLastAccessTime(TargetB.FileInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                dictionary[$"{serial}_{TargetA.PathTypeName}_LastAccessTime"] = TargetA.LastAccessTime;
                dictionary[$"{serial}_{TargetB.PathTypeName}_LastAccessTime"] = TargetB.LastAccessTime;
                ret &= TargetA.LastAccessTime == TargetB.LastAccessTime;
            }
            if (IsAccess ?? false)
            {
                TargetA.Access = AccessRuleSummary.FileToAccessString(TargetA.FileInfo);
                TargetB.Access = AccessRuleSummary.FileToAccessString(TargetB.FileInfo);
                dictionary[$"{serial}_{TargetA.PathTypeName}_Access"] = TargetA.Access;
                dictionary[$"{serial}_{TargetB.PathTypeName}_Access"] = TargetB.Access;
                ret &= TargetA.Access == TargetB.Access;
            }
            if (IsOwner ?? false)
            {
                TargetA.Owner = TargetA.FileInfo.GetAccessControl().GetOwner(typeof(System.Security.Principal.NTAccount)).Value;
                TargetB.Owner = TargetB.FileInfo.GetAccessControl().GetOwner(typeof(System.Security.Principal.NTAccount)).Value;
                dictionary[$"{serial}_{TargetA.PathTypeName}_Owner"] = TargetA.Owner;
                dictionary[$"{serial}_{TargetB.PathTypeName}_Owner"] = TargetB.Owner;
                ret &= TargetA.Owner == TargetB.Owner;
            }
            if (IsInherited ?? false)
            {
                TargetA.Inherited = !TargetA.FileInfo.GetAccessControl().AreAccessRulesProtected;
                TargetB.Inherited = !TargetB.FileInfo.GetAccessControl().AreAccessRulesProtected;
                dictionary[$"{serial}_{TargetA.PathTypeName}_Inherited"] = TargetA.Inherited.ToString();
                dictionary[$"{serial}_{TargetB.PathTypeName}_Inherited"] = TargetB.Inherited.ToString();
                ret &= TargetA.Inherited == TargetB.Inherited;
            }
            if (IsAttributes ?? false)
            {
                TargetA.Attributes = MonitorFunctions.GetAttributes(TargetA.Path);
                TargetB.Attributes = MonitorFunctions.GetAttributes(TargetB.Path);
                dictionary[$"{serial}_{TargetA.PathTypeName}_Attributes"] = MonitorFunctions.ToReadableAttributes(TargetA.Attributes);
                dictionary[$"{serial}_{TargetB.PathTypeName}_Attributes"] = MonitorFunctions.ToReadableAttributes(TargetB.Attributes);
                ret &= TargetA.Attributes.SequenceEqual(TargetB.Attributes);
            }
            if (IsMD5Hash ?? false)
            {
                TargetA.MD5Hash = MonitorFunctions.GetHash(TargetA.Path, System.Security.Cryptography.MD5.Create());
                TargetB.MD5Hash = MonitorFunctions.GetHash(TargetB.Path, System.Security.Cryptography.MD5.Create());
                dictionary[$"{serial}_{TargetA.PathTypeName}_MD5Hash"] = TargetA.MD5Hash;
                dictionary[$"{serial}_{TargetB.PathTypeName}_MD5Hash"] = TargetB.MD5Hash;
                ret &= TargetA.MD5Hash == TargetB.MD5Hash;
            }
            if (IsSHA256Hash ?? false)
            {
                TargetA.SHA256Hash = MonitorFunctions.GetHash(TargetA.Path, System.Security.Cryptography.SHA256.Create());
                TargetB.SHA256Hash = MonitorFunctions.GetHash(TargetB.Path, System.Security.Cryptography.SHA256.Create());
                dictionary[$"{serial}_{TargetA.PathTypeName}_SHA256Hash"] = TargetA.SHA256Hash;
                dictionary[$"{serial}_{TargetB.PathTypeName}_SHA256Hash"] = TargetB.SHA256Hash;
                ret &= TargetA.SHA256Hash == TargetB.SHA256Hash;
            }
            if (IsSHA512Hash ?? false)
            {
                TargetA.SHA256Hash = MonitorFunctions.GetHash(TargetA.Path, System.Security.Cryptography.SHA512.Create());
                TargetB.SHA256Hash = MonitorFunctions.GetHash(TargetB.Path, System.Security.Cryptography.SHA512.Create());
                dictionary[$"{serial}_{TargetA.PathTypeName}_SHA512Hash"] = TargetA.SHA512Hash;
                dictionary[$"{serial}_{TargetB.PathTypeName}_SHA512Hash"] = TargetB.SHA512Hash;
                ret &= TargetA.SHA512Hash == TargetB.SHA512Hash;
            }
            if (IsSize ?? false)
            {
                TargetA.Size = TargetA.FileInfo.Length;
                TargetB.Size = TargetB.FileInfo.Length;
                dictionary[$"{serial}_{TargetA.PathTypeName}_Size"] = TargetA.Size.ToString();
                dictionary[$"{serial}_{TargetB.PathTypeName}_Size"] = TargetB.Size.ToString();
                ret &= TargetA.Size == TargetB.Size;
            }

            return ret;
        }

        public bool CheckDirectory(Dictionary<string, string> dictionary, int serial, int depth)
        {
            if (TargetA.PathType != PathType.Directory || TargetB.PathType != PathType.Directory)
            {
                return false;
            }

            bool ret = true;

            if (IsCreationTime ?? false)
            {
                TargetA.CreationTime = MonitorFunctions.GetCreationTime(TargetA.DirectoryInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                TargetB.CreationTime = MonitorFunctions.GetCreationTime(TargetB.DirectoryInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                dictionary[$"{serial}_{TargetA.PathTypeName}_CreationTime"] = TargetA.CreationTime;
                dictionary[$"{serial}_{TargetB.PathTypeName}_CreationTime"] = TargetB.CreationTime;
                ret &= TargetA.CreationTime == TargetB.CreationTime;
            }
            if (IsLastWriteTime ?? false)
            {
                TargetA.LastWriteTime = MonitorFunctions.GetLastWriteTime(TargetA.DirectoryInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                TargetB.LastWriteTime = MonitorFunctions.GetLastWriteTime(TargetB.DirectoryInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                dictionary[$"{serial}_{TargetA.PathTypeName}_LastWriteTime"] = TargetA.LastWriteTime;
                dictionary[$"{serial}_{TargetB.PathTypeName}_LastWriteTime"] = TargetB.LastWriteTime;
                ret &= TargetA.LastWriteTime == TargetB.LastWriteTime;
            }
            if (IsLastAccessTime ?? false)
            {
                TargetA.LastAccessTime = MonitorFunctions.GetLastAccessTime(TargetA.DirectoryInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                TargetB.LastAccessTime = MonitorFunctions.GetLastAccessTime(TargetB.DirectoryInfo, this.IsDateOnly ?? false, this.IsTimeOnly ?? false);
                dictionary[$"{serial}_{TargetA.PathTypeName}_LastAccessTime"] = TargetA.LastAccessTime;
                dictionary[$"{serial}_{TargetB.PathTypeName}_LastAccessTime"] = TargetB.LastAccessTime;
                ret &= TargetA.LastAccessTime == TargetB.LastAccessTime;
            }
            if (IsAccess ?? false)
            {
                TargetA.Access = AccessRuleSummary.DirectoryToAccessString(TargetA.DirectoryInfo);
                TargetB.Access = AccessRuleSummary.DirectoryToAccessString(TargetB.DirectoryInfo);
                dictionary[$"{serial}_{TargetA.PathTypeName}_Access"] = TargetA.Access;
                dictionary[$"{serial}_{TargetB.PathTypeName}_Access"] = TargetB.Access;
                ret &= TargetA.Access == TargetB.Access;
            }
            if (IsOwner ?? false)
            {
                TargetA.Owner = TargetA.DirectoryInfo.GetAccessControl().GetOwner(typeof(System.Security.Principal.NTAccount)).Value;
                TargetB.Owner = TargetB.DirectoryInfo.GetAccessControl().GetOwner(typeof(System.Security.Principal.NTAccount)).Value;
                dictionary[$"{serial}_{TargetA.PathTypeName}_Owner"] = TargetA.Owner;
                dictionary[$"{serial}_{TargetB.PathTypeName}_Owner"] = TargetB.Owner;
                ret &= TargetA.Owner == TargetB.Owner;
            }
            if (IsInherited ?? false)
            {
                TargetA.Inherited = !TargetA.DirectoryInfo.GetAccessControl().AreAccessRulesProtected;
                TargetB.Inherited = !TargetB.DirectoryInfo.GetAccessControl().AreAccessRulesProtected;
                dictionary[$"{serial}_{TargetA.PathTypeName}_Inherited"] = TargetA.Inherited.ToString();
                dictionary[$"{serial}_{TargetB.PathTypeName}_Inherited"] = TargetB.Inherited.ToString();
                ret &= TargetA.Inherited == TargetB.Inherited;
            }
            if (IsAttributes ?? false)
            {
                TargetA.Attributes = MonitorFunctions.GetAttributes(TargetA.Path);
                TargetB.Attributes = MonitorFunctions.GetAttributes(TargetB.Path);
                dictionary[$"{serial}_{TargetA.PathTypeName}_Attributes"] = MonitorFunctions.ToReadableAttributes(TargetA.Attributes);
                dictionary[$"{serial}_{TargetB.PathTypeName}_Attributes"] = MonitorFunctions.ToReadableAttributes(TargetB.Attributes);
                ret &= TargetA.Attributes.SequenceEqual(TargetB.Attributes);
            }
            if ((IsChildCount ?? false) && depth == 0)
            {
                TargetA.ChildCount = MonitorFunctions.GetDirectoryChildCount(TargetA.Path);
                TargetB.ChildCount = MonitorFunctions.GetDirectoryChildCount(TargetB.Path);
                dictionary[$"{serial}_{TargetA.PathTypeName}_ChildCount"] = MonitorFunctions.ToReadableChildCount(TargetA.ChildCount, TargetA.PathType == IO.Lib.PathType.Directory);
                dictionary[$"{serial}_{TargetB.PathTypeName}_ChildCount"] = MonitorFunctions.ToReadableChildCount(TargetB.ChildCount, TargetB.PathType == IO.Lib.PathType.Directory);
                ret &= TargetA.ChildCount.SequenceEqual(TargetB.ChildCount);
            }

            return ret;
        }

        public bool CheckRegistryKey(Dictionary<string, string> dictionary, int serial, int depth)
        {
            if (TargetA.PathType != PathType.Registry || TargetB.PathType != PathType.Registry)
            {
                return false;
            }

            bool ret = true;

            if (IsAccess ?? false)
            {
                TargetA.Access = AccessRuleSummary.RegistryKeyToAccessString(TargetA.Key);
                TargetB.Access = AccessRuleSummary.RegistryKeyToAccessString(TargetB.Key);
                dictionary[$"{serial}_{TargetA.PathTypeName}_Access"] = TargetA.Access;
                dictionary[$"{serial}_{TargetB.PathTypeName}_Access"] = TargetB.Access;
                ret &= TargetA.Access == TargetB.Access;
            }
            if (IsOwner ?? false)
            {
                TargetA.Owner = TargetA.Key.GetAccessControl().GetOwner(typeof(System.Security.Principal.NTAccount)).Value;
                TargetB.Owner = TargetB.Key.GetAccessControl().GetOwner(typeof(System.Security.Principal.NTAccount)).Value;
                dictionary[$"{serial}_{TargetA.PathTypeName}_Owner"] = TargetA.Owner;
                dictionary[$"{serial}_{TargetB.PathTypeName}_Owner"] = TargetB.Owner;
                ret &= TargetA.Owner == TargetB.Owner;
            }
            if (IsInherited ?? false)
            {
                TargetA.Inherited = !TargetA.Key.GetAccessControl().AreAccessRulesProtected;
                TargetB.Inherited = !TargetB.Key.GetAccessControl().AreAccessRulesProtected;
                dictionary[$"{serial}_{TargetA.PathTypeName}_Inherited"] = TargetA.Inherited.ToString();
                dictionary[$"{serial}_{TargetB.PathTypeName}_Inherited"] = TargetB.Inherited.ToString();
                ret &= TargetA.Inherited == TargetB.Inherited;
            }
            if ((IsChildCount ?? false) && depth == 0)
            {
                TargetA.ChildCount = MonitorFunctions.GetRegistryKeyChildCount(TargetA.Key);
                TargetB.ChildCount = MonitorFunctions.GetRegistryKeyChildCount(TargetB.Key);
                dictionary[$"{serial}_{TargetA.PathTypeName}_ChildCount"] = MonitorFunctions.ToReadableChildCount(TargetA.ChildCount, TargetA.PathType == IO.Lib.PathType.Directory);
                dictionary[$"{serial}_{TargetB.PathTypeName}_ChildCount"] = MonitorFunctions.ToReadableChildCount(TargetB.ChildCount, TargetB.PathType == IO.Lib.PathType.Directory);
                ret &= TargetA.ChildCount.SequenceEqual(TargetB.ChildCount);
            }

            return ret;
        }

        public bool CheckRegistryValue(Dictionary<string, string> dictionary, int serial)
        {
            if (TargetA.PathType != PathType.Registry || TargetB.PathType != PathType.Registry)
            {
                return false;
            }

            bool ret = true;

            if (IsMD5Hash ?? false)
            {
                TargetA.MD5Hash = MonitorFunctions.GetHash(TargetA.Key, TargetA.Name, System.Security.Cryptography.MD5.Create());
                TargetB.MD5Hash = MonitorFunctions.GetHash(TargetB.Key, TargetB.Name, System.Security.Cryptography.MD5.Create());
                dictionary[$"{serial}_{TargetA.PathTypeName}_MD5Hash"] = TargetA.MD5Hash;
                dictionary[$"{serial}_{TargetB.PathTypeName}_MD5Hash"] = TargetB.MD5Hash;
                ret &= TargetA.MD5Hash == TargetB.MD5Hash;
            }
            if (IsSHA256Hash ?? false)
            {
                TargetA.SHA256Hash = MonitorFunctions.GetHash(TargetA.Key, TargetA.Name, System.Security.Cryptography.SHA256.Create());
                TargetB.SHA256Hash = MonitorFunctions.GetHash(TargetB.Key, TargetB.Name, System.Security.Cryptography.SHA256.Create());
                dictionary[$"{serial}_{TargetA.PathTypeName}_SHA256Hash"] = TargetA.SHA256Hash;
                dictionary[$"{serial}_{TargetB.PathTypeName}_SHA256Hash"] = TargetB.SHA256Hash;
                ret &= TargetA.SHA256Hash == TargetB.SHA256Hash;
            }
            if (IsSHA512Hash ?? false)
            {
                TargetA.SHA512Hash = MonitorFunctions.GetHash(TargetA.Key, TargetA.Name, System.Security.Cryptography.SHA512.Create());
                TargetB.SHA512Hash = MonitorFunctions.GetHash(TargetB.Key, TargetB.Name, System.Security.Cryptography.SHA512.Create());
                dictionary[$"{serial}_{TargetA.PathTypeName}_SHA512Hash"] = TargetA.SHA512Hash;
                dictionary[$"{serial}_{TargetB.PathTypeName}_SHA512Hash"] = TargetB.SHA512Hash;
                ret &= TargetA.SHA512Hash == TargetB.SHA512Hash;
            }
            if (IsRegistryType ?? false)
            {
                TargetA.RegistryType = RegistryControl.ValueKindToString(TargetA.Key.GetValueKind(TargetA.Name));
                TargetB.RegistryType = RegistryControl.ValueKindToString(TargetB.Key.GetValueKind(TargetB.Name));
                dictionary[$"{serial}_{TargetA.PathTypeName}_RegistryType"] = TargetA.RegistryType;
                dictionary[$"{serial}_{TargetB.PathTypeName}_RegistryType"] = TargetB.RegistryType;
                ret &= TargetA.RegistryType == TargetB.RegistryType;
            }

            return ret;
        }

        #endregion
    }
}
