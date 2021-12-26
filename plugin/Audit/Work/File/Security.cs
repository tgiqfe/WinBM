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
        [Keys("accessallmatch", "aclallmatch", "aclall")]
        protected bool? _AccessAllMatch { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        private AccessRuleSummary[] _accessRuleSummary = null;

        public override void MainProcess()
        {
            var dictionary = new Dictionary<string, string>();
            this.Success = true;
            int count = 0;

            if (_Access?.Length > 0)
            {
                _accessRuleSummary = AccessRuleSummary.FromAccessString(string.Join("/", _Access), PathType.File);
            }
            if ((_accessRuleSummary == null || _accessRuleSummary.Length == 0) && !string.IsNullOrEmpty(_Account))
            {
                _Account = PredefinedAccount.Resolv(_Account);
                _accessRuleSummary = AccessRuleSummary.FromAccessString($"{_Account};{_Rights};{_AccessControl}", PathType.File);
            }

            /*
            //  アクセス文字列を準備。Access文字列指定/Account+Right指定の場合のみ準備。Inherited指定のみの場合は対象外
            if (string.IsNullOrEmpty(_Access) && !string.IsNullOrEmpty(_Account))
            {
                _Account = PredefinedAccount.Resolv(_Account);
                _Access = $"{_Account};{_Rights};{_AccessControl}";
            }
            */

            if (_accessRuleSummary?.Length > 0)
            {
                dictionary["Check_Access"] =
                    string.Join("/", _accessRuleSummary.Select(x => x.ToString()));
            }

            /*
            if (!string.IsNullOrEmpty(_Access))
            {
                dictionary["Check_Access"] = String.Join(";", _Access);
            }
            */

            if (!string.IsNullOrEmpty(_Owner))
            {
                //  ついでに事前定義アカウントチェック
                _Owner = PredefinedAccount.Resolv(_Owner);
                dictionary["Check_Owner"] = _Owner;
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
                    string targetParent = Path.GetDirectoryName(path);
                    if (!System.IO.Directory.Exists(targetParent))
                    {
                        Manager.WriteLog(LogLevel.Error, "Parent folder on source file is Missing. \"{0}\"", targetParent);
                        return;
                    }

                    //  ワイルドカード指定
                    System.Text.RegularExpressions.Regex wildcard = Wildcard.GetPattern(path);
                    System.IO.Directory.GetFiles(targetParent).
                        Where(x => wildcard.IsMatch(x)).
                        ToList().
                        ForEach(x => SecurityFileCheck(x, dictionary, ++count));
                }
                else
                {
                    //  対象ファイルが存在しない場合
                    if (!System.IO.File.Exists(path))
                    {
                        Manager.WriteLog(LogLevel.Error, "Target file is Missing. \"{0}\"", path);
                        return;
                    }

                    SecurityFileCheck(path, dictionary, ++count);
                }
            }

            AddAudit(dictionary, this._Invert);
        }

        private void SecurityFileCheck(string target, Dictionary<string, string> dictionary, int count)
        {
            try
            {
                dictionary[$"file_{count}"] = target;

                FileSecurity security = new System.IO.FileInfo(target).GetAccessControl();

                if (_accessRuleSummary?.Length > 0)
                {
                    AuthorizationRuleCollection rules = security.GetAccessRules(true, false, typeof(NTAccount));
                    string targetAccess =
                        string.Join("/", AccessRuleSummary.FromAccessRules(rules, PathType.File).Select(x => x.ToString()));
                    if (rules.Count == _accessRuleSummary.Length &&
                        rules.OfType<AuthorizationRule>().All(x => _accessRuleSummary.Any(y => y.Compare(x))))
                    {
                        dictionary[$"file_{count}_Access_Match"] = targetAccess;
                    }
                    else
                    {
                        dictionary[$"file_{count}_Access_NoMatch"] = targetAccess;
                        this.Success = false;
                    }
                }

                /*
                if (!string.IsNullOrEmpty(_Access))
                {
                    AuthorizationRuleCollection rule = security.GetAccessRules(true, false, typeof(NTAccount));
                    string targetAccess = FileControl.AccessRulesToString(rule);
                    if (targetAccess.Equals(_Access, StringComparison.OrdinalIgnoreCase))
                    {
                        dictionary[$"file_{count}_Access_Match"] = targetAccess;
                    }
                    else
                    {
                        dictionary[$"file_{count}_Access_NoMatch"] = targetAccess;
                        this.Success = false;
                    }
                }
                */

                if (!string.IsNullOrEmpty(_Owner))
                {
                    string targetOwner = security.GetOwner(typeof(NTAccount)).Value;
                    if (targetOwner.Equals(_Owner, StringComparison.OrdinalIgnoreCase))
                    {
                        dictionary[$"file_{count}_Owner_Match"] = targetOwner;
                    }
                    else
                    {
                        dictionary[$"file_{count}_Owner_NoMatch"] = targetOwner;
                        this.Success = false;
                    }
                }

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
