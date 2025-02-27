﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM.Task;
using WinBM;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Audit.Lib.Monitor;

namespace Audit.Work
{
    internal class AuditTaskWork : TaskJob
    {
        private static string _AuditMonitorFile = null;

        private static string _SinceDBFile = null;

        private static string _WatchDBDir = null;

        /// <summary>
        /// PreProcessのタイミングでAuditMonitorの準備
        /// </summary>
        public override void PreProcess()
        {
            base.PreProcess();

            if (_AuditMonitorFile == null)
            {
                if ((Manager.PluginParam?.ContainsKey(Item.AUDIT_MONITORFILE) ?? false) &&
                    !string.IsNullOrEmpty(Manager.PluginParam[Item.AUDIT_MONITORFILE]))
                {
                    _AuditMonitorFile = Manager.PluginParam[Item.AUDIT_MONITORFILE];
                }
                else
                {
                    _AuditMonitorFile = Item.GetDefaultMonitorFile();
                }

                string parent = Path.GetDirectoryName(_AuditMonitorFile);
                if (!System.IO.Directory.Exists(parent))
                {
                    System.IO.Directory.CreateDirectory(parent);
                }
            }
        }

        /// <summary>
        /// AuditMonitorファイルに結果情報を格納
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="invert"></param>
        protected void AddAudit(Dictionary<string, string> dictionary, bool invert)
        {
            var leaf = new AuditMonitor.LogLeaf()
            {
                TaskName = this.TaskName,
                PageName = this.PageName,
                SpecName = this.SpecName,
                TimeStamp = DateTime.Now,
                Invert = invert,
                Result = this.Success ^ invert,
                Params = dictionary
            };
            leaf.AddLine(_AuditMonitorFile);

            Manager.WriteLog(LogLevel.Info, $"{this.TaskName} Add audit result. serial={leaf.Serial}");
        }

        #region Load/Save SinceDB

        /// <summary>
        /// SinceDBファイルを読み込んで現在読み込みされているポジションを取得
        /// </summary>
        /// <param name="logPath"></param>
        /// <returns></returns>
        protected int LoadSinceDB(string logPath)
        {
            if (_SinceDBFile == null)
            {
                if ((Manager.PluginParam?.ContainsKey(Item.AUDIT_SINCEDBFILE) ?? false) &&
                    !string.IsNullOrEmpty(Manager.PluginParam[Item.AUDIT_SINCEDBFILE]))
                {
                    _SinceDBFile = Manager.PluginParam[Item.AUDIT_SINCEDBFILE];
                }
                else
                {
                    _SinceDBFile = Item.GetDefaultSinceDBFile();
                }
            }

            try
            {
                using (var sr = new StreamReader(_SinceDBFile, Encoding.UTF8))
                {
                    var db = JsonSerializer.Deserialize<Dictionary<string, int>>(sr.ReadToEnd());
                    if (db.ContainsKey(logPath))
                    {
                        return db[logPath];
                    }
                }
            }
            catch
            {
                Manager.WriteLog(LogLevel.Debug, "SinceDB read error.");
            }

            return 0;
        }

        /// <summary>
        /// 現在読み込みしたポジションを保存
        /// </summary>
        /// <param name="count"></param>
        protected void SaveSinceDB(string logPath, int count)
        {
            if (_SinceDBFile == null)
            {
                if ((Manager.PluginParam?.ContainsKey(Item.AUDIT_SINCEDBFILE) ?? false) &&
                    !string.IsNullOrEmpty(Manager.PluginParam[Item.AUDIT_SINCEDBFILE]))
                {
                    _SinceDBFile = Manager.PluginParam[Item.AUDIT_SINCEDBFILE];
                }
                else
                {
                    _SinceDBFile = Item.GetDefaultSinceDBFile();
                }
            }

            Dictionary<string, int> db = null;
            try
            {
                using (var sr = new StreamReader(_SinceDBFile, Encoding.UTF8))
                {
                    db = JsonSerializer.Deserialize<Dictionary<string, int>>(sr.ReadToEnd());
                }
            }
            catch
            {
                Manager.WriteLog(LogLevel.Debug, "SinceDB read error.");
            }

            db ??= new Dictionary<string, int>();
            db[logPath] = count;

            try
            {
                string parent = Path.GetDirectoryName(_SinceDBFile);
                if (!System.IO.Directory.Exists(parent))
                {
                    System.IO.Directory.CreateDirectory(parent);
                }
                using (var sw = new StreamWriter(_SinceDBFile, false, Encoding.UTF8))
                {
                    sw.WriteLine(JsonSerializer.Serialize(
                        db,
                        new JsonSerializerOptions() { WriteIndented = true }));
                }
            }
            catch
            {
                Manager.WriteLog(LogLevel.Debug, "SinceDB write error.");
            }
        }

        #endregion
        #region WatchDB

        protected string GetWatchDBDirectory()
        {
            if (_WatchDBDir == null)
            {
                if ((Manager.PluginParam?.ContainsKey(Item.AUDIT_WATCHDBDIR) ?? false) &&
                    !string.IsNullOrEmpty(Manager.PluginParam[Item.AUDIT_WATCHDBDIR]))
                {
                    _WatchDBDir = Manager.PluginParam[Item.AUDIT_WATCHDBDIR];
                }
                else
                {
                    _WatchDBDir = Item.GetDefaultWatchDBDir();
                }
            }
            return _WatchDBDir;
        }

        #endregion
    }
}
