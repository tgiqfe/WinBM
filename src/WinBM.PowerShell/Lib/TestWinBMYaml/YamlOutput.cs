﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WinBM.PowerShell.Lib.TestWinBMYaml
{
    internal class YamlOutput
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Skip { get; set; }
        public string Task { get; set; }
        public Dictionary<string, string> Param { get; set; }
        public List<string> IllegalList { get; set; }

        /// <summary>
        /// インスタンス作成用メソッド
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static List<YamlOutput> Create(string content)
        {
            List<Dictionary<string, string>> paramsetList = new List<Dictionary<string, string>>();

            using (var sr = new StringReader(content))
            {
                string readLine = "";
                bool inChild = false;
                while ((readLine = sr.ReadLine()) != null)
                {
                    if (readLine == "output:")
                    {
                        inChild = true;
                        continue;
                    }
                    if (inChild && readLine.Trim() == "spec:")
                    {
                        paramsetList = YamlFunctions.GetParameters(sr);
                        break;
                    }
                }
            }

            List<YamlOutput> list = new List<YamlOutput>();
            foreach (Dictionary<string, string> paramset in paramsetList)
            {
                var spec = new YamlOutput();
                spec.IllegalList = new List<string>();
                foreach (KeyValuePair<string, string> pair in paramset)
                {
                    switch (pair.Key)
                    {
                        case "name":
                            spec.Name = pair.Value;
                            break;
                        case "description":
                            spec.Description = pair.Value;
                            break;
                        case "skip":
                            if (bool.TryParse(pair.Value, out bool skip))
                            {
                                spec.Skip = skip;
                            }
                            else
                            {
                                spec.IllegalList.Add(pair.Key + ": " + pair.Value);
                            }
                            break;
                        case "task":
                            spec.Task = pair.Value;
                            break;
                        case "param":
                            using (var sr = new StringReader(pair.Value))
                            {
                                spec.Param = YamlFunctions.GetParameters(sr)[0];
                            }
                            break;
                        default:
                            spec.IllegalList.Add("[Illegal] " + pair.Key + ": " + pair.Value);
                            break;
                    }
                }
                list.Add(spec);
            }

            return list;
        }

        public string SearchIllegal()
        {
            if (IllegalList.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine();
                foreach (var illegal in IllegalList)
                {
                    sb.AppendLine($"      {illegal}");
                }
                return sb.ToString();
            }
            return null;
        }
    }
}
