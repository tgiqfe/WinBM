using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using IO.Lib;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace IO.Work.File
{
    internal class Write : WorkFile
    {
        [TaskParameter(Mandatory = true, Resolv = true, Delimiter = ';')]
        [Keys("path", "filepath", "target", "targetpath")]
        protected string[] _Path { get; set; }

        [TaskParameter(Mandatory = true)]
        [Keys("text", "txt")]
        protected string _Text { get; set; }

        [TaskParameter]
        [Keys("append", "apend", "apnd")]
        protected bool _Append { get; set; }

        [TaskParameter]
        [Keys("encoding", "encode", "enc")]
        protected string _Encoding { get; set; }

        [TaskParameter]
        [Keys("writemode", "mode")]
        [Values("text,txt,file", "binary,bin", Default = "text")]
        protected WriteMode _WriteMode { get; set; }

        [TaskParameter]
        [Keys("expand")]
        protected bool? _Expand { get; set; }

        private Encoding enc = null;

        //  Expand用パラメータ
        private const int BUFF_SIZE = 4096;
        private byte[] _bytes = null;

        protected enum WriteMode { Text, Binary, }

        public override void MainProcess()
        {
            this.Success = true;

            //  文字コードセット
            enc = FileEncoding.Get(_Encoding);

            TargetFileProcess(_Path, WriteFileAction);
        }

        private void WriteFileAction(string target)
        {
            //  対象ファイルの親フォルダーが存在しない場合
            string parent = System.IO.Path.GetDirectoryName(target);
            if (!System.IO.Directory.Exists(parent))
            {
                System.IO.Directory.CreateDirectory(parent);
            }

            try
            {
                switch (_WriteMode)
                {
                    case WriteMode.Text:
                        using (var sw = new StreamWriter(target, _Append, enc))
                        {
                            sw.WriteLine(_Text);
                        }
                        break;
                    case WriteMode.Binary:
                        WriteBinaryFile(target);
                        break;
                }
            }
            catch (Exception e)
            {
                Manager.WriteLog(LogLevel.Error, "{0} {1}", this.TaskName, e.Message);
                Manager.WriteLog(LogLevel.Debug, e.ToString());
                this.Success = false;
            }
        }

        private void WriteBinaryFile(string target)
        {
            if (_bytes == null)
            {
                _bytes = new byte[0] { };
                string text = _Text.Contains("\n") ? Regex.Replace(_Text, "\r?\n", "") : _Text;
                if (Regex.IsMatch(text, @"^[0-9a-fA-F]+$"))
                {
                    var tempBytes = new List<byte>();
                    for (int i = 0; i < text.Length / 2; i++)
                    {
                        tempBytes.Add(Convert.ToByte(text.Substring(i * 2, 2), 16));
                    }
                    _bytes = tempBytes.ToArray();
                }
            }

            using (var fs = new FileStream(target, FileMode.Create, FileAccess.Write))
            using (var bw = new BinaryWriter(fs))
            {
                if (_Expand ?? false)
                {
                    //  解凍する場合
                    using (var ms = new MemoryStream(_bytes))
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
                    //  解凍不要
                    bw.Write(_bytes);
                }
            }
        }
    }
}
