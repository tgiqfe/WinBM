using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace WinBM.Recipe
{
    public class PageJob : PageBase
    {
        /// <summary>
        /// Jobを実行する為の前提条件
        /// </summary>
        [YamlMember(Alias = "require")]
        public SpecJob[] Require { get; set; }

        /// <summary>
        /// Jobの実行部分本体
        /// </summary>
        [YamlMember(Alias = "work")]
        public SpecJob[] Work { get; set; }

        public override string ToString()
        {
            string[] reqArray = this.Require == null ?
                new string[] { } :
                this.Require.Select(x => string.IsNullOrEmpty(x.Name) ? "require" : x.Name).ToArray();
            string[] workArray = this.Work == null ?
                new string[] { } :
                this.Work.Select(x => string.IsNullOrEmpty(x.Name) ? "work" : x.Name).ToArray();
            return string.Join(", ", reqArray.Concat(workArray));
        }

        public override void PostDeserialize(string pageName)
        {
            //  名前指定されていない場合に自動的セット
            //  Paramのキーを大文字/小文字無視する為のDictionary再セット
            if (Require != null)
            {
                int count = 0;
                foreach (SpecJob spec in this.Require)
                {
                    count++;
                    if (string.IsNullOrEmpty(spec.Name))
                    {
                        spec.Name = string.Format("{0}_Require{1}_{2}",
                            pageName,
                            count,
                            spec.Task.Substring(0, spec.Task.IndexOf("/")));
                    }
                    if (spec.Param?.Count > 0)
                    {
                        var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (KeyValuePair<string, string> pair in spec.Param)
                        {
                            dictionary[pair.Key] = pair.Value;
                        }
                        spec.Param = dictionary;
                    }
                }
            }
            if (Work != null)
            {
                int count = 0;
                foreach (SpecJob spec in this.Work)
                {
                    count++;
                    if (string.IsNullOrEmpty(spec.Name))
                    {
                        spec.Name = string.Format("{0}_Work{1}_{2}",
                            pageName,
                            count,
                            spec.Task.Substring(0, spec.Task.IndexOf("/")));
                    }
                    if (spec.Param?.Count > 0)
                    {
                        var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (KeyValuePair<string, string> pair in spec.Param)
                        {
                            dictionary[pair.Key] = pair.Value;
                        }
                        spec.Param = dictionary;
                    }
                }
            }
        }
    }
}
