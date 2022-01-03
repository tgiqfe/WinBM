using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using IO.Lib;

namespace Audit.Work.Directory
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Security : AuditTaskWork
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        [TaskParameter(MandatoryAny = 1, Resolv = true, Delimiter = '/')]
        [Keys("access", "acess", "acces", "aces")]
        protected string[] _Access { get; set; }

        [TaskParameter(MandatoryAny = 2, Resolv = true)]
        [Keys("account", "acount")]
        protected string _Account { get; set; }

        [TaskParameter(MandatoryAny = 2)]
        [Keys("rights", "right")]
        [Values("readandexecute,readandexec,readadnexe,readexec,readexe,readonly,read", "modify,mod", "fullcontrol,full",
            Default = "readandexecute")]
        protected FileSystemRights? _Rights { get; set; }

        [TaskParameter]
        [Keys("accesscontrol", "acccontrol", "accctrl", "accessctrl", "acl")]
        [Values("allow,arrow", "deny,denied")]
        protected AccessControlType _AccessControl { get; set; }

        [TaskParameter(MandatoryAny = 3)]
        [Keys("owner", "own")]
        protected string _Owner { get; set; }

        [TaskParameter(MandatoryAny = 4)]
        [Keys("inherited", "inherit", "inheritance")]
        protected bool? _Inherited { get; set; }

        //  ########################

        [TaskParameter]
        [Keys("norecursive", "norec", "norecurs", "norecurse")]
        protected bool _NoRecurse { get; set; }

        [TaskParameter]
        [Keys("accessallmatch", "aclallmatch", "aclall")]
        protected bool? _AccessAllMatch { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        private AccessRuleSummary[] _accessRuleSummary = null;
        private UserAccount _ownerAccount = null;

        public override void MainProcess()
        {
            var dictionary = new Dictionary<string, string>();
            this.Success = true;
            int count = 0;

            if (_Access?.Length > 0)
            {
                _accessRuleSummary = AccessRuleSummary.FromAccessString(string.Join("/", _Access), PathType.Directory);
            }
            if ((_accessRuleSummary == null || _accessRuleSummary.Length == 0) && !string.IsNullOrEmpty(_Account))
            {
                _Account = PredefinedAccount.Resolv(_Account);
                _accessRuleSummary = AccessRuleSummary.FromAccessString(
                    string.Format("{0};{1};{2};{3};{4}",
                        _Account,
                        _Rights,
                        _NoRecurse ? "None" : "ContainerInherit,ObjectInherit",
                        "None",
                        _AccessControl),
                    PathType.Directory);
            }

            if (_accessRuleSummary?.Length > 0)
            {
                dictionary["Check_Access"] =
                    string.Join("/", _accessRuleSummary.Select(x => x.ToString()));
            }
            if (!string.IsNullOrEmpty(_Owner))
            {
                _ownerAccount = new UserAccount(_Owner);
                dictionary["Check_Owner"] = _ownerAccount.ToString();
            }
            if (_Inherited != null)
            {
                dictionary["Check_Inherited"] = _Inherited.ToString();
            }

            foreach (string path in _Path)
            {
                if (Path.GetFileName(path).Contains("*"))
                {
                    Manager.WriteLog(LogLevel.Info, "{0} Wildcard Copy.", this.TaskName);

                    //  対象ファイルの親フォルダーが存在しない場合
                    string parent = Path.GetDirectoryName(path);
                    if (!System.IO.Directory.Exists(parent))
                    {
                        Manager.WriteLog(LogLevel.Warn, "Parent on target is Missing. \"{0}\"", parent);
                        return;
                    }

                    //  ワイルドカード指定
                    System.Text.RegularExpressions.Regex wildcard = Wildcard.GetPattern(path);
                    System.IO.Directory.GetDirectories(parent).
                        Where(x => wildcard.IsMatch(x)).
                        ToList().
                        ForEach(x => SecurityDirectoryCheck(x, dictionary, ++count));
                }
                else
                {
                    //  対象ファイルが存在しない場合
                    if (!System.IO.Directory.Exists(path))
                    {
                        Manager.WriteLog(LogLevel.Warn, "Target is Missing. \"{0}\"", path);
                        return;
                    }

                    SecurityDirectoryCheck(path, dictionary, ++count);
                }
            }

            AddAudit(dictionary, this._Invert);
        }

        private void SecurityDirectoryCheck(string target, Dictionary<string, string> dictionary, int count)
        {
            try
            {
                dictionary[$"directory_{count}"] = target;

                DirectorySecurity security = new System.IO.DirectoryInfo(target).GetAccessControl();

                if (_accessRuleSummary?.Length > 0)
                {
                    AuthorizationRuleCollection rules = security.GetAccessRules(true, false, typeof(NTAccount));
                    string targetAccess =
                        string.Join("/", AccessRuleSummary.FromAccessRules(rules, PathType.Directory).Select(x => x.ToString()));
                    if (rules.Count == _accessRuleSummary.Length &&
                        rules.OfType<AuthorizationRule>().All(x => _accessRuleSummary.Any(y => y.Compare(x))))
                    {
                        dictionary[$"directory_{count}_Access_Match"] = targetAccess;
                    }
                    else
                    {
                        dictionary[$"directory_{count}_Access_NotMatch"] = targetAccess;
                        this.Success = false;
                    }
                }

                if (!string.IsNullOrEmpty(_Owner))
                {
                    //  必要に応じて再帰的に所有者チェック。
                    //  不一致を確認した時点で再起チェックは終了。
                    //  不一致の場合は所有者名を返し、一致の場合はnullを返す。
                    Func<string, DirectorySecurity, string> recurseCheck = null;
                    recurseCheck = (targetPath, targetSecurity) =>
                    {
                        string owner = targetSecurity.GetOwner(typeof(NTAccount)).Value;
                        if (_ownerAccount.IsMatch(owner))
                        {
                            if (!_NoRecurse)
                            {
                                foreach (string child in System.IO.Directory.GetFiles(targetPath, "*", SearchOption.AllDirectories))
                                {
                                    var childSecurity = new System.IO.FileInfo(child).GetAccessControl();
                                    string childOwner = childSecurity.GetOwner(typeof(NTAccount)).Value;
                                    if (!_ownerAccount.IsMatch(childOwner))
                                    {
                                        return childOwner;
                                    }
                                }
                                foreach (string child in System.IO.Directory.GetDirectories(targetPath, "*", SearchOption.AllDirectories))
                                {
                                    var childSecurity = new System.IO.DirectoryInfo(child).GetAccessControl();
                                    string childOwner = childSecurity.GetOwner(typeof(NTAccount)).Value;
                                    if (!_ownerAccount.IsMatch(childOwner))
                                    {
                                        return childOwner;
                                    }
                                }
                            }
                            return null;
                        }
                        return owner;
                    };

                    string targetOwner = recurseCheck(target, security);
                    if (targetOwner == null)
                    {
                        dictionary[$"directory_{count}_Owner_Match"] = _ownerAccount.ToString();
                    }
                    else
                    {
                        dictionary[$"directory_{count}_Owner_NotMatch"] = targetOwner;
                        this.Success = false;
                    }
                }

                if (_Inherited != null)
                {
                    bool targetInherited = !security.AreAccessRulesProtected;
                    if (targetInherited == _Inherited)
                    {
                        dictionary[$"directory_{count}_Inherited_Match"] = targetInherited.ToString();
                    }
                    else
                    {
                        dictionary[$"directory_{count}_Inherited_NotMatch"] = targetInherited.ToString();
                        this.Success = false;
                    }
                }
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
                this.Success = false;
            }
        }
    }
}
