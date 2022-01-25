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

namespace Audit.Work.File
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Security : WorkFile
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
            
            if (_Access?.Length > 0)
            {
                _accessRuleSummary = AccessRuleSummary.FromAccessString(string.Join("/", _Access), PathType.File);
            }
            if ((_accessRuleSummary == null || _accessRuleSummary.Length == 0) && !string.IsNullOrEmpty(_Account))
            {
                var userAccount = new UserAccount(_Account);
                _accessRuleSummary = AccessRuleSummary.FromAccessString(
                    $"{userAccount.FullName};{_Rights};{_AccessControl}", PathType.File);
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

            TargetSequence(_Path, dictionary, SecurityFileAction);

            AddAudit(dictionary, this._Invert);
        }

        private void SecurityFileAction(string target, Dictionary<string, string> dictionary, int count)
        {
            try
            {
                dictionary[$"file_{count}"] = target;

                FileSecurity security = new System.IO.FileInfo(target).GetAccessControl();

                //  アクセス権チェック
                if (_accessRuleSummary?.Length > 0)
                {
                    AuthorizationRuleCollection rules = security.GetAccessRules(true, false, typeof(NTAccount));
                    string targetAccess =
                        string.Join("/", AccessRuleSummary.FromAccessRules(rules, PathType.File).Select(x => x.ToString()));
                    if (rules.Count == _accessRuleSummary.Length &&
                        rules.OfType<AuthorizationRule>().All(x => _accessRuleSummary.Any(y => y.IsMatch(x))))
                    {
                        dictionary[$"file_{count}_Access_Match"] = targetAccess;
                    }
                    else
                    {
                        dictionary[$"file_{count}_Access_NoMatch"] = targetAccess;
                        this.Success = false;
                    }
                }

                //  所有者チェック
                if (!string.IsNullOrEmpty(_Owner))
                {
                    string targetOwner = security.GetOwner(typeof(NTAccount)).Value;
                    if (_ownerAccount.IsMatch(targetOwner))
                    {
                        dictionary[$"file_{count}_Owner_Match"] = targetOwner;
                    }
                    else
                    {
                        dictionary[$"file_{count}_Owner_NoMatch"] = targetOwner;
                        this.Success = false;
                    }
                }

                //  継承設定チェック
                if (_Inherited != null)
                {
                    bool targetInherited = !security.AreAccessRulesProtected;
                    if (targetInherited == _Inherited)
                    {
                        dictionary[$"file_{count}_Inherited_Match"] = targetInherited.ToString();
                    }
                    else
                    {
                        dictionary[$"file_{count}_Inherited_NoMatch"] = targetInherited.ToString();
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
