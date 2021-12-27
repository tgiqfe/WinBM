using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WinBM
{
    public class GlobalLog
    {
        #region Private Parameter

        public static readonly string WorkDir = Environment.UserName == "SYSTEM" ?
            Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), "WinBM") :
            Path.Combine(Path.GetTempPath(), "WinBM");

        public static readonly string DBFile = Path.Combine(WorkDir, "setting.db");

        public static readonly string DBJson = Path.Combine(WorkDir, "setting.json");

        public static readonly string LogDir = Path.Combine(WorkDir, "Logs");

        public static readonly string LogFile = Path.Combine(LogDir, DateTime.Now.ToString("yyyyMMdd") + ".log");

        #endregion

        /// <summary>
        /// ログ保存先フォルダーを準備
        /// </summary>
        public static void Init()
        {
            if (!Directory.Exists(LogDir))
            {
                Directory.CreateDirectory(LogDir);
            }
        }

        /// <summary>
        /// ログ書き込み
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public static void WriteLog(LogLevel level, string message)
        {
            using (var sw = new StreamWriter(LogFile, true, Encoding.UTF8))
            {
                sw.WriteLine("[{0}][{1}] {2}",
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), level, message);
            }
        }

        /// <summary>
        /// ログ書き込み。動的パラメータに対応
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void WriteLog(LogLevel level, string message, params object[] args)
        {
            WriteLog(level, string.Format(message, args));
        }
    }
}
