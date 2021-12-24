using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBM
{
    public class FileScope
    {
        public List<FileEnv> FileEnvList { get; set; }

        public void AddEnv(string path, string name, string val)
        {
            this.FileEnvList ??= new List<FileEnv>();
            this.FileEnvList.Add(new FileEnv()
            {
                Path = path,
                Name = name,
                Value = val
            });
        }
    }

    public class FileEnv
    {
        public string Path { get; set; }

        private string _Name = null;
        public string Name
        {
            get { return this._Name; }
            set { this._Name = "%" + value + "%"; }
        }

        public string Value { get; set; }

        public bool IsMathPath(string path)
        {
            return this.Path.Equals(path, StringComparison.OrdinalIgnoreCase);
        }

        public void Resolv(ref string text)
        {
            if (text.Contains(this.Name, StringComparison.OrdinalIgnoreCase))
            {
                text = System.Text.RegularExpressions.Regex.Replace(
                    text,
                    this.Name,
                    this.Value,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
        }
    }
}
