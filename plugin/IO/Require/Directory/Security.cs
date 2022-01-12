using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.Security.Principal;
using System.Security.AccessControl;
using System.IO;
using IO.Lib;

namespace IO.Require.Directory
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Security : RequireDirectory
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
        [Keys("sealed", "seal", "shield")]
        protected bool _Sealed { get; set; }

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
            this.Success = true;

            //  Access情報セット
            if (_Access?.Length > 0)
            {
                _accessRuleSummary = AccessRuleSummary.FromAccessString(string.Join("/", _Access), PathType.Directory);
            }
            if ((_accessRuleSummary == null || _accessRuleSummary.Length == 0) && !string.IsNullOrEmpty(_Account))
            {
                var userAccount = new UserAccount(_Account);
                _accessRuleSummary = AccessRuleSummary.FromAccessString(
                    string.Format("{0};{1};{2};{3};{4}",
                        userAccount.FullName,
                        _Rights,
                        _Sealed ? "None" : "ContainerInherit,ObjectInherit",
                        "None",
                        _AccessControl),
                    PathType.Directory);
            }

            //  Owner情報セット
            _ownerAccount = new UserAccount(_Owner);

            TargetDirectoryProcess(_Path, SecurityDirectoryAction);

            this.Success ^= this._Invert;
        }

        private void SecurityDirectoryAction(string target)
        {
            try
            {
                var security = new System.IO.DirectoryInfo(target).GetAccessControl();

                if (_accessRuleSummary?.Length > 0)
                {
                    AuthorizationRuleCollection rules = security.GetAccessRules(true, false, typeof(NTAccount));
                    string targetAccess =
                        string.Join("/", AccessRuleSummary.FromAccessRules(rules, PathType.Directory).Select(x => x.ToString()));
                    if (rules.Count == _accessRuleSummary.Length &&
                        rules.OfType<AuthorizationRule>().All(x => _accessRuleSummary.Any(y => y.IsMatch(x))))
                    {
                        Manager.WriteLog(LogLevel.Info, "Access match: {0}", targetAccess);
                    }
                    else
                    {
                        Manager.WriteLog(LogLevel.Attention, "Access not match: {0}", targetAccess);
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
                            if (!_Sealed)
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
                        Manager.WriteLog(LogLevel.Info, "Owner match: {0}", _ownerAccount);
                    }
                    else
                    {
                        Manager.WriteLog(LogLevel.Attention, "Owner not match: {0}", targetOwner);
                        this.Success = false;
                    }
                }

                if (_Inherited != null)
                {
                    bool targetInherited = !security.AreAccessRulesProtected;
                    if (targetInherited == _Inherited)
                    {
                        Manager.WriteLog(LogLevel.Info, "Inherited match: {0}", targetInherited);
                    }
                    else
                    {
                        Manager.WriteLog(LogLevel.Attention, "Inherited not match: {0}", targetInherited);
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
