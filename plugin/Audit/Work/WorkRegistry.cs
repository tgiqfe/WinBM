using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using IO.Lib;

namespace Audit.Work
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class WorkRegistry : AuditTaskWork
    {
        protected delegate void TargetRegistryKeyAction(
            RegistryKey key, Dictionary<string, string> dictaionry, int count);

        protected delegate void TargetRegistryValueAction(
            RegistryKey key, string name, Dictionary<string, string> dictaionry, int count);

        protected delegate void SrcDstRegistryKeyAction(
            RegistryKey sourceKey, RegistryKey destinationKey, Dictionary<string, string> dictaionry, int count);

        protected delegate void SrcDstRegistryValueAction(
            RegistryKey sourceKey, RegistryKey destinationKey, string sourceName, string destinationName, Dictionary<string, string> dictaionry, int count);

        /*
         * ここにシーケンシャル用メソッドを実装予定
         */
    }
}
