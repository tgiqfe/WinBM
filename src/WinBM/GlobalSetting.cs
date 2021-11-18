using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using System.IO;

namespace WinBM
{
    public class GlobalSetting
    {
        #region Private Parameter

        private static readonly string _DBFile = Path.Combine(
            Environment.GetEnvironmentVariable("ProgramData"), "WinBM", "setting.db");

        private static readonly string _DBJson = Path.Combine(
            Environment.GetEnvironmentVariable("ProgramData"), "WinBM", "setting.json");

        private static readonly string _LogDir = Path.Combine(
            Environment.GetEnvironmentVariable("ProgramData"), "WinBM", "Logs");

        private static readonly string _LogFile = Path.Combine(
            _LogDir, DateTime.Now.ToString("yyyyMMdd") + ".log");

        #endregion

        [BsonId]
        public int Serial { get; set; } = 1;

        /// <summary>
        /// DryRunモードのON/OFF
        /// </summary>
        public bool DryRun { get; set; }

        public bool StepConfig { get; set; }
        public bool StepOutput { get; set; }
        public bool StepRequire { get; set; }
        public bool StepWork { get; set; }

        public string[] PluginFiles { get; set; }

        public string PluginDirectory { get; set; }

        public string WorkDirectory { get; set; }

        public Dictionary<string, string> PluginParam { get; set; }

        #region Init process

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GlobalSetting()
        {
            if (!Directory.Exists(_LogDir))
            {
                Directory.CreateDirectory(_LogDir);
            }
        }

        public void Init()
        {
            this.DryRun = false;
            this.PluginFiles = null;
            this.PluginDirectory = null;
            this.WorkDirectory = null;
        }

        #endregion
        #region Load/Save

        /// <summary>
        /// DBに格納した設定データを取得
        /// </summary>
        /// <returns></returns>
        public static GlobalSetting Load()
        {
            GlobalSetting config = null;
            try
            {
                using (var litedb = new LiteDatabase(_DBFile))
                {
                    var collection = litedb.GetCollection<GlobalSetting>("GlobalConfig");
                    collection.EnsureIndex(x => x.Serial, true);

                    config = collection.Find(x => x.Serial == 1).First();
                }
            }
            catch { }

            if (config == null)
            {
                config = new GlobalSetting();
                config.Init();
            }

            return config;
        }

        /// <summary>
        /// DBに設定データを格納
        /// </summary>
        public void Save()
        {
            string parent = Path.GetDirectoryName(_DBFile);
            if (!Directory.Exists(parent))
            {
                Directory.CreateDirectory(parent);
            }

            using (var litedb = new LiteDatabase(_DBFile))
            {
                var collection = litedb.GetCollection<GlobalSetting>("GlobalConfig");
                collection.EnsureIndex(x => x.Serial, true);
                collection.Upsert(this);
            }
            using (var sw = new StreamWriter(_DBJson, false, Encoding.UTF8))
            {
                sw.WriteLine(System.Text.Json.JsonSerializer.Serialize(this,
                    new System.Text.Json.JsonSerializerOptions()
                    {
                        WriteIndented = true
                    }));
            }
        }

        #endregion
        #region Log

        public void WriteLog(LogLevel level, string message)
        {
            using (var sw = new StreamWriter(_LogFile, true, Encoding.UTF8))
            {
                sw.WriteLine("[{0}][{1}] {2}",
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), level, message);
            }
        }

        public void WriteLog(LogLevel level, string message, params object[] args)
        {
            WriteLog(level, string.Format(message, args));
        }

        #endregion
    }
}
