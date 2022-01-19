using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Threading;

namespace Web.Lib
{
    /// <summary>
    /// 参考)
    /// https://stackoverflow.com/questions/20661652/progress-bar-with-httpclient
    /// </summary>
    class HttpClientDownload : IDisposable
    {
        #region ClassParameter

        public int ProgressViewInterval { get; set; } = 1000;
        public CancellationTokenSource TokenSource { get; set; }

        #endregion

        private string _url = null;
        private string _path = null;
        private DateTime _lastViewDate;
        private long _lastDownloadedSize = 0;

        private HttpClient _client = null;
        private bool _disposedValue;

        public delegate void ProgressChangedHandler(object sender, ProgressChangedEventArgs e);
        public delegate void DownloadCompletedHandler(object sender, DownloadCompletedEventArgs e);
        public delegate void DownloadCanceledHandler(object sender, DownloadCanceledEventArgs e);

        public event ProgressChangedHandler ProgressChanged;
        public event DownloadCompletedHandler DownloadCompleted;
        public event DownloadCanceledHandler DownloadCanceled;

        public HttpClientDownload(string url, string path)
        {
            this._url = url;
            this._path = path;
            this._lastViewDate = DateTime.Now;
        }

        public async Task Start()
        {
            this.TokenSource = new CancellationTokenSource();
            await Start(TokenSource.Token);
        }

        public async Task Start(CancellationToken token)
        {
            _client = new HttpClient() { Timeout = TimeSpan.FromHours(1) };

            using (HttpResponseMessage res = await _client.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead))
            {
                await DownloadFromHttpResponseMessage(res, token);
            }
        }

        private async Task DownloadFromHttpResponseMessage(HttpResponseMessage res, CancellationToken token)
        {
            res.EnsureSuccessStatusCode();

            long? totalBytes = res.Content.Headers.ContentLength;

            using (Stream st = await res.Content.ReadAsStreamAsync())
            {
                await ProcessContentStream(totalBytes, st, token);
            }
        }

        private async Task ProcessContentStream(long? totalSize, Stream st, CancellationToken token)
        {
            long downloadedSize = 0;
            long readCount = 0;
            byte[] buffer = new byte[8192];
            bool isMoreToRead = true;

            var status = new DownloadStatus();

            try
            {
                using (var fs = new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    do
                    {
                        //  キャンセル
                        if (token.IsCancellationRequested)
                        {
                            status.Canceled = true;
                            TriggerDownloadCanceled();
                            break;
                        }

                        var bytesRead = await st.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            isMoreToRead = false;
                            TriggerProgressChanged(totalSize, downloadedSize);
                            continue;
                        }
                        await fs.WriteAsync(buffer, 0, bytesRead);

                        downloadedSize += bytesRead;
                        readCount++;

                        if (readCount % 100 == 0)
                        {
                            TriggerProgressChanged(totalSize, downloadedSize);
                        }
                    } while (isMoreToRead);
                }
                status.Success = true;
            }
            catch (Exception e)
            {
                status.Error = true;
                status.Exception = e;
            }

            //  終了
            TriggerDownloadCompleted(status);
        }

        #region EventTrigger

        /// <summary>
        /// ダウンロード進捗が変化したときに発火
        /// </summary>
        /// <param name="totalSize"></param>
        /// <param name="downloadedSize"></param>
        private void TriggerProgressChanged(long? totalSize, long downloadedSize)
        {
            if (ProgressChanged == null)
            {
                return;
            }

            //  進捗の表示間隔を設定
            if (ProgressViewInterval > 0)
            {
                if ((DateTime.Now - _lastViewDate).TotalMilliseconds < ProgressViewInterval)
                {
                    return;
                }
                _lastViewDate = DateTime.Now;
            }

            ProgressChanged(this, new ProgressChangedEventArgs(totalSize, downloadedSize, _lastDownloadedSize));
            _lastDownloadedSize = downloadedSize;
        }

        /// <summary>
        /// ダウンロードが完了したときに発火
        /// </summary>
        private void TriggerDownloadCompleted(DownloadStatus status)
        {
            if (DownloadCompleted == null)
            {
                return;
            }
            DownloadCompleted(this, status.GetDownloadCompletedEventArgs());
        }

        /// <summary>
        /// ダウンロードをキャンセルしたときに発火
        /// </summary>
        private void TriggerDownloadCanceled()
        {
            if (DownloadCanceled == null)
            {
                return;
            }
            DownloadCanceled(this, new DownloadCanceledEventArgs());
        }

        #endregion
        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _client?.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
