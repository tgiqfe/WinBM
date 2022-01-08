using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using WinBM.Task;

namespace WinBM
{
    public class SessionManager
    {
        #region Stepable

        /// <summary>
        /// Step(pause)が可能かどうか。
        /// 対話モードの場合のみ可能にさせるように、呼び出し側で設定すること。
        /// </summary>
        public bool Stepable { get; set; }

        /// <summary>
        /// Config部のStep
        /// </summary>
        public bool StepConfig { get; set; }

        /// <summary>
        /// output部のStep
        /// </summary>
        public bool StepOutput { get; set; }

        /// <summary>
        /// Require部のStep
        /// </summary>
        public bool StepRequire { get; set; }

        /// <summary>
        /// Work部のStep
        /// </summary>
        public bool StepWork { get; set; }

        #endregion

        /// <summary>
        /// 対話モードであるという宣言。呼び出し側で設定すること。
        /// </summary>
        public bool Interactive { get; set; }

        /// <summary>
        /// プログレスバーを表示させる為に使用。対話モードでPowerShell実行時に指定すること
        /// </summary>
        public Cmdlet Cmdlet { get; set; }

        #region Output Manager

        private List<TaskOutput> _StandardOutputList = null;
        private List<TaskOutput> _StandardErrorOutputList = null;
        private List<TaskOutput> _LogOutputList = null;
        private List<TaskOutput> _TaskOutputList = null;
        private List<TaskOutput> _ProgressBarOutputList = null;

        public void AddOutput(TaskOutput task)
        {
            switch (task.Type)
            {
                case TaskOutput.OutputType.Standard:
                    _StandardOutputList ??= new List<TaskOutput>();
                    _StandardOutputList.Add(task);
                    break;
                case TaskOutput.OutputType.StandardError:
                    _StandardErrorOutputList ??= new List<TaskOutput>();
                    _StandardErrorOutputList.Add(task);
                    break;
                case TaskOutput.OutputType.Log:
                    _LogOutputList ??= new List<TaskOutput>();
                    _LogOutputList.Add(task);
                    break;
                case TaskOutput.OutputType.Task:
                    _TaskOutputList ??= new List<TaskOutput>();
                    _TaskOutputList.Add(task);
                    break;
                case TaskOutput.OutputType.ProgressBar:
                    _ProgressBarOutputList ??= new List<TaskOutput>();
                    _ProgressBarOutputList.Add(task);
                    break;
            }
        }

        /// <summary>
        /// 外部プログラムが標準出力する内容を出力
        /// </summary>
        /// <param name="message"></param>
        public void WriteStandard(string message)
        {
            _StandardOutputList?.ForEach(x => x.Write(message));
        }
        public void WriteStandard(string message, params object[] args)
        {
            WriteStandard(string.Format(message, args));
        }

        /// <summary>
        /// 外部プログラムが標準エラー出力する内容を出力
        /// </summary>
        /// <param name="message"></param>
        public void WriteStandardError(string message)
        {
            _StandardErrorOutputList?.ForEach(x => x.Write(message));
        }
        public void WriteStandardError(string message, params object[] args)
        {
            WriteStandardError(string.Format(message, args));
        }

        /// <summary>
        /// ログを出力
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public void WriteLog(LogLevel level, string message)
        {
            _LogOutputList?.ForEach(x => x.Write(level, message));
        }
        public void WriteLog(LogLevel level, string message, params object[] args)
        {
            WriteLog(level, string.Format(message, args));
        }
        public void WriteLog(string message, params object[] args)
        {
            WriteLog(default(LogLevel), string.Format(message, args));
        }

        /// <summary>
        /// Workの終了後のTaskを返す
        /// </summary>
        /// <param name="returnCode"></param>
        public void WriteTask(TaskBase taskBase)
        {
            _TaskOutputList?.ForEach(x => x.Write(taskBase));
        }

        /// <summary>
        /// PowerShellのRunSpace上にプログレスバーを出力
        /// </summary>
        public void WriteProgressBar(int line, int max, int cursor, string description)
        {
            _ProgressBarOutputList?.ForEach(x => x.Write(line, max, cursor, description));
        }

        #endregion
        #region Plugin parameter

        /// <summary>
        /// プラグインファイルのパスを指定
        /// </summary>
        public string[] PluginFiles { get; set; }

        /// <summary>
        /// プラグインファイルを保存している場所の指定
        /// </summary>
        public string PluginDirectory { get; set; }

        /// <summary>
        /// プラグイン側で使用するパラメータ
        /// </summary>
        public Dictionary<string, string> PluginParam { get; set; }

        #endregion
    }
}
