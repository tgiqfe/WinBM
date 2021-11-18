using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WinBM.Cmd.Arguments
{
    class RecipeFile
    {
        private string[] _RecipeFiles { get; set; }

        public string[] Paths { get { return this._RecipeFiles; } }

        public RecipeFile(string recipeFile)
        {
            if (File.Exists(recipeFile))
            {
                this._RecipeFiles = new string[1] { recipeFile };
            }
            else if (Directory.Exists(recipeFile))
            {
                this._RecipeFiles = Directory.GetFiles(recipeFile).
                    Where(x => Path.GetExtension(x).ToLower() switch
                    {
                        ".yml" => true,
                        ".yaml" => true,
                        _ => false
                    }).
                    ToArray();
            }
        }
    }
}
