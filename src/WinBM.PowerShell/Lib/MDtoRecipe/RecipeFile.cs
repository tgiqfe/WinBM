using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WinBM.PowerShell.Lib.MDtoRecipe
{
    internal class RecipeFile
    {
        public string FileName { get; set; }
        public string Content { get; set; }
        public List<string> RecipeFiles { get; set; }

        public RecipeFile()
        {
            this.RecipeFiles = new List<string>();
        }

        public void Write(string outputDirectory)
        {
            Console.WriteLine("---");
            Console.WriteLine($"FileName: {this.FileName}");
            Console.WriteLine("Source file:");
            RecipeFiles.ForEach(x => Console.WriteLine($"- {x}"));

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            using (var sw = new StreamWriter(Path.Combine(outputDirectory, FileName), false, Encoding.UTF8))
            {
                sw.WriteLine(this.Content);
            }
        }
    }
}
