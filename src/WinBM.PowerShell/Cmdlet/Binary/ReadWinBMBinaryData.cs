using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO.Compression;
using System.IO;
using Microsoft.Win32;
using IO.Lib;

namespace WinBM.PowerShell.Cmdlet.Binary
{
    [Cmdlet(VerbsCommunications.Read, "WinBMBinaryData")]
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class ReadWinBMBinaryData : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string TargetPath { get; set; }

        [Parameter(Position = 1), Alias("Name")]
        public string TargetName { get; set; }

        [Parameter, Alias("Output")]
        public string OutputFile { get; set; }

        [Parameter]
        public int TextBlock { get; set; }

        [Parameter]
        public SwitchParameter Compress { get; set; }

        private string _currentDirectory = null;
        const int BUFF_SIZE = 4096;
        private string[] regKeyPrefix = new string[]
        {
            "HKCR:\\", "HKEY_CLASSES_ROOT\\",
            "HKCU:\\", "HKEY_CURRENT_USER\\",
            "HKLM:\\", "HKEY_LOCAL_MACHINE\\",
            "HKU:\\", "HKEY_USERS\\",
            "HKCC:\\", "HKEY_CURRENT_CONFIG\\",
        };

        protected override void BeginProcessing()
        {
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            string retText = "";

            //  ファイルorレジストリ値からバイナリデータを取得
            if (regKeyPrefix.Any(x => TargetPath.StartsWith(x, StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrEmpty(TargetName))
            {
                //  レジストリ値
                retText = ReadRegistryBinary(TargetPath, TargetName);
            }
            else if (File.Exists(TargetPath))
            {
                //  バイナリファイル
                retText = ReadFileBinary(TargetPath);
            }
            else
            {
                Console.WriteLine("Target is missing. \"{0}\"", TargetPath);
                return;
            }

            //  テキストブロック化
            if (this.TextBlock > 0)
            {
                int count = TextBlock;
                StringBuilder sb = new StringBuilder();
                using (var sr = new StringReader(retText))
                {
                    int readed = 0;
                    char[] buffer = new char[count];
                    while ((readed = sr.Read(buffer, 0, count)) > 0)
                    {
                        sb.AppendLine(new string(buffer, 0, readed));
                    }
                }
                retText = sb.ToString();
            }

            //  ファイルへ出力 or オブジェクトを返す
            if (!string.IsNullOrEmpty(OutputFile))
            {
                using (var sw = new StreamWriter(OutputFile, false, Encoding.UTF8))
                {
                    sw.Write(retText);
                }
            }
            else
            {
                WriteObject(retText);
            }
        }

        /// <summary>
        /// バイナリファイル読み込み(テキストデータもバイナリとして読み込み)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string ReadFileBinary(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            using (var ms = new MemoryStream())
            {
                if (fs.Length < int.MaxValue)
                {
                    byte[] buffer = new byte[BUFF_SIZE];
                    int readed = 0;

                    if (Compress)
                    {
                        //  圧縮する場合
                        using (var gs = new GZipStream(ms, CompressionMode.Compress))
                        {
                            while ((readed = br.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                gs.Write(buffer, 0, readed);
                            }
                        }
                    }
                    else
                    {
                        //  非圧縮
                        while ((readed = br.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, readed);
                        }
                    }
                    return BitConverter.ToString(ms.ToArray()).Replace("-", "");
                }
                else
                {
                    Console.Error.WriteLine("Size Over.");
                    return null;
                }
            }
        }

        /// <summary>
        /// REG_BINARYのレジストリ値を読み込み
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string ReadRegistryBinary(string key, string name)
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(key, false, false))
            {
                var valueKind = regKey.GetValueKind(name);
                if (valueKind == RegistryValueKind.Binary)
                {
                    if (Compress)
                    {
                        using (var msS = new MemoryStream(regKey.GetValue(name) as byte[]))
                        using (var msD = new MemoryStream())
                        {
                            using (var gs = new GZipStream(msD, CompressionMode.Compress))
                            {
                                byte[] buffer = new byte[BUFF_SIZE];
                                int readed = 0;
                                while ((readed = msS.Read(buffer, 0, BUFF_SIZE)) > 0)
                                {
                                    gs.Write(buffer, 0, readed);
                                }
                            }
                            return BitConverter.ToString(msD.ToArray()).Replace("-", "");
                        }
                    }
                    else
                    {
                        return BitConverter.ToString(regKey.GetValue(name) as byte[]).Replace("-", "").ToUpper();
                    }
                }
            }
            return null;
        }

        protected override void EndProcessing()
        {
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
