using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Win32;

namespace Audit.Lib.Monitor
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class MonitorTargetCollection : Dictionary<string, MonitorTarget>
    {
        private List<string> _CheckedKeys = new List<string>();

        const string REGPATH_PREFIX = "[reg]";

        public MonitorTargetCollection() { }

        #region Get/Set MonitorTarget

        public MonitorTarget GetMonitorTarget(string path)
        {
            string matchKey = this.Keys.FirstOrDefault(x => x.Equals(path, StringComparison.OrdinalIgnoreCase));
            return matchKey == null ? null : this[matchKey];
        }

        public MonitorTarget GetMonitorTarget(string path, string name)
        {
            string regPath = REGPATH_PREFIX + path + "\\" + name;
            string matchKey = this.Keys.FirstOrDefault(x => x.Equals(regPath, StringComparison.OrdinalIgnoreCase));
            return matchKey == null ? null : this[matchKey];
        }

        public void SetMonitorTarget(string path, MonitorTarget target)
        {
            target.FullPath = path;
            this[path] = target;
            this._CheckedKeys.Add(path);
        }

        public void SetMonitorTarget(string path, string name, MonitorTarget target)
        {
            string regPath = REGPATH_PREFIX + path + "\\" + name;
            target.FullPath = regPath;
            this[regPath] = target;
            this._CheckedKeys.Add(regPath);
        }

        #endregion

        public IEnumerable<string> GetUncheckedKeys()
        {
            return this.Keys.Where(x => !_CheckedKeys.Any(y => y.Equals(x, StringComparison.OrdinalIgnoreCase)));
        }

        #region Load/Save

        public static MonitorTargetCollection Load(string dbDir, string id)
        {
            try
            {
                using (var sr = new StreamReader(Path.Combine(dbDir, id), Encoding.UTF8))
                {
                    return JsonSerializer.Deserialize<MonitorTargetCollection>(sr.ReadToEnd());
                }
            }
            catch { }
            return new MonitorTargetCollection();
        }

        public void Save(string dbDir, string id)
        {
            if (!Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
            }
            using (var sw = new StreamWriter(Path.Combine(dbDir, id), false, Encoding.UTF8))
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });
                sw.WriteLine(json);
            }
        }

        #endregion
    }
}
