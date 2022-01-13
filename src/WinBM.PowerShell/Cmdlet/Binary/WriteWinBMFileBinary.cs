using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace WinBM.PowerShell.Cmdlet.Binary
{
    [Cmdlet(VerbsCommunications.Write, "WinBMFileBinary")]
    public class WriteWinBMFileBinary : PSCmdlet
    {
        [Parameter(ValueFromPipeline = true)]
        public string Text { get; set; }

        [Parameter]
        public string ImportFile { get; set; }

        [Parameter(Mandatory = true, Position = 0)]
        public string OutputFile { get; set; }

        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            string text = "";
            if (!string.IsNullOrEmpty(Text))
            {
                text = Text;
            }
            if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(ImportFile))
            {
                using (var sr = new StreamReader(ImportFile, Encoding.UTF8))
                {
                    text = sr.ReadToEnd();
                }
            }
            if (text.Contains("\n"))
            {
                text = Regex.Replace(text, "\r?\n", "");
            }

            byte[] bytes = new byte[0] { };
            if (Regex.IsMatch(text, @"^[0-9a-fA-F]+$"))
            {
                var tempBytes = new List<byte>();
                for (int i = 0; i < text.Length / 2; i++)
                {
                    tempBytes.Add(Convert.ToByte(text.Substring(i * 2, 2), 16));
                }
                bytes = tempBytes.ToArray();
            }

            using (var fw = new FileStream(OutputFile, FileMode.Create, FileAccess.Write))
            using (var bw = new BinaryWriter(fw))
            {
                bw.Write(bytes);
            }
        }

        protected override void EndProcessing()
        {
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
