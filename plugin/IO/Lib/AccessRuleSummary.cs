using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Security.AccessControl;
using Microsoft.Win32;
using System.IO;

namespace IO.Lib
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class AccessRuleSummary
    {
        public PathType Type { get; set; }
        public NTAccount Account { get; set; }
        public FileSystemRights? FileSystemRights { get; set; }
        public RegistryRights? RegistryRights { get; set; }
        public InheritanceFlags? InheritanceFlags { get; set; }
        public PropagationFlags? PropagationFlags { get; set; }
        public AccessControlType AccessControlType { get; set; }

        #region Candidate

        private static string[] candidate_readOnly = new string[]
        {
            "readandexecute","readandexec","readadnexe","readexec","readexe","readonly","read", "readkey"
        };
        private static string[] candidate_modify = new string[]
        {
            "modify", "mod",
        };
        private static string[] candidate_fullControl = new string[]
        {
            "fullcontrol", "full",
        };
        private static string[] candidate_allow = new string[]
        {
            "allow", "alow", "arrow"
        };
        private static string[] candidate_deny = new string[]
        {
            "deny", "denied"
        };

        private static string[] candidate_containerInherit = new string[]
        {
            "containerinherit", "container inherit", "containerinheritance", "container inheritance"
        };
        private static string[] candidate_objectInherit = new string[]
        {
            "objectinherit", "object inherit", "objectinheritance", "object inheritance"
        };
        private static string[] candidate_none = new string[]
        {
            "none", "non"
        };

        private static string[] candidate_inheritOnly = new string[]
        {
            "inheritonly", "inherit only", "inheritanceonly", "inheritance only"
        };
        private static string[] candidate_noPropagateInherit = new string[]
        {
            "nopropagateinherit", "no propagate inherit", "nopropagateinheritance", "no propagate inheritance"
        };

        #endregion

        /// <summary>
        /// ファイルのアクセスルール
        /// </summary>
        /// <param name="account">アカウント名</param>
        /// <param name="rights">権限</param>
        /// <param name="accessControlType">アクセス許可種別</param>
        public AccessRuleSummary(NTAccount account, FileSystemRights rights, AccessControlType accessControlType)
        {
            this.Type = PathType.File;
            this.Account = account;
            this.FileSystemRights = rights;
            this.AccessControlType = accessControlType;
        }

        /// <summary>
        /// ディレクトリのアクセスルール
        /// </summary>
        /// <param name="account">アカウント名</param>
        /// <param name="rights">権限</param>
        /// <param name="inheritanceFlags">下位への継承</param>
        /// <param name="propagationFlags">上位からの継承</param>
        /// <param name="accessControlType">アクセス許可種別</param>
        public AccessRuleSummary(NTAccount account, FileSystemRights rights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType accessControlType)
        {
            this.Type = PathType.Directory;
            this.Account = account;
            this.FileSystemRights = rights;
            this.InheritanceFlags = inheritanceFlags;
            this.PropagationFlags = propagationFlags;
            this.AccessControlType = accessControlType;
        }

        /// <summary>
        /// レジストリキーのアクセスルール
        /// </summary>
        /// <param name="account">アカウント名</param>
        /// <param name="rights">権限</param>
        /// <param name="inheritanceFlags">下位への継承</param>
        /// <param name="propagationFlags">上位からの継承</param>
        /// <param name="accessControlType">アクセス許可種別</param>
        public AccessRuleSummary(NTAccount account, RegistryRights rights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType accessControlType)
        {
            this.Type = PathType.Registry;
            this.Account = account;
            this.RegistryRights = rights;
            this.InheritanceFlags = inheritanceFlags;
            this.PropagationFlags = propagationFlags;
            this.AccessControlType = accessControlType;
        }

        #region To method

        /// <summary>
        /// Access文字列化
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Type switch
            {
                PathType.File => string.Format("{0};{1};{2}",
                    this.Account.Value,
                    this.FileSystemRights,
                    this.AccessControlType),
                PathType.Directory => string.Format("{0};{1};{2};{3};{4}",
                    this.Account.Value,
                    this.FileSystemRights,
                    this.InheritanceFlags,
                    this.PropagationFlags,
                    this.AccessControlType),
                PathType.Registry => string.Format("{0};{1};{2};{3};{4}",
                    this.Account.Value,
                    this.RegistryRights,
                    this.InheritanceFlags,
                    this.PropagationFlags,
                    this.AccessControlType),
                _ => null
            };
        }

        /// <summary>
        /// AccessRule化
        /// </summary>
        /// <returns></returns>
        public AccessRule ToAccessRule()
        {
            return this.Type switch
            {
                PathType.File => new FileSystemAccessRule(
                    this.Account,
                    this.FileSystemRights ?? System.Security.AccessControl.FileSystemRights.ReadAndExecute,
                    this.AccessControlType),
                PathType.Directory => new FileSystemAccessRule(
                    this.Account,
                    this.FileSystemRights ?? System.Security.AccessControl.FileSystemRights.ReadAndExecute,
                    this.InheritanceFlags ?? default(System.Security.AccessControl.InheritanceFlags),
                    this.PropagationFlags ?? default(System.Security.AccessControl.PropagationFlags),
                    this.AccessControlType),
                PathType.Registry => new RegistryAccessRule(
                    this.Account,
                    this.RegistryRights ?? System.Security.AccessControl.RegistryRights.ReadKey,
                    this.InheritanceFlags ?? default(System.Security.AccessControl.InheritanceFlags),
                    this.PropagationFlags ?? default(System.Security.AccessControl.PropagationFlags),
                    this.AccessControlType),
                _ => null
            };
        }

        #endregion
        #region From method

        public static AccessRuleSummary[] FromAccessString(string accessString, PathType pathType)
        {
            var list = new List<AccessRuleSummary>();
            string[] accesses = accessString.Split('/');

            switch (pathType)
            {
                case PathType.File:
                    accesses.ToList().ForEach(x =>
                    {
                        string[] fields = x.Split(';');
                        fields[0] = PredefinedAccount.Resolv(fields[0]);
                        if (fields.Length >= 3)
                        {
                            list.Add(new AccessRuleSummary(
                                new NTAccount(fields[0]),
                                GetFileSystemRights(fields[1]),
                                GetAccessControlType(fields[2])));
                        }
                    });
                    return list.ToArray();
                case PathType.Directory:
                    accesses.ToList().ForEach(x =>
                    {
                        string[] fields = x.Split(';');
                        fields[0] = PredefinedAccount.Resolv(fields[0]);
                        if (fields.Length >= 5)
                        {
                            list.Add(new AccessRuleSummary(
                                new NTAccount(fields[0]),
                                GetFileSystemRights(fields[1]),
                                GetInheritanceFlags(fields[2]),
                                GetPropagationFlags(fields[3]),
                                GetAccessControlType(fields[4])));
                        }
                    });
                    return list.ToArray();
                case PathType.Registry:
                    accesses.ToList().ForEach(x =>
                    {
                        string[] fields = x.Split(';');
                        fields[0] = PredefinedAccount.Resolv(fields[0]);
                        if (fields.Length >= 5)
                        {
                            list.Add(new AccessRuleSummary(
                                new NTAccount(fields[0]),
                                GetRegistryRights(fields[1]),
                                GetInheritanceFlags(fields[2]),
                                GetPropagationFlags(fields[3]),
                                GetAccessControlType(fields[4])));
                        }
                    });
                    return list.ToArray();
            }

            return null;
        }

        public static AccessRuleSummary[] FromAccessRules(AuthorizationRuleCollection rules, PathType pathType)
        {
            switch (pathType)
            {
                case PathType.File:
                    return rules.OfType<FileSystemAccessRule>().
                        ToList().
                        Select(x => new AccessRuleSummary(
                            (NTAccount)x.IdentityReference,
                            x.FileSystemRights == System.Security.AccessControl.FileSystemRights.FullControl ?
                                System.Security.AccessControl.FileSystemRights.FullControl :
                                (x.FileSystemRights & (~System.Security.AccessControl.FileSystemRights.Synchronize)),
                            x.AccessControlType)).
                        ToArray();
                case PathType.Directory:
                    return rules.OfType<FileSystemAccessRule>().
                        ToList().
                        Select(x => new AccessRuleSummary(
                            (NTAccount)x.IdentityReference,
                            x.FileSystemRights == System.Security.AccessControl.FileSystemRights.FullControl ?
                                System.Security.AccessControl.FileSystemRights.FullControl :
                                (x.FileSystemRights & (~System.Security.AccessControl.FileSystemRights.Synchronize)),
                            x.InheritanceFlags,
                            x.PropagationFlags,
                            x.AccessControlType)).
                        ToArray();
                case PathType.Registry:
                    return rules.OfType<RegistryAccessRule>().
                        ToList().
                        Select(x => new AccessRuleSummary(
                            (NTAccount)x.IdentityReference,
                            x.RegistryRights,
                            x.InheritanceFlags,
                            x.PropagationFlags,
                            x.AccessControlType)).
                        ToArray();
            }

            return null;
        }

        /// <summary>
        /// ファイルから取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static AccessRuleSummary[] FromFile(string path)
        {
            return File.Exists(path) ?
                FromAccessRules(
                    new FileInfo(path).GetAccessControl().GetAccessRules(true, false, typeof(NTAccount)), PathType.File) :
                null;
        }

        /// <summary>
        /// ディレクトリから取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static AccessRuleSummary[] FromDirectory(string path)
        {
            return Directory.Exists(path) ?
                FromAccessRules(
                    new DirectoryInfo(path).GetAccessControl().GetAccessRules(true, false, typeof(NTAccount)), PathType.Directory) :
                null;
        }

        /// <summary>
        /// レジストリパス(文字列)から取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static AccessRuleSummary[] FromRegistry(string path)
        {
            using (var regKey = RegistryControl.GetRegistryKey(path, false, false))
            {
                return FromRegistry(regKey);
            }
        }

        /// <summary>
        /// RegistryKeyから取得
        /// </summary>
        /// <param name="regKey"></param>
        /// <returns></returns>
        public static AccessRuleSummary[] FromRegistry(RegistryKey regKey)
        {
            return regKey == null ?
                null :
                FromAccessRules(
                    regKey.GetAccessControl().GetAccessRules(true, false, typeof(NTAccount)), PathType.Registry);
        }

        #endregion
        #region GetParameter

        private static FileSystemRights GetFileSystemRights(string rights)
        {
            switch (rights)
            {
                case string s when candidate_modify.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                    return System.Security.AccessControl.FileSystemRights.Modify;
                case string s when candidate_fullControl.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                    return System.Security.AccessControl.FileSystemRights.FullControl;
                case string s when candidate_readOnly.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                default:
                    return System.Security.AccessControl.FileSystemRights.ReadAndExecute;
            }
        }

        private static RegistryRights GetRegistryRights(string rights)
        {
            switch (rights)
            {
                case string s when candidate_fullControl.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                    return System.Security.AccessControl.RegistryRights.ReadKey;
                case string s when candidate_readOnly.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                default:
                    return System.Security.AccessControl.RegistryRights.ReadKey;
            }
        }

        private static InheritanceFlags GetInheritanceFlags(string inheritanceFlags)
        {
            InheritanceFlags flags = default(InheritanceFlags);
            foreach (string flagString in inheritanceFlags.Split(',').Select(x => x.Trim()).ToArray())
            {
                switch (flagString)
                {
                    case string s when candidate_containerInherit.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                        flags |= System.Security.AccessControl.InheritanceFlags.ContainerInherit;
                        break;
                    case string s when candidate_objectInherit.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                        flags |= System.Security.AccessControl.InheritanceFlags.ObjectInherit;
                        break;
                    case string s when candidate_none.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                        flags |= System.Security.AccessControl.InheritanceFlags.None;
                        break;
                    default:
                        break;
                }
            }
            return flags;
        }

        private static PropagationFlags GetPropagationFlags(string propagationFlags)
        {
            PropagationFlags flags = default(PropagationFlags);
            foreach (string flagString in propagationFlags.Split(',').Select(x => x.Trim()).ToArray())
            {
                switch (flagString)
                {
                    case String s when candidate_inheritOnly.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                        flags |= System.Security.AccessControl.PropagationFlags.InheritOnly;
                        break;
                    case String s when candidate_noPropagateInherit.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                        flags |= System.Security.AccessControl.PropagationFlags.NoPropagateInherit;
                        break;
                    case String s when candidate_none.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                        flags |= System.Security.AccessControl.PropagationFlags.None;
                        break;
                    default:
                        break;
                }
            }
            return flags;
        }

        private static AccessControlType GetAccessControlType(string accessControlType)
        {
            switch (accessControlType)
            {
                case string s when candidate_deny.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                    return AccessControlType.Deny;
                case string s when candidate_allow.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                default:
                    return AccessControlType.Allow;
            }
        }

        #endregion
        #region Compare method

        public bool Compare(AccessRuleSummary summary)
        {
            if (this.Type == summary.Type)
            {
                switch (this.Type)
                {
                    case PathType.File:
                        return
                            (this.Account == summary.Account) &&
                            (this.FileSystemRights == summary.FileSystemRights) &&
                            (this.AccessControlType == summary.AccessControlType);
                    case PathType.Directory:
                        return
                            (this.Account == summary.Account) &&
                            (this.FileSystemRights == summary.FileSystemRights) &&
                            (this.InheritanceFlags == summary.InheritanceFlags) &&
                            (this.PropagationFlags == summary.PropagationFlags) &&
                            (this.AccessControlType == summary.AccessControlType);
                    case PathType.Registry:
                        return
                            (this.Account == summary.Account) &&
                            (this.RegistryRights == summary.RegistryRights) &&
                            (this.InheritanceFlags == summary.InheritanceFlags) &&
                            (this.PropagationFlags == summary.PropagationFlags) &&
                            (this.AccessControlType == summary.AccessControlType);
                }
            }
            return false;
        }

        public bool Compare(string accessString)
        {
            if (accessString.Contains("/"))
            {
                return false;
            }

            string[] fields = accessString.Split(';');
            switch (this.Type)
            {
                case PathType.File:
                    if (fields.Length >= 3)
                    {
                        bool ret = true;

                        string account = fields[0];
                        string rights = fields[1];
                        string accessControlType = fields[2];

                        ret &= (account.Contains("\\") && this.Account.Value.Equals(account, StringComparison.OrdinalIgnoreCase)) ||
                            (!account.Contains("\\") && this.Account.Value.EndsWith("\\" + account, StringComparison.OrdinalIgnoreCase));
                        ret &= GetFileSystemRights(rights) == this.FileSystemRights;
                        ret &= GetAccessControlType(accessControlType) != this.AccessControlType;

                        return ret;
                    }
                    break;
                case PathType.Directory:
                    if (fields.Length >= 5)
                    {
                        bool ret = true;

                        string account = fields[0];
                        string rights = fields[1];
                        string inheritanceFlags = fields[2];
                        string propagationFlags = fields[3];
                        string accessControlType = fields[4];

                        ret &= (account.Contains("\\") && this.Account.Value.Equals(account, StringComparison.OrdinalIgnoreCase)) ||
                            (!account.Contains("\\") && this.Account.Value.EndsWith("\\" + account, StringComparison.OrdinalIgnoreCase));
                        ret &= GetFileSystemRights(rights) == this.FileSystemRights;
                        ret &= GetInheritanceFlags(inheritanceFlags) == this.InheritanceFlags;
                        ret &= GetPropagationFlags(propagationFlags) == this.PropagationFlags;
                        ret &= GetAccessControlType(accessControlType) == this.AccessControlType;

                        return ret;
                    }
                    break;
                case PathType.Registry:
                    if (fields.Length >= 5)
                    {
                        bool ret = true;

                        string account = fields[0];
                        string rights = fields[1];
                        string inheritanceFlags = fields[2];
                        string propagationFlags = fields[3];
                        string accessControlType = fields[4];

                        ret &= (account.Contains("\\") && this.Account.Value.Equals(account, StringComparison.OrdinalIgnoreCase)) ||
                            (!account.Contains("\\") && this.Account.Value.EndsWith("\\" + account, StringComparison.OrdinalIgnoreCase));
                        ret &= GetRegistryRights(rights) == this.RegistryRights;
                        ret &= GetInheritanceFlags(inheritanceFlags) == this.InheritanceFlags;
                        ret &= GetPropagationFlags(propagationFlags) == this.PropagationFlags;
                        ret &= GetAccessControlType(accessControlType) == this.AccessControlType;

                        return ret;
                    }
                    break;
            }

            return false;
        }

        public bool Compare(AuthorizationRule rule)
        {
            switch (this.Type)
            {
                case PathType.File:
                    if (rule is FileSystemAccessRule frule)
                    {
                        bool ret = true;

                        ret &= this.Account == (NTAccount)frule.IdentityReference;
                        if (frule.FileSystemRights == System.Security.AccessControl.FileSystemRights.FullControl)
                        {
                            ret &= this.FileSystemRights == System.Security.AccessControl.FileSystemRights.FullControl;
                        }
                        else
                        {
                            ret &= this.FileSystemRights == (frule.FileSystemRights & (~System.Security.AccessControl.FileSystemRights.Synchronize));
                        }
                        ret &= this.AccessControlType == frule.AccessControlType;

                        return ret;
                    }
                    break;
                case PathType.Directory:
                    if (rule is FileSystemAccessRule drule)
                    {
                        bool ret = true;

                        ret &= this.Account == (NTAccount)drule.IdentityReference;
                        if (drule.FileSystemRights == System.Security.AccessControl.FileSystemRights.FullControl)
                        {
                            ret &= this.FileSystemRights == System.Security.AccessControl.FileSystemRights.FullControl;
                        }
                        else
                        {
                            ret &= this.FileSystemRights == (drule.FileSystemRights & (~System.Security.AccessControl.FileSystemRights.Synchronize));
                        }
                        ret &= this.InheritanceFlags == drule.InheritanceFlags;
                        ret &= this.PropagationFlags == drule.PropagationFlags;
                        ret &= this.AccessControlType == drule.AccessControlType;

                        return ret;
                    }
                    break;
                case PathType.Registry:
                    if (rule is RegistryAccessRule rrule)
                    {
                        bool ret = true;

                        ret &= this.Account == (NTAccount)rrule.IdentityReference;
                        ret &= this.RegistryRights == rrule.RegistryRights;
                        ret &= this.InheritanceFlags == rrule.InheritanceFlags;
                        ret &= this.PropagationFlags == rrule.PropagationFlags;
                        ret &= this.AccessControlType == rrule.AccessControlType;

                        return ret;
                    }
                    break;
            }
            return false;
        }

        #endregion
        #region Object to Object

        public static string FileToAccessString(string path)
        {
            if (File.Exists(path))
            {
                var rules = new FileInfo(path).GetAccessControl().GetAccessRules(true, false, typeof(NTAccount));
                return string.Join("/", FromAccessRules(rules, PathType.File).Select(x => x.ToString()));
            }
            return null;
        }

        public static string DirectoryToAccessString(string path)
        {
            if (Directory.Exists(path))
            {
                var rules = new DirectoryInfo(path).GetAccessControl().GetAccessRules(true, false, typeof(NTAccount));
                return string.Join("/", FromAccessRules(rules, PathType.Directory).Select(x => x.ToString()));
            }
            return null;
        }

        public static string RegistryKeyToAccessString(string path)
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(path, false, false))
            {
                return RegistryKeyToAccessString(regKey);
            }
        }

        public static string RegistryKeyToAccessString(RegistryKey regKey)
        {
            if (regKey != null)
            {
                var rules = regKey.GetAccessControl().GetAccessRules(true, false, typeof(NTAccount));
                return string.Join("/", FromAccessRules(rules, PathType.Registry).Select(x => x.ToString()));
            }
            return null;
        }

        #endregion
    }
}
