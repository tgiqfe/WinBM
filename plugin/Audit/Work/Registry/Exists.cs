using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Microsoft.Win32;
using IO.Lib;

namespace Audit.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Exists : AuditTaskWork
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string[] _Path { get; set; }

        [TaskParameter(ResolvEnv = true, Delimiter = '\n')]
        [Keys("name", "name", "namae", "registryname", "regname", "paramname")]
        protected string[] _Name { get; set; }

        [TaskParameter]
        [Keys("valuekind", "kind", "type", "registrytype", "regtype")]
        [Values("unknown,reg_unknown",
            "string,reg_sz",
            "dword,reg_dword,int",
            "qword,reg_qword,long",
            "multistring,reg_multi_sz,strings,stringarray,array",
            "expandstring,reg_expand_sz,expand_string",
            "binary,reg_binary,bin",
            "none,reg_none,non,no",
            Default = "string")]
        [ValidateEnumSet("Unknown", "String", "DWord", "QWord", "MultiString", "ExpandString", "Binary", "None")]
        protected RegistryValueKind? _ValueKind { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            this.Success = true;
            var dictionary = new Dictionary<string, string>();

            //  レジストリタイプ未指定の場合にREG_SZのセットは行わない。

            if (_Name?.Length > 0)
            {
                //  名前指定有りの場合は、同キー配下の複数の名前をチェック
                ExistsRegistryValueCheck(_Path[0], dictionary);
            }
            else
            {
                //  名前指定なしの為、複数キーのチェック
                ExistsRegistryKeyCheck(_Path, dictionary);
            }

            AddAudit(dictionary, this._Invert);
        }

        private void ExistsRegistryValueCheck(string target, Dictionary<string, string> dictionary)
        {
            using (var regKey = RegistryControl.GetRegistryKey(target, false, false))
            {
                //  レジストリキーのチェック
                if (regKey == null)
                {
                    dictionary[$"registryKey_NotExists"] = target;
                    this.Success = false;
                    return;
                }
                dictionary[$"registryKey_Exists"] = target;

                //  レジストリ名のチェック
                if (_Name?.Length > 0)
                {
                    int count = 0;
                    foreach (string name in _Name)
                    {
                        count++;
                        if (!regKey.GetValueNames().Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)))
                        {
                            dictionary[$"registryName_{count}_NotExists"] = name;
                            this.Success = false;
                            continue;
                        }
                        dictionary[$"registryName_{count}_Exists"] = name;

                        //  レジストリタイプのチェック
                        if (_ValueKind != null)
                        {
                            RegistryValueKind valueKind = regKey.GetValueKind(name);
                            if (regKey.GetValueKind(name) != _ValueKind)
                            {
                                dictionary[$"registryType_{count}_NotMatch"] = RegistryControl.ValueKindToString(valueKind);
                                this.Success = false;
                                continue;
                            }
                            dictionary[$"registryType_{count}_Match"] = RegistryControl.ValueKindToString(valueKind);
                        }
                    }
                }
                else if (_Name == null && _ValueKind != null)
                {
                    //  レジストリ名が無指定で、レジストリタイプをしている場合
                    if (regKey.GetValueKind("") != _ValueKind)
                    {
                        dictionary[$"registryType_NotMatch"] = _ValueKind.ToString();
                        this.Success = false;
                        return;
                    }
                    dictionary[$"registryType_Match"] = _ValueKind.ToString();
                }
            }
        }

        private void ExistsRegistryKeyCheck(string[] targets, Dictionary<string, string> dictionary)
        {
            int count = 0;
            foreach (string target in targets)
            {
                count++;
                using (var regKey = RegistryControl.GetRegistryKey(target, false, false))
                {
                    //  レジストリキーのチェック
                    if (regKey == null)
                    {
                        dictionary[$"registryKey_{count}_NotExists"] = target;
                        this.Success = false;
                        return;
                    }
                    dictionary[$"registryKey_{count}_Exists"] = target;
                }
            }
        }
    }
}
