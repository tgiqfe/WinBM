﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using IO.Lib;
using System.IO;

namespace IO.Work.File
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Attribute : WorkFile
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        [TaskParameter(MandatoryAny = 1)]
        [Keys("removesecurityblock", "removesecblock", "securityblock", "secblock")]
        protected bool _RemoveSecurityBlock { get; set; }

        [TaskParameter(MandatoryAny = 2)]
        [Keys("attributes", "attribute", "attribs", "attrib", "attrs", "attr")]
        protected string[] _Attributes { get; set; }

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
            this.Success = true;

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
            }

            TargetSequence(_Path, AttributeFileAction);
        }

        private void AttributeFileAction(string target)
        {
            try
            {
                //  セキュリティブロックを解除
                if (_RemoveSecurityBlock && FileControl.CheckSecurityBlock(target))
                {
                    Manager.WriteLog(LogLevel.Info, "{0} Remove securityblock.", this.TaskName);
                    FileControl.RemoveSecurityBlock(target);
                }

                //  属性を設定
                if (_Attributes?.Length > 0)
                {
                    Action<string> setAttribute = (targetFileOrDirectory) =>
                    {
                        FileAttributes attr = System.IO.File.GetAttributes(targetFileOrDirectory);

                        Action<FileAttributes, string, bool> actionAttributes = (checkAttr, attrString, toEnable) =>
                        {
                            bool hasAttribute = (attr & checkAttr) == checkAttr;
                            if (!hasAttribute && toEnable)
                            {
                                Manager.WriteLog(LogLevel.Info, $"Attribute add: {attrString}");
                                attr |= checkAttr;
                            }
                            else if (hasAttribute && !toEnable)
                            {
                                Manager.WriteLog(LogLevel.Info, $"Attribute subtrcut: {attrString}");
                                attr &= (~checkAttr);
                            }
                        };

                        if (_attr_readOnly != null)
                        {
                            actionAttributes(FileAttributes.ReadOnly, "ReadOnly", (bool)_attr_readOnly);
                        }
                        if (_attr_hidden != null)
                        {
                            actionAttributes(FileAttributes.Hidden, "Hidden", (bool)_attr_hidden);
                        }
                        if (_attr_system != null)
                        {
                            actionAttributes(FileAttributes.System, "System", (bool)_attr_system);
                        }

                        System.IO.File.SetAttributes(targetFileOrDirectory, attr);
                    };

                    setAttribute(target);
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
    /// Attributesへのエイリアス
    /// </summary>
    internal class Attributes : Attribute
    {
        protected override bool IsAlias { get { return true; } }
    }
}
