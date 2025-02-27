﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Security.Principal;

namespace IO.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Revoke : WorkRegistry
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string[] _Path { get; set; }

        [TaskParameter(MandatoryAny = 1, Resolv = true, Delimiter = ';')]
        [Keys("account", "acount", "owner", "own")]
        protected string[] _Account { get; set; }

        [TaskParameter(MandatoryAny = 2)]
        [Keys("all", "any")]
        protected bool _All { get; set; }

        public override void MainProcess()
        {
            this.Success = true;

            //  ドメインアカウント/ローカルアカウントの両方に対応させる為、事前定義アカウントチェックは無し。

            TargetKeySequence(_Path, writable: true, RevokeRegistryAction);
        }

        private void RevokeRegistryAction(RegistryKey target)
        {
            try
            {
                bool isChange = false;
                RegistrySecurity security = target.GetAccessControl();
                if (_All)
                {
                    foreach (RegistryAccessRule rule in security.GetAccessRules(true, false, typeof(NTAccount)))
                    {
                        security.RemoveAccessRule(rule);
                        isChange = true;
                    }
                }
                else
                {
                    foreach (RegistryAccessRule rule in security.GetAccessRules(true, false, typeof(NTAccount)))
                    {
                        string targetAccount = rule.IdentityReference.Value;

                        if (_Account.Any(x =>
                            x.Contains("\\") && targetAccount.Equals(x, StringComparison.OrdinalIgnoreCase) ||
                            !x.Contains("\\") && targetAccount.EndsWith("\\" + x, StringComparison.OrdinalIgnoreCase)))
                        {
                            security.RemoveAccessRule(rule);
                            isChange = true;
                        }
                    }
                }

                if (isChange)
                {
                    target.SetAccessControl(security);
                }
                else
                {
                    Manager.WriteLog(LogLevel.Info, "Revoke not change. \"{0}\"", target.Name);
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
