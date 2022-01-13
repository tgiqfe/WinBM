using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using System.IO.Compression;

namespace WinBM.PowerShell.Cmdlet.Binary
{
    [Cmdlet(VerbsCommunications.Read, "WinBMFileBinary")]
    public class ReadWinBMFileBinary : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; }

        [Parameter, Alias("Output")]
        public string OutputFile { get; set; }

        [Parameter]
        public int TextBlock { get; set; }

        private string _currentDirectory = null;
        const int BUFF_SIZE = 4096;

        protected override void BeginProcessing()
        {
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            if (!File.Exists(Path))
            {
                Console.WriteLine("Target file is missing. \"{0}\"", Path);
                return;
            }

            string retString = "";
            using (var fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            using (var ms = new MemoryStream())
            {
                if (fs.Length < int.MaxValue)
                {
                    byte[] buffer = new byte[BUFF_SIZE];
                    int readed = 0;
                    while ((readed = br.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, readed);
                    }
                    retString = BitConverter.ToString(ms.ToArray()).Replace("-", "");
                }
                else
                {
                    Console.Error.WriteLine("Size Over.");
                    return;
                }
            }

            //  テキストブロック化
            if (this.TextBlock > 0)
            {
                int count = TextBlock;
                using (var sr = new StringReader(retString))
                {
                    int readed = 0;
                    StringBuilder sb = new StringBuilder();
                    char[] buffer = new char[count];
                    while ((readed = sr.Read(buffer, 0, count)) > 0)
                    {
                        sb.AppendLine(new string(buffer, 0, readed));
                    }
                    retString = sb.ToString();
                }
            }

            //  ファイルへ出力
            if (!string.IsNullOrEmpty(OutputFile))
            {
                using (var sw = new StreamWriter(OutputFile, false, Encoding.UTF8))
                {
                    sw.Write(retString);
                }
            }
            else
            {
                WriteObject(retString);
            }
        }

        protected override void EndProcessing()
        {
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
