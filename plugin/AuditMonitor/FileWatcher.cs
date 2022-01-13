using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace AuditMonitor
{
    class FileWatcher : IDisposable
    {
        /// <summary>
        /// PositionManager
        /// </summary>
        private PositionManager _Manager = null;

        /// <summary>
        /// FileSystemWatcherクラス
        /// </summary>
        private FileSystemWatcher _Watcher = null;

        private object lockObj = new object();

        private string _TargetFile = null;
        private int _Position = 0;
        private long _WatchSize = 0;
        private bool disposedValue;

        public FileWatcher() { }
        public FileWatcher(string targetPath, PositionManager manager)
        {
            this._TargetFile = targetPath;

            this._Manager = manager;

            this._Watcher = new FileSystemWatcher();
            _Watcher.Path = Path.GetDirectoryName(targetPath);
            _Watcher.Filter = Path.GetFileName(targetPath);
            _Watcher.IncludeSubdirectories = false;
            _Watcher.NotifyFilter = NotifyFilters.LastWrite;
            _Watcher.Changed += new FileSystemEventHandler((source, e) =>
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Changed:
                        ReadTargetFile();
                        break;
                }
            });
            _Watcher.EnableRaisingEvents = true;

            ReadTargetFile();
        }

        /// <summary>
        /// ファイル初回読み込み時/更新時に追加出力
        /// </summary>
        private void ReadTargetFile()
        {
            //  前回読み込み時よりファイルサイズが小さい場合は、再作成されたと認識
            long targetSize = new FileInfo(_TargetFile).Length;
            if (targetSize < _WatchSize)
            {
                _Manager.ClearView();
                this._Position = 0;
            }
            _WatchSize = targetSize;

            lock (lockObj)
            {
                int count = 0;
                using(var fs = new FileStream(_TargetFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    string readLine = "";
                    StringBuilder sb = new StringBuilder();
                    while ((readLine = sr.ReadLine()) != null)
                    {
                        count++;
                        if (count <= _Position) { continue; }

                        /*
                         * AuditMonitorデータベースファイルをJsonIndentedで出力する為の記述
                         */
                        if (readLine.EndsWith("};"))
                        {
                            sb.AppendLine(readLine.TrimEnd(';'));
                            _Manager.AddAndView(sb.ToString());
                            sb.Clear();
                        }
                        else
                        {
                            sb.AppendLine(readLine);
                        }

                        /*
                         * AuditMonitorデータベースファイルを1行ずつ出力する場合は↓の記述。
                         */
                        //_Manager.AddAndView(readLine);
                    }
                }
                this._Position = count;
            }
        }

        /// <summary>
        /// 監視終了
        /// </summary>
        public void Stop()
        {
            if (_Watcher != null)
            {
                _Watcher.EnableRaisingEvents = false;
                _Watcher.Dispose();
                _Watcher = null;
            }
        }

        #region Disposable method

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }
                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
