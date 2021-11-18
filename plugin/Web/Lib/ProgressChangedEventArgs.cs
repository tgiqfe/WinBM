using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Lib
{
    /// <summary>
    /// ダウンロード進捗状況を格納するEvent引数用クラス
    /// </summary>
    class ProgressChangedEventArgs
    {
        private long _totalSize = 0;
        private long _downloadedSize = 0;
        private long _speedPerInterval = 0;
        private double _progressPercentage = 0;
        private bool _enabled = false;

        public long DownloadedSize { get { return this._downloadedSize; } }
        public long TotalSize { get { return this._totalSize; } }
        public long SpeedPerInterval { get { return this._speedPerInterval; } }
        public string DownloadedSize_readable
        {
            get
            {
                string sizeText = _downloadedSize switch
                {
                    long size when (size >= 0 && size < 1000) =>
                        string.Format("{0}Byte", size),
                    long size when (size >= 1000 && size < 1000000) =>
                        string.Format("{0}KB", Math.Round((size * 1.0) / 1024, 2)),
                    long size when (size >= 1000000 && size < 1000000000) =>
                        string.Format("{0}MB", Math.Round((size * 1.0) / 1024 / 1024, 2)),
                    long size when (size >= 1000000000 && size < 1000000000000) =>
                        string.Format("{0}GB", Math.Round((size * 1.0) / 1024 / 1024 / 1024, 2)),
                    long size when (size >= 1000000000000) =>
                        string.Format("{0}TB", Math.Round((size * 1.0) / 1024 / 1024 / 1024 / 1024, 2)),
                    _ => "-",
                };
                return sizeText;
            }
        }
        public string TotalSize_readable
        {
            get
            {
                string sizeText = _totalSize switch
                {
                    long size when (size >= 0 && size < 1000) =>
                        string.Format("{0}Byte", size),
                    long size when (size >= 1000 && size < 1000000) =>
                        string.Format("{0}KB", Math.Round((size * 1.0) / 1024, 2)),
                    long size when (size >= 1000000 && size < 1000000000) =>
                        string.Format("{0}MB", Math.Round((size * 1.0) / 1024 / 1024, 2)),
                    long size when (size >= 1000000000 && size < 1000000000000) =>
                        string.Format("{0}GB", Math.Round((size * 1.0) / 1024 / 1024 / 1024, 2)),
                    long size when (size >= 1000000000000) =>
                        string.Format("{0}TB", Math.Round((size * 1.0) / 1024 / 1024 / 1024 / 1024, 2)),
                    _ => "-",
                };
                return sizeText;
            }
        }
        public string SpeedPerInterval_readable
        {
            get
            {
                string sizeText = _speedPerInterval switch
                {
                    long size when (size >= 0 && size < 1000) =>
                        string.Format("{0}Byte", size),
                    long size when (size >= 1000 && size < 1000000) =>
                        string.Format("{0}KB", Math.Round((size * 1.0) / 1024, 2)),
                    long size when (size >= 1000000 && size < 1000000000) =>
                        string.Format("{0}MB", Math.Round((size * 1.0) / 1024 / 1024, 2)),
                    long size when (size >= 1000000000 && size < 1000000000000) =>
                        string.Format("{0}GB", Math.Round((size * 1.0) / 1024 / 1024 / 1024, 2)),
                    long size when (size >= 1000000000000) =>
                        string.Format("{0}TB", Math.Round((size * 1.0) / 1024 / 1024 / 1024 / 1024, 2)),
                    _ => "-",
                };
                return sizeText;
            }
        }

        public double ProgressPercentage { get { return this._progressPercentage; } }
        public bool Enabled { get { return this._enabled; } }

        public ProgressChangedEventArgs() { }
        public ProgressChangedEventArgs(long? totalSize, long downloadedSize, long lastDownloadedSize)
        {
            this._downloadedSize = downloadedSize;
            this._speedPerInterval = downloadedSize - lastDownloadedSize;
            if (totalSize.HasValue)
            {
                this._totalSize = (long)totalSize;
                //this._progressPercentage = Math.Round((double)downloadedSize / totalSize.Value * 100, 2);
                this._progressPercentage = (double)downloadedSize / totalSize.Value;
                this._enabled = true;
            }
        }
    }
}
