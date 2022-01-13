using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace AuditMonitor
{
    public class LogLeaf
    {
        private static int _Serial = 0;

        #region Class Parameter

        public int Serial { get; set; }
        public string TaskName { get; set; }
        public string PageName { get; set; }
        public string SpecName { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool Invert { get; set; }
        public bool Result { get; set; }
        public Dictionary<string, string> Params { get; set; }

        #endregion

        private string _LineString_pre = null;
        private string _LineString_suf = null;
        private string _DetailString_pre = null;
        private string _DetailString_suf = null;

        public LogLeaf()
        {
            this.Serial = ++_Serial;
        }

        public string ToJson()
        {
            string json = null;
            try
            {
                json = JsonSerializer.Serialize(this);
            }
            catch { }
            return json;
        }

        public static LogLeaf FromJson(string jsonText)
        {
            LogLeaf leaf = null;
            try
            {
                leaf = JsonSerializer.Deserialize<LogLeaf>(jsonText);
            }
            catch { }
            return leaf;
        }

        public void AddLine(string auditMonitorFile)
        {
            try
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                });
                using (var sw = new StreamWriter(auditMonitorFile, true, Encoding.UTF8))
                {
                    sw.WriteLine(json + ";");
                }
            }
            catch { }
        }

        static readonly int _rowMaxCount = 90;
        static readonly int _maxDotStringLength = _rowMaxCount - 17;
        static readonly int _maxLogTargetLength = _maxDotStringLength - 5;

        /* 出力サンプル
         * 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789
         * -------10|-------20|-------30|-------40|-------50|-------60|-------70|-------80|-------90|
         * 
         * [Audit/File/Compare] Copytest001 / CopyTest01-audit01_01 ................ [ ● Succeeded ]
         * [Audit/File/Compare] Copytest001 / CopyTest01-audit01_02 ................ [ ● Failed ]
         * [Audit/File/Compare] Copytest001 / CopyTest01-audit01_03_AAAAAAAAAAA. ... [ ● Failed ]
         *                                                                     |     |              |
         *                                               _maxLogTargetLength --+     |              |
         *                                                     _maxDotStringLength --+              |
         *                                                                           _rowMaxCount --+
         */

        /// <summary>
        /// 1行での簡易出力
        /// </summary>
        public void ViewLine()
        {
            if (_LineString_pre == null)
            {
                string logTarget = string.Format("{0} / {1} {2}",
                    this.PageName, this.SpecName, this.TaskName.Substring(TaskName.IndexOf("[")));
                if (logTarget.Length > _maxLogTargetLength)
                {
                    logTarget = logTarget.Substring(0, _maxLogTargetLength) + ".";
                }
                _LineString_pre = (logTarget + " ").PadRight(_maxDotStringLength, '.') + " ";
            }
            if (_LineString_suf == null)
            {
                _LineString_suf = "";
            }

            Console.Write(_LineString_pre);
            OutResult();
            Console.WriteLine(_LineString_suf);
        }

        /// <summary>
        /// 複数行に渡っての詳細出力
        /// </summary>
        public void ViewDetail()
        {
            if (_DetailString_pre == null)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"  Serial          : {this.Serial}");
                sb.AppendLine($"  TimeStamp       : {TimeStamp.ToString("yyyy/MM/dd HH:mm:ss")}");
                sb.AppendLine($"  TaskName        : {this.TaskName}");
                sb.AppendLine($"  PageName        : {this.PageName}");
                sb.AppendLine($"  SpecName        : {this.SpecName}");
                sb.AppendLine($"  Invert          : {this.Invert}");
                sb.Append("  Result          : ");
                _DetailString_pre = sb.ToString();
            }
            if (_DetailString_suf == null)
            {
                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("  Params          :");
                foreach (KeyValuePair<string, string> pair in this.Params)
                {
                    //  長すぎる場合は文字切り
                    string keyString = pair.Key.Length > 60 ?
                        pair.Key.Substring(0, 57) + "..." :
                        pair.Key;
                    string lineString = $"    {keyString} = {pair.Value}";
                    if (lineString.Length > (Console.WindowWidth - 1))
                    {
                        lineString = lineString.Substring(0, Console.WindowWidth - 4) + "...";
                    }
                    sb.AppendLine(lineString);
                }
                _DetailString_suf = sb.ToString();
            }

            Console.Write(_DetailString_pre);
            OutResult();
            Console.Write(_DetailString_suf);
        }

        /// <summary>
        /// Result結果を色を変えて出力
        /// </summary>
        private void OutResult()
        {
            if (this.Result)
            {
                Console.Write("[ ");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write("● Succeeded");
                Console.ResetColor();
                Console.Write(" ]");
            }
            else
            {
                Console.Write("[ ");
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("● Failed");
                Console.ResetColor();
                Console.Write(" ]");
            }
        }
    }
}
