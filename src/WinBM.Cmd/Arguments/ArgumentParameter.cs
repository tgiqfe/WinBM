using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace WinBM.Cmd.Arguments
{
    internal class ArgumentParameter
    {
        [ArgsParam]
        private RecipeFile _RecipeFile { get; set; }

        public bool Enabled { get; set; }
        public RecipeFile RecipeFile { get { return this._RecipeFile; } }
        
        public ArgumentParameter() { }
        public ArgumentParameter(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "/f":
                    case "-f":
                        if (i + 1 < args.Length)
                        {
                            this._RecipeFile = new RecipeFile(args[++i]);
                        }
                        break;
                }
            }
            this.Enabled = CheckParam();
        }

        private bool CheckParam()
        {
            bool ret = true;

            var props = this.GetType().GetProperties(
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                var argParam = prop.GetCustomAttribute<ArgsParamAttribute>();
                if (argParam == null)
                {
                    continue;
                }

                if (argParam.Mandatory)
                {
                    object obj = prop.GetValue(this);
                    ret &= obj switch
                    {
                        string s => !string.IsNullOrEmpty(s),
                        string[] ar => (ar?.Length > 0),
                        _ => obj != null
                    };
                }
            }

            return ret;
        }
    }
}
