using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.Diagnostics;
using System.Security.Principal;
using System.Security.AccessControl;
using IO.Lib;
using Microsoft.Win32;

namespace IO.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Grant : IOTaskWorkRegistry
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string[] _Path { get; set; }

        [TaskParameter(MandatoryAny = 1, ResolvEnv = true, Delimiter = '/')]
        [Keys("access", "acess", "acces", "aces")]
        protected string[] _Access { get; set; }

        [TaskParameter(MandatoryAny = 2, ResolvEnv = true)]
        [Keys("account", "acount")]
        protected string _Account { get; set; }

        [TaskParameter(MandatoryAny = 2)]
        [Keys("rights", "right")]
        [Values("readkey,readandexecute,readandexec,readadnexe,readexec,readexe,readonly,read", "fullcontrol,full",
            Default = "readkey")]
        [ValidateEnumSet("ReadKey", "FullControl")]
        protected RegistryRights? _Rights { get; set; }

        [TaskParameter]
        [Keys("accesscontrol", "acccontrol", "accctrl", "accessctrl", "acl", "accesscontroltype", "acltype")]
        [Values("allow,arrow", "deny,denied")]
        protected AccessControlType _AccessControl { get; set; }

        [TaskParameter(MandatoryAny = 3)]
        [Keys("inherited", "inherit", "inheritance")]
        [Values("none,non,no", "enable,enabled,en", "disable,disabled,dis", "remove,rm")]
        protected InheritedAction? _Inherited { get; set; }

        [TaskParameter]
        [Keys("norecurse", "norecursive", "norec", "norecurs")]
        protected bool _NoRecurse { get; set; }

        private AccessRuleSummary[] _accessRuleSummary = null;

        public override void MainProcess()
        {
            this.Success = true;

            if (_Access?.Length > 0)
            {
                _accessRuleSummary = AccessRuleSummary.FromAccessString(
                    string.Join("/", _Access),
                    PathType.Registry);
            }
            if ((_accessRuleSummary == null || _accessRuleSummary.Length == 0) && !string.IsNullOrEmpty(_Account))
            {
                _Account = PredefinedAccount.Resolv(_Account);
                _accessRuleSummary = AccessRuleSummary.FromAccessString(
                    string.Format("{0};{1};{2};{3};{4}",
                        _Account,
                        _Rights,
                        _NoRecurse ? "None" : "ContainerInherit",
                        "None",
                        _AccessControl),
                    PathType.Registry);
            }

            TargetRegistryKeyProcess(_Path, writable: true, GrantRegistryAction);
        }

        private void GrantRegistryAction(RegistryKey target)
        {
            try
            {
                bool isChange = false;

                RegistrySecurity security = target.GetAccessControl();

                if (_accessRuleSummary?.Length > 0)
                {
                    _accessRuleSummary.ToList().ForEach(x =>
                    {
                        isChange = true;
                        security.AddAccessRule((RegistryAccessRule)x.ToAccessRule());
                    });
                }

                switch (_Inherited)
                {
                    case InheritedAction.None:
                        break;
                    case InheritedAction.Enable:
                        isChange = true;
                        security.SetAccessRuleProtection(false, false);
                        break;
                    case InheritedAction.Disable:
                        isChange = true;
                        security.SetAccessRuleProtection(true, true);
                        break;
                    case InheritedAction.Remove:
                        isChange = true;
                        security.SetAccessRuleProtection(true, false);
                        break;
                }

                if (isChange)
                {
                    target.SetAccessControl(security);
                }
                else
                {
                    Manager.WriteLog(LogLevel.Info, "Grant not change: \"{0}\"", target.Name);
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

    /// <summary>
    /// Grantへのエイリアス
    /// </summary>
    internal class Access : Grant
    {
        protected override bool IsAlias { get { return true; } }
    }
}
