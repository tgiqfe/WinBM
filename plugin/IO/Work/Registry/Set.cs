using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using Microsoft.Win32;
using IO.Lib;

namespace IO.Work.Registry
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Set : TaskJob
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true)]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string _Path { get; set; }

        [TaskParameter(ResolvEnv = true)]
        [Keys("name", "name", "namae", "registryname", "regname", "paramname")]
        protected string _Name { get; set; }

        [TaskParameter(ResolvEnv = true)]
        [Keys("value", "val", "valu", "registryvalue", "regvalue")]
        protected string _Value { get; set; }

        [TaskParameter(ResolvEnv = true, Delimiter = '\n')]
        [Keys("valuemulti", "valmulti", "multi", "valuearray", "valarray", "array")]
        protected string[] _ValueMulti { get; set; }

        [TaskParameter]
        [Keys("valueexpand", "valexpand", "valuenotresolvenv", "valnotresolvenv", "valuenotresolv", "valnotresolv")]
        protected string _ValueExpand { get; set; }

        [TaskParameter]
        [Keys("valuekind", "kind", "type", "regtype")]
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

        public override void MainProcess()
        {
            this.Success = true;

            //  レジストリタイプ未指定の場合、REG_SZをセット
            _ValueKind ??= RegistryValueKind.String;

            //  複数行文字列格納用パラメータ。環境変数解決をしないオプションは無し。もし必要ならば追加
            if (_ValueKind == RegistryValueKind.MultiString)
            {
                if (_ValueMulti == null && !string.IsNullOrEmpty(_Value))
                {
                    _ValueMulti = System.Text.RegularExpressions.Regex.Split(_Value, "\\\\0");
                }
            }

            //  展開文字列格納用パラメータ
            if (_ValueKind == RegistryValueKind.ExpandString)
            {
                if (string.IsNullOrEmpty(_ValueExpand) && !string.IsNullOrEmpty(_Value))
                {
                    _ValueExpand = _Value;
                }
            }

            //  数値指定時の修正 (マイナス指定の場合はSuccess=false, 16進数設定に対応)
            if (_ValueKind == RegistryValueKind.DWord)
            {
                if (int.TryParse(_Value, out int tempInt) && tempInt < 0)
                {
                    Manager.WriteLog(LogLevel.Error, "Failed value, because minus value.");
                    this.Success = false;
                    return;
                }
                else if (_Value.StartsWith("0x"))
                {
                    _Value = Convert.ToInt32(_Value.Substring(2), 16).ToString();
                }
            }
            if (_ValueKind == RegistryValueKind.QWord)
            {
                if (long.TryParse(_Value, out long tempLong) && tempLong < 0)
                {
                    Manager.WriteLog(LogLevel.Error, "Failed value, because minus value.");
                    this.Success = false;
                    return;
                }
                else if (_Value.StartsWith("0x"))
                {
                    _Value = Convert.ToInt64(_Value.Substring(2), 16).ToString();
                }
            }

            SetRegistryAction(_Path);
        }

        private void SetRegistryAction(string target)
        {
            //  ワイルドカード指定は対応しない方針
            if (target.Contains("*"))
            {
                Manager.WriteLog(LogLevel.Warn, "The path contains a wildcard.");
                return;
            }

            try
            {
                switch (_ValueKind)
                {
                    case RegistryValueKind.String:
                        Microsoft.Win32.Registry.SetValue(target, _Name, _Value ?? "", RegistryValueKind.String);
                        break;
                    case RegistryValueKind.Binary:
                        Microsoft.Win32.Registry.SetValue(target, _Name, RegistryControl.StringToRegBinary(_Value), RegistryValueKind.Binary);
                        break;
                    case RegistryValueKind.DWord:
                        Microsoft.Win32.Registry.SetValue(target, _Name, int.Parse(_Value), RegistryValueKind.DWord);
                        break;
                    case RegistryValueKind.QWord:
                        Microsoft.Win32.Registry.SetValue(target, _Name, long.Parse(_Value), RegistryValueKind.QWord);
                        break;
                    case RegistryValueKind.MultiString:
                        Microsoft.Win32.Registry.SetValue(target, _Name, _ValueMulti, RegistryValueKind.MultiString);
                        break;
                    case RegistryValueKind.ExpandString:
                        Microsoft.Win32.Registry.SetValue(target, _Name, _ValueExpand, RegistryValueKind.ExpandString);
                        break;
                    case RegistryValueKind.None:
                        Microsoft.Win32.Registry.SetValue(target, _Name, new byte[2] { 0, 0 }, RegistryValueKind.None);
                        break;
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

    internal class Add : Set
    {
        protected override bool IsAlias { get { return true; } }
    }
}
