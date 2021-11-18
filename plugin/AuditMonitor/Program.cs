using System;
using System.IO;
using AuditMonitor.Arguments;
using System.Threading;
using System.Text;

namespace AuditMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Mutex mutex = new Mutex(false, "WinBM_AuditMonitor"))
            {
                if (!mutex.WaitOne(0, false)) { return; }

                var ap = new ArgumentParameter(args);
                if (ap.Enabled)
                {
                    RunProcess(ap);
                }
            }
        }

        private static void RunProcess(ArgumentParameter ap)
        {
            //  監視対象ファイルのリセット
            if (ap.ResetMonitorTarget && File.Exists(ap.MonitorTarget))
            {
                File.Delete(ap.MonitorTarget);
            }
            if (!File.Exists(ap.MonitorTarget))
            {
                string parent = Path.GetDirectoryName(ap.MonitorTarget);
                if (!Directory.Exists(parent))
                {
                    Directory.CreateDirectory(parent);
                }
                File.CreateText(ap.MonitorTarget).Close();
            }

            var manager = new PositionManager();
            using (var watcher = new FileWatcher(ap.MonitorTarget, manager))
            {
                //  Control+C無効化
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                };

                Console.Title = "AuditMonitor (Ctrl+Alt+￥で終了)";

                bool reading = true;
                while (reading)
                {
                    ConsoleKeyInfo c = Console.ReadKey(true);
                    switch (c.Key)
                    {
                        case ConsoleKey.Oem102:
                        case ConsoleKey.Oem5:
                            //  Ctrl+Alt+￥ (日本語キーボードの場合、￥キー2箇所に対応)
                            if (((c.Modifiers & ConsoleModifiers.Alt) != 0) &&
                                ((c.Modifiers & ConsoleModifiers.Control) != 0) &&
                                ((c.Modifiers & ConsoleModifiers.Shift) == 0))
                            {
                                reading = false;
                            }
                            break;
                        case ConsoleKey.Oem2:
                            //  Ctrl+Alt+/
                            if (((c.Modifiers & ConsoleModifiers.Alt) != 0) &&
                                ((c.Modifiers & ConsoleModifiers.Control) != 0) &&
                                ((c.Modifiers & ConsoleModifiers.Shift) == 0))
                            {
                                using (var sw = new StreamWriter(ap.MonitorTarget, false, Encoding.UTF8))
                                {
                                    sw.WriteLine();
                                }
                            }
                            break;
                        case ConsoleKey.UpArrow:
                            manager.MoveUp();
                            break;
                        case ConsoleKey.DownArrow:
                            manager.Movedown();
                            break;
                        case ConsoleKey.Home:
                            manager.MoveHome();
                            break;
                        case ConsoleKey.End:
                            manager.MoveEnd();
                            break;
                    }
                }
            }
        }
    }
}
