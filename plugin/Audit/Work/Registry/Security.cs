﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Microsoft.Win32;
using System.Security.Principal;
using System.Security.AccessControl;
using System.IO;
using IO.Lib;

namespace Audit.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Security : WorkRegistry
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string[] _Path { get; set; }

        [TaskParameter(MandatoryAny = 1, Resolv = true, Delimiter = '/')]
        [Keys("access", "acess", "acces", "aces")]
        protected string[] _Access { get; set; }

        [TaskParameter(MandatoryAny = 2, Resolv = true)]
        [Keys("account", "acount")]
        protected string _Account { get; set; }

        [TaskParameter(MandatoryAny = 2)]
        [Keys("rights", "right")]
        [Values("readkey,readandexecute,readandexec,readadnexe,readexec,readexe,readonly,read", "fullcontrol,full",
            Default = "readkey")]
        [ValidateEnumSet("ReadKey", "FullControl")]
        protected RegistryRights? _Rights { get; set; }

        [TaskParameter]
        [Keys("accesscontrol", "acccontrol", "accctrl", "accessctrl", "acl")]
        [Values("allow,arrow", "deny,denied")]
        protected AccessControlType _AccessControl { get; set; }

        [TaskParameter(MandatoryAny = 3)]
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

        public override void MainProcess()
        {
            var dictionary = new Dictionary<string, string>();
            this.Success = true;

            if (_Access?.Length > 0)
            {
                _accessRuleSummary = AccessRuleSummary.FromAccessString(string.Join("/", _Access), PathType.Registry);
            }
            if ((_accessRuleSummary == null || _accessRuleSummary.Length == 0) && !string.IsNullOrEmpty(_Account))
            {
                var userAccount = new UserAccount(_Account);
                _accessRuleSummary = AccessRuleSummary.FromAccessString(
                    string.Format("{0};{1};{2};{3};{4}",
                        userAccount.FullName,
                        _Rights,
                        _Sealed ? "None" : "ContainerInherit",
                        "None",
                        _AccessControl),
                    PathType.Registry);
            }

            if (_accessRuleSummary?.Length > 0)
            {
                dictionary["Check_Access"] =
                    string.Join("/", _accessRuleSummary.Select(x => x.ToString()));
            }
            if (_Inherited != null)
            {
                dictionary["Check_Inherited"] = _Inherited.ToString();
            }

            TargetKeySequence(_Path, false, dictionary, SecurityRegistryKeyAction);

            AddAudit(dictionary, this._Invert);
        }

        private void SecurityRegistryKeyAction(RegistryKey target, Dictionary<string, string> dictionary, int count)
        {
            try
            {
                dictionary[$"registryKey_{count}"] = target.Name;

                RegistrySecurity security = target.GetAccessControl();

                //  アクセス権チェック
                if (_accessRuleSummary?.Length > 0)
                {
                    AuthorizationRuleCollection rules = security.GetAccessRules(true, false, typeof(NTAccount));
                    string targetAccess =
                        string.Join("/", AccessRuleSummary.FromAccessRules(rules, PathType.Registry).Select(x => x.ToString()));
                    if (rules.Count == _accessRuleSummary.Length &&
                        rules.OfType<AuthorizationRule>().All(x => _accessRuleSummary.Any(y => y.IsMatch(x))))
                    {
                        dictionary[$"registryKey_{count}_Access_Match"] = targetAccess;
                    }
                    else
                    {
                        dictionary[$"registryKey_{count}_Access_NotMatch"] = targetAccess;
                        this.Success = false;
                    }
                }

                //  継承有無チェック
                if (_Inherited != null)
                {
                    bool targetInherited = !security.AreAccessRulesProtected;
                    if (targetInherited == _Inherited)
                    {
                        dictionary[$"registryKey_{count}_Inherited_Match"] = targetInherited.ToString();
                    }
                    else
                    {
                        dictionary[$"registryKey_{count}_Inherited_NotMatch"] = targetInherited.ToString();
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
