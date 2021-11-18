using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace AuditMonitor.Arguments
{
    class ArgumentParameter
    {
        public bool Enabled { get; set; }

        #region Argument Parameter

        /// <summary>
        /// 監視対象ファイル
        /// </summary>
        [ArgsParam]
        private MonitorTarget _MonitorTarget { get; set; } = new MonitorTarget();
        public string MonitorTarget { get { return this._MonitorTarget.ToString(); } }

        /// <summary>
        /// 起動時に監視対象のファイルをリセットするかどうか
        /// </summary>
        [ArgsParam]
        private ResetMonitorTarget _ResetMonitorTarget { get; set; }
        public bool ResetMonitorTarget { get { return this._ResetMonitorTarget?.Enabled ?? false; } }

        #endregion

        public ArgumentParameter() { }
        public ArgumentParameter(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "/t":
                    case "-t":
                        if (i + 1 < args.Length)
                        {
                            this._MonitorTarget = new MonitorTarget(args[++i]);
                        }
                        break;
                    case "/r":
                    case "-r":
                        this._ResetMonitorTarget = new ResetMonitorTarget(true);
                        break;
                }
            }
            this.Enabled = CheckParam();
        }

        private bool CheckParam()
        {
            bool ret = true;

            var props = this.GetType().GetProperties(
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                var argParam = prop.GetCustomAttribute<ArgsParamAttribute>();
                if (argParam == null)
                {
                    continue;
                }

                if (argParam.Mandatory)
                {
                    object obj = prop.GetValue(this);
                    ret &= IsDefined(obj);
                }
            }

            return ret;
        }

        private bool IsDefined(object obj)
        {
            switch (obj)
            {
                case string s:
                    return !string.IsNullOrEmpty(s);
                case string[] ar:
                    return (ar?.Length > 0);
                default:
                    return obj != null;
            }
        }
    }
}
