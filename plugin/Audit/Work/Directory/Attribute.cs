using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.IO;
using IO.Lib;

namespace Audit.Work.Directory
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Attribute : AuditTaskWork
    {
        [TaskParameter(Mandatory = true, ResolvEnv = true, Delimiter = ';')]
        [Keys("path", "directorypath", "folderpath", "dirpath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        [TaskParameter(MandatoryAny = 1)]
        [Keys("securityblock", "secblock")]
        protected bool? _SecurityBlock { get; set; }

        [TaskParameter(MandatoryAny = 2)]
        [Keys("attributes", "attribute", "attribs", "attrib", "attrs", "attr")]
        protected string[] _Attributes { get; set; }

        [TaskParameter]
        [Keys("invert", "not", "no", "none")]
        protected bool _Invert { get; set; }

        /// <summary>
        /// 属性名の候補リスト
        /// </summary>
        private readonly static string[] _candidate_readonly =
            new string[] { "readonly", "read", "readandexecute", "readandexec", "readadnexe", "readexec", "readexe", "r" };
        private readonly static string[] _candidate_hidden =
            new string[] { "hidden", "hide", "hiden", "h" };
        private readonly static string[] _candidate_system =
            new string[] { "system", "sys", "s" };

        private bool? _attr_readOnly = null;
        private bool? _attr_hidden = null;
        private bool? _attr_system = null;

        public override void MainProcess()
        {
            var dictionary = new Dictionary<string, string>();
            this.Success = true;
            int count = 0;

            if (!_SecurityBlock != null)
            {
                dictionary["Check_SecurityBlock"] = _SecurityBlock.ToString();
            }
            if (_Attributes?.Length > 0)
            {
                foreach (string attribute in _Attributes)
                {
                    if (attribute.StartsWith("-"))
                    {
                        switch (attribute.TrimStart('-'))
                        {
                            case string s when _candidate_readonly.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                                _attr_readOnly = false;
                                break;
                            case string s when _candidate_hidden.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                                _attr_hidden = false;
                                break;
                            case string s when _candidate_system.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                                _attr_system = false;
                                break;
                        }
                    }
                    else
                    {
                        switch (attribute.TrimStart('+'))
                        {
                            case string s when _candidate_readonly.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                                _attr_readOnly = true;
                                break;
                            case string s when _candidate_hidden.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                                _attr_hidden = true;
                                break;
                            case string s when _candidate_system.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)):
                                _attr_system = true;
                                break;
                        }
                    }
                }
                dictionary[$"Check_Attribute"] =
                    string.Format("[{0}]ReadOnly [{1}]Hidden [{2}]System",
                        _attr_readOnly == null ? "-" : (bool)_attr_readOnly ? "x" : " ",
                        _attr_hidden == null ? "-" : (bool)_attr_hidden ? "x" : " ",
                        _attr_system == null ? "-" : (bool)_attr_system ? "x" : " ");
            }

            foreach (string path in _Path)
            {
                if (Path.GetFileName(path).Contains("*"))
                {
                    Manager.WriteLog(LogLevel.Info, "{0} Wildcard Copy.", this.TaskName);

                    //  対象の親フォルダーが存在しない場合
                    string parent = Path.GetDirectoryName(path);
                    if (!System.IO.Directory.Exists(parent))
                    {
                        Manager.WriteLog(LogLevel.Warn, "Parent on target is Missing. \"{0}\"", parent);
                        return;
                    }

                    //  ワイルドカード指定
                    System.Text.RegularExpressions.Regex wildcard = Wildcard.GetPattern(path);
                    System.IO.Directory.GetDirectories(parent).
                        Where(x => wildcard.IsMatch(x)).
                        ToList().
                        ForEach(x => AttributeDirectoryCheck(x, dictionary, ++count));
                }
                else
                {
                    //  対象ファイルが存在しない場合
                    if (!System.IO.Directory.Exists(path))
                    {
                        Manager.WriteLog(LogLevel.Warn, "Target is Missing. \"{0}\"", path);
                        return;
                    }

                    AttributeDirectoryCheck(path, dictionary, ++count);
                }
            }

            AddAudit(dictionary, this._Invert);
        }

        private void AttributeDirectoryCheck(string target, Dictionary<string, string> dictionary, int count)
        {
            dictionary[$"directory_{count}"] = target;

            try
            {
                //  セキュリティブロックのチェック
                //  配下の全ファイルをチェックし、
                //    - 全てtrueかをチェック  ⇒ 結果が全てtrue or 全てtrueというわけではない
                //    - 全てfalseかをチェック ⇒ 結果が全てfalse or 全てfalseというわけではない
                if (_SecurityBlock != null)
                {
                    bool[] retArray = System.IO.Directory.GetFiles(target, "*", SearchOption.AllDirectories).
                        ToList().
                        Select(x => FileControl.CheckSecurityBlock(x)).
                        ToArray();

                    if (((bool)_SecurityBlock && retArray.All(x => x)) ||
                        (!(bool)_SecurityBlock && retArray.All(x => !x)))
                    {
                        dictionary[$"directory_{count}_Match"] = ((bool)_SecurityBlock).ToString();
                    }
                    else
                    {
                        dictionary[$"directory_{count}_NoMatch"] = ((bool)_SecurityBlock).ToString();
                        this.Success = false;
                    }
                }

                //  属性のチェック (配下のファイル/フォルダーはチェックしない)
                if (_Attributes?.Length > 0)
                {
                    FileAttributes attr = System.IO.File.GetAttributes(target);

                    dictionary[$"directory_{count}_Attribute"] =
                        string.Format("[{0}]ReadOnly [{1}]Hidden [{2}]System",
                            (attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ? "x" : " ",
                            (attr & FileAttributes.Hidden) == FileAttributes.Hidden ? "x" : " ",
                            (attr & FileAttributes.System) == FileAttributes.System ? "x" : " ");

                    Action<FileAttributes, string, bool> checkAttributes = (checkAttr, attrString, enable) =>
                    {
                        bool hasAttribute = (attr & checkAttr) == checkAttr;
                        if (hasAttribute ^ !enable)
                        {
                            dictionary[$"directory_{count}_{attrString}_Match"] = hasAttribute.ToString();
                        }
                        else
                        {
                            dictionary[$"directory_{count}_{attrString}_NoMatch"] = hasAttribute.ToString();
                            this.Success = false;
                        }
                    };

                    if (_attr_readOnly != null)
                    {
                        checkAttributes(FileAttributes.ReadOnly, "ReadOnly", (bool)_attr_readOnly);
                    }
                    if (_attr_hidden != null)
                    {
                        checkAttributes(FileAttributes.Hidden, "Hidden", (bool)_attr_hidden);
                    }
                    if (_attr_system != null)
                    {
                        checkAttributes(FileAttributes.System, "System", (bool)_attr_system);
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
