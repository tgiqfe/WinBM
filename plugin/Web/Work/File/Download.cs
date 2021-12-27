using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBM;
using WinBM.Task;
using System.Net;
using System.Web;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Web.Lib;

namespace Web.Work.File
{
    internal class Download : TaskJob
    {
        /// <summary>
        /// Web上のファイルのダウンロードURL
        /// </summary>
        [TaskParameter(Mandatory = true)]
        [Keys("url", "uri")]
        protected string _Uri { get; set; }

        /// <summary>
        /// ダウンロードしたファイルの保存先フォルダー
        /// ファイル名は、URLの最後の値から判定
        /// </summary>
        [TaskParameter(MandatoryAny = 1, Resolv = true)]
        [Keys("outputdirectory", "savedirectory", "destinationdirectory", "outputdir", "savedir", "destinationdir")]
        protected string _OutputDirectory { get; set; }

        /// <summary>
        /// ダウンロードしたファイルの保存先パス。
        /// _OutputDirectory,_Pathのどちらかが必須。_Pathはファイル名も含めて指定
        /// </summary>
        [TaskParameter(MandatoryAny = 2, Resolv = true)]
        [Keys("outputpath", "savepath", "destinationpath")]
        protected string _Path { get; set; }

        /// <summary>
        /// 非同期でダウンロードする。
        /// </summary>
        [TaskParameter]
        [Keys("background", "bk", "back")]
        protected bool _Isbackground { get; set; }

        /// <summary>
        /// bitsadmin実行時のウィンドウタイトル変更用
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpString"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern bool SetWindowText(IntPtr hWnd, string lpString);

        public override void MainProcess()
        {
            if (string.IsNullOrEmpty(_Path))
            {
                _Path = Path.Combine(_OutputDirectory, DownloadFileName.GetFromURL(_Uri));
            }
            Manager.WriteLog(LogLevel.Info, $"Output path -> \"{_Path}\"");

            //  保存先の親フォルダーが存在しない場合
            string parent = Path.GetDirectoryName(_Path);
            if (!System.IO.Directory.Exists(parent))
            {
                Manager.WriteLog(LogLevel.Info, "Create parent folder. \"{0}\"", parent);
                System.IO.Directory.CreateDirectory(parent);
            }

            if (_Isbackground)
            {
                //  バックグラウンドでダウンロード実行
                //  ダウンロード処理はbitsadminコマンドを使用
                using (var proc = new Process())
                {
                    proc.StartInfo.FileName = "bitsadmin.exe";
                    proc.StartInfo.Arguments = $"/transfer \"WinBM_WebFileDownload\" \"{_Uri}\" \"{_Path}\"";

                    //  対話モード実行の場合、新ウィンドウを開いてダウンロード
                    if (Manager.Interactive)
                    {
                        proc.StartInfo.CreateNoWindow = false;
                        proc.StartInfo.UseShellExecute = true;
                    }
                    else
                    {
                        proc.StartInfo.CreateNoWindow = true;
                        proc.StartInfo.UseShellExecute = true;
                        proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        //
                        //  ダウンロード中のリストは↓で確認
                        //  bitsadmin /list
                        //
                        //  ダウンロードをキャンセルする場合は↓コマンドで
                        //  bitsadmin /cancel WinBM_WebFileDownload
                        //
                    }
                    proc.Start();

                    //  ウィンドウタイトルを変更。
                    //  100ミリ秒×20回試行
                    for (int i = 0; i < 20; i++)
                    {
                        IntPtr hwnd = proc.MainWindowHandle;
                        if (hwnd != IntPtr.Zero)
                        {
                            SetWindowText(hwnd, "WebFileDownload");
                            break;
                        }
                        Console.WriteLine(proc.MainWindowHandle);
                        System.Threading.Thread.Sleep(100);
                    }
                }
                this.Success = true;
            }
            else
            {
                //  同期処理でダウンロード (↓の理由で独自DL処理実装)
                //  System.Net.WebClient ⇒ キャンセル処理が実装されていない (できたとしても結構大変そう)
                //  System.Net.HttpClient ⇒ プログレスが実装されていない
                //  Windows.Web.Http.HttpClient ⇒ コンソールアプリでUWPアプリのランタイムを実装する方法が見つからなかった
                using (var dl = new HttpClientDownload(_Uri, _Path))
                {
                    var tokenSource = new System.Threading.CancellationTokenSource();
                    var token = tokenSource.Token;

                    Action<object, ConsoleCancelEventArgs> downloadCancel = (sender, e) =>
                    {
                        tokenSource.Cancel();
                    };
                    Console.CancelKeyPress += new ConsoleCancelEventHandler(downloadCancel);

                    dl.ProgressChanged += (sender, e) =>
                    {
                        Manager.WriteLog(LogLevel.Info, "{0}% ({1} / {2}) [{3}/sec]",
                            Math.Round(e.ProgressPercentage * 100, 2),
                            e.DownloadedSize_readable,
                            e.TotalSize_readable,
                            e.SpeedPerInterval_readable);
                    };
                    dl.DownloadCanceled += (sender, e) =>
                    {
                        Manager.WriteLog(LogLevel.Warn, "Download cancel.");
                    };
                    dl.DownloadCompleted += (sender, e) =>
                    {
                        Manager.WriteLog(LogLevel.Info, "Download complete. \"{0}\"", _Path);
                    };

                    dl.Start(token).Wait();
                    Console.CancelKeyPress -= new ConsoleCancelEventHandler(downloadCancel);
                }

                this.Success = true;
            }
        }
    }
}
