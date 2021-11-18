using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using Microsoft.Win32;
using IO.Lib;
using System.Security.AccessControl;
using System.Security.Principal;

namespace IO.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Get : IOTaskWorkRegistry
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string[] _Path { get; set; }

        [TaskParameter(ResolvEnv = true, Delimiter = '\n')]
        [Keys("name", "name", "namae", "registryname", "regname", "paramname")]
        protected string[] _Name { get; set; }

        [TaskParameter]
        [Keys("tolog", "log")]
        protected bool _ToLog { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            if (_Name?.Length > 0)
            {
                //  レジストリ値の情報を取得。キーを複数指定していた場合、2つ目以降を無視
                TargetRegistryValueProcess(_Path[0], _Name, false, GetRegistryValueAction);
            }
            else
            {
                //  レジストリキーの情報を取得
                TargetRegistryKeyProcess(_Path, writable: false, GetRegistryKeyAction);
            }
        }

        private void GetRegistryValueAction(RegistryKey targetKey, string targetName)
        {
            if (targetKey.GetValueNames().Any(x => x.Equals(targetName, StringComparison.OrdinalIgnoreCase)))
            {
                string registryKey = targetKey.Name;
                string registryParameterName = targetName;

                RegistryValueKind valueKind = targetKey.GetValueKind(targetName);
                string registryValue = RegistryControl.RegistryValueToString(targetKey, targetName, valueKind, false);
                string registryValueNotExpand = RegistryControl.RegistryValueToString(targetKey, targetName, valueKind, true);
                string registryValueKind = RegistryControl.ValueKindToString(valueKind);

                var sb = new StringBuilder();
                sb.AppendLine($"{this.TaskName} Registry parametenr name summary");
                sb.AppendLine($"  RegistryKey            : {registryKey}");
                sb.AppendLine($"  RegistryParameterName  : {registryParameterName}");
                sb.AppendLine($"  RegistryValue          : {registryValue}");
                sb.AppendLine($"  RegistryValueNotExpand : {registryValueNotExpand}");
                sb.Append($"  RegistryValueKind      : {registryValueKind}");

                if (_ToLog)
                {
                    Manager.WriteLog(LogLevel.Info, sb.ToString());
                }
                else
                {
                    Manager.WriteStandard(sb.ToString());
                }
            }
            else
            {
                Manager.WriteLog(LogLevel.Error, "Target parameter name is Missing. \"{0}\"", targetName);
                return;
            }
        }

        private void GetRegistryKeyAction(RegistryKey target)
        {
            RegistrySecurity security = target.GetAccessControl();
            AuthorizationRuleCollection rules = security.GetAccessRules(true, false, typeof(NTAccount));

            string registryKeyName = System.IO.Path.GetFileName(target.Name);
            string registryKeyPath = target.Name;

            //string access = RegistryControl.AccessRulesToString(rules);
            string access = string.Join('/',
                AccessRuleSummary.FromAccessRules(rules, AccessRuleSummary.TargetType.Registry).
                    Select(x => x.ToString()));

            string owner = security.GetOwner(typeof(NTAccount)).Value;
            string inherited = (!security.AreAccessRulesProtected).ToString();

            var sb = new StringBuilder();
            sb.AppendLine($"{this.TaskName} Registry key summary");
            sb.AppendLine($"  registryKeyName        : {registryKeyName}");
            sb.AppendLine($"  RegistryKeyPath        : {registryKeyPath}");
            sb.AppendLine($"  Access                 : {access}");
            sb.AppendLine($"  Owner                  : {owner}");
            sb.Append($"  Inherited              : {inherited}");

            if (_ToLog)
            {
                Manager.WriteLog(LogLevel.Info, sb.ToString());
            }
            else
            {
                Manager.WriteStandard(sb.ToString());
            }
        }
    }
}
