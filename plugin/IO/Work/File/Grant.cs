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

namespace IO.Work.File
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Grant : IOTaskWork
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
        [ValidateEnumSet("ReadAndExecute", "Modify", "FullControl")]
        protected FileSystemRights? _Rights { get; set; }

        [TaskParameter]
        [Keys("accesscontrol", "acccontrol", "accctrl", "accessctrl", "acl", "accesscontroltype", "acltype")]
        [Values("allow,arrow", "deny,denied")]
        protected AccessControlType _AccessControl { get; set; }

        [TaskParameter(MandatoryAny = 3)]
        [Keys("inherited", "inherit", "inheritance")]
        [Values("none,non,no", "enable,enabled,en", "disable,disabled,dis", "remove,rm")]
        protected InheritedAction? _Inherited { get; set; }

        private AccessRuleSummary[] _accessRuleSummary = null;

        public override void MainProcess()
        {
            this.Success = true;

            if (_Access?.Length > 0)
            {
                _accessRuleSummary = AccessRuleSummary.FromAccessString(
                    string.Join("/", _Access),
                    PathType.File);
            }
            if ((_accessRuleSummary == null || _accessRuleSummary.Length == 0) && !string.IsNullOrEmpty(_Account))
            {
                _Account = PredefinedAccount.Resolv(_Account);
                _accessRuleSummary = AccessRuleSummary.FromAccessString(
                    $"{_Account};{_Rights};{_AccessControl}",
                    PathType.File);
            }

            TargetFileProcess(_Path, GrantFileAction);
        }

        private void GrantFileAction(string target)
        {
            System.IO.FileInfo fInfo = new System.IO.FileInfo(target);

            try
            {
                bool isChange = false;

                FileSecurity security = fInfo.GetAccessControl();

                if (_accessRuleSummary?.Length > 0)
                {
                    _accessRuleSummary.ToList().ForEach(x =>
                    {
                        isChange = true;
                        security.AddAccessRule((FileSystemAccessRule)x.ToAccessRule());
                    });
                }

                switch (_Inherited)
                {
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
                    case InheritedAction.None:
                    default:
                        break;
                }

                if (isChange)
                {
                    fInfo.SetAccessControl(security);
                }
                else
                {
                    Manager.WriteLog(LogLevel.Info, "Grant not change: \"{0}\"", target);
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
