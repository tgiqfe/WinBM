using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Diagnostics;

namespace Standard.Lib.ScriptLanguage
{
    internal class LanguageCollection : List<Language>
    {
        #region Load/Save

        public static LanguageCollection Load(string path)
        {
            List<Language> list = null;
            try
            {
                using (var sr = new StreamReader(path, Encoding.UTF8))
                {
                    list = JsonSerializer.Deserialize<List<Language>>(sr.ReadToEnd());
                }
            }
            catch { }
            if (list == null)
            {
                list = DefaultLanguageSetting.Create();
            }

            var collection = new LanguageCollection();
            collection.AddRange(list);
            collection.Save(path);

            return collection;
        }

        public void Save(string path)
        {
            string parent = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(parent))
            {
                System.IO.Directory.CreateDirectory(parent);
            }

            try
            {
                using (var sw = new StreamWriter(path, false, Encoding.UTF8))
                {
                    string json = JsonSerializer.Serialize(this, new JsonSerializerOptions()
                    {
                        WriteIndented = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    });
                    sw.WriteLine(json);
                }
            }
            catch { }
        }

        #endregion

        public Language GetLanguage(string filePath)
        {
            if (File.Exists(filePath))
            {
                string extension = Path.GetExtension(filePath);
                return this.FirstOrDefault(x =>
                    x.Extensions.Any(y =>
                        y.Equals(extension, StringComparison.OrdinalIgnoreCase)));
            }
            return null;
        }

        public Process GetProcess(string filePath)
        {
            if (File.Exists(filePath))
            {
                string extension = Path.GetExtension(filePath);
                Language lang = this.FirstOrDefault(x =>
                    x.Extensions.Any(y =>
                        y.Equals(extension, StringComparison.OrdinalIgnoreCase)));
                return lang == null ?
                    null :
                    lang.GetProcess(filePath, "");
            }
            return null;
        }

        public bool ContainExtension(string path)
        {
            string extension = Path.GetExtension(path);
            return this.Any(x =>
                x.Extensions.Any(y =>
                    y.Equals(extension, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
