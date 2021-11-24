using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace Audit.Lib
{
    public class WatchPathCollection : Dictionary<string, WatchPath>
    {
        private List<string> _CheckedKeys = new List<string>();

        public WatchPath GetWatchPath(string path)
        {
            string matchKey = this.Keys.FirstOrDefault(x => x.Equals(path, StringComparison.OrdinalIgnoreCase));
            return matchKey == null ? null : this[matchKey];
        }

        public void SetWatchPath(string path, WatchPath watchPath)
        {
            watchPath.FullPath = path;
            this[path] = watchPath;
            this._CheckedKeys.Add(path);
        }

        public IEnumerable<string> GetUncheckedKeys()
        {
            return this.Keys.Where(x => !_CheckedKeys.Any(y => y.Equals(x, StringComparison.OrdinalIgnoreCase)));
        }

        #region Load/Save

        public static WatchPathCollection Load(string dbDir, string id)
        {
            try
            {
                using (var sr = new StreamReader(Path.Combine(dbDir, id), Encoding.UTF8))
                {
                    return JsonSerializer.Deserialize<WatchPathCollection>(sr.ReadToEnd());
                }
            }
            catch { }
            return new WatchPathCollection();
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
