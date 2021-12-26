using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.RepresentationModel;
using System.IO;
using WinBM.Recipe.Lib;
using System.Reflection;
using LiteDB;

namespace WinBM.Recipe
{
    public class Page
    {
        public enum EnumKind
        {
            Init, Config, Output, Job
        }

        [YamlIgnore]
        [BsonId]
        public string Serial
        {
            //  Metadata自体がnullの場合は [_] を返す。Metadata.Nameがnullの場合は [-] を返す。
            get
            {
                return $"{this.FilePath}:{this.Index}";
            }
        }

        [YamlIgnore]
        public string FilePath { get; set; }

        [YamlIgnore]
        public int Index { get; set; }

        [YamlMember(Alias = "kind")]
        public EnumKind Kind { get; set; }

        [YamlMember(Alias = "metadata")]
        public Metadata Metadata { get; set; }

        [YamlMember(Alias = "env")]
        public PageEnv Env { get; set; }

        [YamlMember(Alias = "config")]
        public PageConfig Config { get; set; }

        [YamlMember(Alias = "output")]
        public PageOutput Output { get; set; }

        [YamlMember(Alias = "job")]
        public PageJob Job { get; set; }

        #region Serialize

        /// <summary>
        /// Yamlファイルへ書き込む。上書きモード固定
        /// </summary>
        /// <param name="fileName"></param>
        public void Serialize(string fileName)
        {
            Serialize(fileName, false);
        }

        /// <summary>
        /// Yamlファイルへ書き込む。
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="append"></param>
        public void Serialize(string fileName, bool append)
        {
            using (var sw = new StreamWriter(fileName, append, Encoding.UTF8))
            {
                Serialize(sw);
            }
        }

        /// <summary>
        /// Yamlファイルへ書き込む。
        /// </summary>
        /// <param name="tw"></param>
        public void Serialize(TextWriter tw)
        {
            //  Kindで指定していないプロパティを除外
            PropertyInfo[] props = this.GetType().
                GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).
                Where(x => x.Name != "Kind" && x.Name != "Metadata" && x.Name != "Serial" && x.Name != "FilePath" && x.Name != "Index").
                ToArray();
            foreach (PropertyInfo prop in props)
            {
                if (prop.Name == this.Kind.ToString())
                {
                    var pageBase = (prop.GetValue(this) as PageBase);
                    pageBase.PreSerialize();
                }
                else
                {
                    prop.SetValue(this, null);
                }
            }

            //  Priority == 0の場合はnull化
            if (this.Metadata != null && this.Metadata.Priority == "0")
            {
                this.Metadata.Priority = null;
            }

            var serializer = new SerializerBuilder().
                WithEmissionPhaseObjectGraphVisitor(x =>
                    new YamlIEnumerableSkipEmptyObjectGraphVisitor(x.InnerVisitor)).
                    Build();
            tw.WriteLine("---");
            tw.WriteLine(serializer.Serialize(this));
        }

        public string Serialize()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            {
                this.Serialize(writer);
                writer.Flush();
                stream.Position = 0;
                return reader.ReadToEnd();
            }
        }

        #endregion
        #region Deserialize

        public static List<Page> Deserialize(string fileName)
        {
            using (var sr = new StreamReader(fileName, Encoding.UTF8))
            {
                List<Page> list = Deserialize(sr);
                list.ForEach(x =>
                {
                    x.FilePath = fileName;
                    x.Metadata.SetFilePath(fileName);
                });
                return list;
            }
        }

        /// <summary>
        /// Yamlファイルを読み込むTextReaderからデシリアライズ
        /// </summary>
        /// <param name="tr"></param>
        /// <returns></returns>
        public static List<Page> Deserialize(TextReader tr)
        {
            var list = new List<Page>();

            var yaml = new YamlStream();
            yaml.Load(tr);
            foreach (YamlDocument document in yaml.Documents)
            {
                YamlNode node = document.RootNode;
                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream))
                using (var reader = new StreamReader(stream))
                {
                    new YamlStream(new YamlDocument[] { new YamlDocument(node) }).Save(writer);
                    writer.Flush();
                    stream.Position = 0;
                    list.Add(new Deserializer().Deserialize<Page>(reader));
                }
            }

            int count = 0;
            foreach (Page page in list)
            {
                //  Metadata設定の修正
                count++;
                page.Index = count;
                page.Metadata ??= new Metadata();
                if (string.IsNullOrEmpty(page.Metadata.Name))
                {
                    page.Metadata.Name = $"Page{count}";
                }

                //  Priorityがnullの場合は0に戻す
                if (string.IsNullOrEmpty(page.Metadata.Priority))
                {
                    page.Metadata.Priority = "0";
                }

                //  Kindに一致したコンテンツ(Env/Config/Output/Job)を残して削除
                PropertyInfo[] props = page.GetType().
                    GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).
                    Where(x => x.Name != "Kind" && x.Name != "Metadata" && x.Name != "Serial" && x.Name != "FilePath" && x.Name != "Index").
                    ToArray();
                foreach (PropertyInfo prop in props)
                {
                    if (prop.Name == page.Kind.ToString())
                    {
                        var pageBase = (prop.GetValue(page) as PageBase);
                        pageBase.PostDeserialize(page.Metadata.Name);
                    }
                    else
                    {
                        prop.SetValue(page, null);
                    }
                }
            }

            return list;
        }

        #endregion
        #region Save to DB

        public static void Save(IEnumerable<Page> list, string fileName)
        {
            string parent = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(parent))
            {
                Directory.CreateDirectory(parent);
            }
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            using (var litedb = new LiteDatabase(fileName))
            {
                var collection = litedb.GetCollection<Page>("WinBMRecipe");
                collection.EnsureIndex(x => x.Serial, true);
                collection.Upsert(list);
            }
        }

        public static IEnumerable<Page> Load(string fileName)
        {
            List<Page> list = null;
            try
            {
                using (var litedb = new LiteDatabase(fileName))
                {
                    var collection = litedb.GetCollection<Page>("WinBMRecipe");
                    collection.EnsureIndex(x => x.Serial, true);
                    list = collection.Query().ToList();
                }
            }
            catch { }

            if (list == null)
            {
                list = new List<Page>();
            }
            return list;
        }

        #endregion

    }
}
