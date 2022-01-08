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
    /// <summary>
    /// レジストリ値の一致チェック
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Match : AuditTaskWork
    {
        [TaskParameter(Mandatory = true, Resolv = true)]
        [Keys("path", "registrypath", "targetpath", "key", "registrykey", "targetkey", "regkey", "target")]
        protected string[] _Path { get; set; }

        [TaskParameter(Resolv = true)]
        [Keys("name", "name", "namae", "registryname", "regname", "paramname")]
        protected string _Name { get; set; }

        [TaskParameter(Resolv = true)]
        [Keys("value", "val", "valu", "registryvalue", "regvalue")]
        protected string _Value { get; set; }

        [TaskParameter(Resolv = true, Delimiter = ';')]
        [Keys("valuemulti", "valmulti", "multi", "valuearray", "valarray", "array")]
        protected string[] _ValueMulti { get; set; }

        [TaskParameter]
        [Keys("valueexpand", "valexpand", "valuenotresolvenv", "valnotresolvenv", "valuenotresolv", "valnotresolv", "valuenotresolve", "valnotresolve")]
        protected string _ValueExpand { get; set; }

        [TaskParameter]
        [Keys("ignorecase")]
        protected bool _IgnoreCase { get; set; }

        private StringComparison _comparer = StringComparison.Ordinal;

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        public override void MainProcess()
        {
            this.Success = true;
            int count = 0;
            var dictionary = new Dictionary<string, string>();

            if (_Value == null) { _Value = ""; }
            if (_IgnoreCase)
            {
                _comparer = StringComparison.OrdinalIgnoreCase;
            }

            foreach (string path in _Path)
            {
                MatchRegistryAction(path, dictionary, ++count);
            }
            AddAudit(dictionary, this._Invert);
        }

        private void MatchRegistryAction(string target, Dictionary<string, string> dictionary, int count)
        {
            using (var regKey = RegistryControl.GetRegistryKey(target, false, false))
            {
                //  レジストリキーのチェック
                if (regKey == null)
                {
                    dictionary[$"registryKey_{count}_NotExists"] = target;
                    this.Success = false;
                    return;
                }

                //  レジストリ名のチェック
                if (!string.IsNullOrEmpty(_Name) && 
                    !regKey.GetValueNames().Any(x => x.Equals(_Name, StringComparison.OrdinalIgnoreCase)))
                {
                    dictionary[$"registryName_{count}_NotExists"] = _Name;
                    this.Success = false;
                    return;
                }

                RegistryValueKind valueKind = regKey.GetValueKind(_Name);
                string targetValue = "";
                switch (valueKind)
                {
                    case RegistryValueKind.String:
                        targetValue = RegistryControl.RegistryValueToString(regKey, _Name, valueKind, false);
                        if (targetValue.Equals(_Value, _comparer))
                        {
                            dictionary[$"registryValue_{count}_Match"] = targetValue;
                        }
                        else
                        {
                            dictionary[$"registryValue_{count}_NotMatch"] = targetValue;
                            this.Success = false;
                        }
                        break;
                    case RegistryValueKind.Binary:
                        targetValue = RegistryControl.RegistryValueToString(regKey, _Name, valueKind, false);
                        if (targetValue.Equals(_Value, StringComparison.OrdinalIgnoreCase))
                        {
                            dictionary[$"registryValue_{count}_Match"] = targetValue;
                        }
                        else
                        {
                            dictionary[$"registryValue_{count}_NotMatch"] = targetValue;
                            this.Success = false;
                        }
                        break;
                    case RegistryValueKind.DWord:
                        if (_Value.StartsWith("0x"))
                        {
                            _Value = Convert.ToInt32(_Value.Substring(2), 16).ToString();
                        }

                        targetValue = RegistryControl.RegistryValueToString(regKey, _Name, valueKind, false);
                        if (targetValue == _Value)
                        {
                            dictionary[$"registryValue_{count}_Match"] = targetValue;
                        }
                        else
                        {
                            dictionary[$"registryValue_{count}_NotMatch"] = targetValue;
                            this.Success = false;
                        }
                        break;
                    case RegistryValueKind.QWord:
                        if (_Value.StartsWith("0x"))
                        {
                            _Value = Convert.ToInt64(_Value.Substring(2), 16).ToString();
                        }

                        targetValue = RegistryControl.RegistryValueToString(regKey, _Name, valueKind, false);
                        if (targetValue == _Value)
                        {
                            dictionary[$"registryValue_{count}_Match"] = targetValue;
                        }
                        else
                        {
                            dictionary[$"registryValue_{count}_NotMatch"] = targetValue;
                            this.Success = false;
                        }
                        break;
                    case RegistryValueKind.MultiString:
                        targetValue = RegistryControl.RegistryValueToString(regKey, _Name, valueKind, false);
                        if (_ValueMulti != null)
                        {
                            _Value = string.Join("\\0", _ValueMulti);
                        }

                        if (targetValue.Equals(_Value, _comparer))
                        {
                            dictionary[$"registryValue_{count}_Match"] = targetValue;
                        }
                        else
                        {
                            dictionary[$"registryValue_{count}_NotMatch"] = targetValue;
                            this.Success = false;
                        }
                        break;
                    case RegistryValueKind.ExpandString:
                        if (string.IsNullOrEmpty(_ValueExpand))
                        {
                            //  _ValueExpand無指定の場合、環境変数解決しつつ_Valueを参照
                            targetValue = RegistryControl.RegistryValueToString(regKey, _Name, valueKind, false);
                        }
                        else
                        {
                            //  _ValueExpand指定されている場合、環境変数せず参照
                            targetValue = RegistryControl.RegistryValueToString(regKey, _Name, valueKind, true);
                            _Value = _ValueExpand;
                        }

                        if (targetValue.Equals(_Value, _comparer))
                        {
                            dictionary[$"registryValue_{count}_Match"] = targetValue;
                        }
                        else
                        {
                            dictionary[$"registryValue_{count}_NotMatch"] = targetValue;
                            this.Success = false;
                        }
                        break;
                    case RegistryValueKind.None:
                        if (string.IsNullOrEmpty(_Value))
                        {
                            dictionary[$"registryValue_{count}_Match"] = "";
                        }
                        else
                        {
                            dictionary[$"registryValue_{count}_NotMatch"] = "";
                            this.Success = false;
                        }
                        break;
                }
            }
        }
    }
}
