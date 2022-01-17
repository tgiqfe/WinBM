using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.IO.Compression;
using Microsoft.Win32;
using IO.Lib;

namespace WinBM.PowerShell.Cmdlet.Binary
{
    [Cmdlet(VerbsCommunications.Write, "WinBMBinaryData")]
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class WriteWinBMBinaryData : PSCmdlet
    {
        [Parameter(ValueFromPipeline = true)]
        public string Text { get; set; }

        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string OutputPath { get; set; }

        [Parameter, Alias("Name")]
        public string OutputName { get; set; }

        [Parameter]
        public SwitchParameter Expand { get; set; }

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
            string text = Text;
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

            if (regKeyPrefix.Any(x => OutputPath.StartsWith(x, StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrEmpty(OutputName))
            {
                //  レジストリ値
                WriteRegistryBinary(OutputPath, OutputName, bytes);
            }
            else
            {
                //  バイナリファイル
                WriteFileBinary(OutputPath, bytes);
            }
        }

        /// <summary>
        /// バイナリファイルとして書き込み
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bytes"></param>
        private void WriteFileBinary(string path, byte[] bytes)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var bw = new BinaryWriter(fs))
            {
                if (Expand)
                {
                    //  解凍する場合
                    using (var ms = new MemoryStream(bytes))
                    using (var gs = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[BUFF_SIZE];
                        int readed = 0;
                        while ((readed = gs.Read(buffer, 0, BUFF_SIZE)) > 0)
                        {
                            bw.Write(buffer, 0, readed);
                        }
                    }
                }
                else
                {
                    //  解凍不要な場合
                    bw.Write(bytes);
                }
            }
        }

        /// <summary>
        /// REG_BINARYのレジストリ値として書き込み
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="bytes"></param>
        private void WriteRegistryBinary(string key, string name, byte[] bytes)
        {
            using (RegistryKey regKey = RegistryControl.GetRegistryKey(key, true, true))
            {
                if (Expand)
                {
                    //  解凍する場合
                    using (var msS = new MemoryStream(bytes))
                    using (var msD = new MemoryStream())
                    {
                        using (var gs = new GZipStream(msS, CompressionMode.Decompress))
                        {
                            byte[] buffer = new byte[BUFF_SIZE];
                            int readed = 0;
                            while ((readed = gs.Read(buffer, 0, BUFF_SIZE)) > 0)
                            {
                                msD.Write(buffer, 0, readed);
                            }
                        }
                        regKey.SetValue(name, msD.ToArray(), RegistryValueKind.Binary);
                    }
                }
                else
                {
                    //  解凍不要な場合
                    regKey.SetValue(name, bytes, RegistryValueKind.Binary);
                }
            }
        }

        protected override void EndProcessing()
        {
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}
