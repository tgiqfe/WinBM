using System;
using System.Collections.Generic;
using System.Linq;
using WinBM.Recipe;
using WinBM.Task;
using System.Reflection;
using System.IO;

namespace WinBM
{
    public class Rancher
    {
        private List<Type> _LoadedTypes = Assembly.GetExecutingAssembly().GetTypes().ToList();
        private List<TaskBase> _PostPageList = new List<TaskBase>();

        private SessionManager _Manager = null;

        public Rancher() { }
        public Rancher(SessionManager manager)
        {
            this._Manager = manager;
        }

        #region Process Config

        /// <summary>
        /// Config
        /// </summary>
        public void ConfigProcess(List<Page> list)
        {
            var registeredTask = new List<string>();
            foreach (Page page in list.OrderBy(x => x.Metadata.Priority))
            {
                if (page.Metadata.Skip ?? false)
                {
                    _Manager.Setting.WriteLog(LogLevel.Info, "Skip. MetadataName={0}", page.Metadata.Name);
                    continue;
                }

                var postSpecList = new List<TaskConfig>();

                //  Config
                if (page.Config.Spec != null)
                {
                    foreach (SpecConfig spec in page.Config.Spec)
                    {
                        if (spec.Skip ?? false)
                        {
                            _Manager.Setting.WriteLog(LogLevel.Info, "Skip. SpecName={0}", spec.Name);
                            continue;
                        }

                        bool onStep =
                            _Manager.Stepable && (_Manager.Setting.StepConfig || (page.Metadata.Step ?? false));

                        string tempTaskLabel = spec.Task.ToLower();
                        if (!registeredTask.Contains(tempTaskLabel))
                        {

                            registeredTask.Add(tempTaskLabel);
                            TaskConfig task = Activate<TaskConfig>(spec, "Config");
                            if (task == null)
                            {
                                _Manager.WriteLog(LogLevel.Debug, "Config skip. task name missing. \"{0}\"", spec.Task);
                                continue;
                            }

                            task.Manager = _Manager;
                            task.PageName = page.Metadata.Name;
                            task.SpecName = spec.Name;
                            task.SpecType = "Config";
                            task.SetParam(spec.Param);
                            if (task.CheckParam())
                            {
                                task.PreProcess();
                                if (onStep)
                                {
                                    onStep = false;
                                    Console.ReadLine();
                                }
                                task.MainProcess();
                                task.PostProcess();

                                if (task.IsPostSpec) { postSpecList.Add(task); }
                                if (task.IsPostPage) { _PostPageList.Add(task); }
                            }
                        }

                        if (onStep) { Console.ReadLine(); }
                    }
                }

                //  Page内の全Config実行後の処理
                postSpecList.ForEach(x => x.PostSpec());
            }
        }

        #endregion

        #region Process Output

        /// <summary>
        /// Output
        /// </summary>
        /// <param name="list"></param>
        public void OutputProcess(List<Page> list)
        {
            //  Output
            //  Priority値を昇順にOutputを登録し、Task名が重複した場合は、後から登録しようとしたものを無視
            //  つまり、Priorityの低いほうを優先。
            var registeredTask = new List<string>();
            foreach (Page page in list.OrderBy(x => x.Metadata.Priority))
            {
                if (page.Metadata.Skip ?? false)
                {
                    _Manager.Setting.WriteLog(LogLevel.Info, "Skip. MetadataName={0}", page.Metadata.Name);
                    continue;
                }

                var postSpecList = new List<TaskOutput>();

                //  Output
                if (page.Output.Spec != null)
                {
                    foreach (SpecOutput spec in page.Output.Spec)
                    {
                        if (spec.Skip ?? false)
                        {
                            _Manager.Setting.WriteLog(LogLevel.Info, "Skip. SpecName={0}", spec.Name);
                            continue;
                        }

                        bool onStep =
                            _Manager.Stepable && (_Manager.Setting.StepOutput || (page.Metadata.Step ?? false));

                        string tempTaskLabel = spec.Task.ToLower();
                        if (!registeredTask.Contains(tempTaskLabel))
                        {
                            registeredTask.Add(tempTaskLabel);
                            TaskOutput task = Activate<TaskOutput>(spec, "Output");
                            if (task == null)
                            {
                                _Manager.WriteLog(LogLevel.Debug, "Output skip. task name missing. \"{0}\"", spec.Task);
                                continue;
                            }

                            task.Manager = _Manager;
                            task.PageName = page.Metadata.Name;
                            task.SpecName = spec.Name;
                            task.SpecType = "Output";
                            task.SetParam(spec.Param);
                            if (task.CheckParam())
                            {
                                task.PreProcess();
                                if (onStep)
                                {
                                    onStep = false;
                                    Console.ReadLine();
                                }
                                task.MainProcess();
                                task.PostProcess();

                                if (task.Success) { _Manager.AddOutput(task); }
                                if (task.IsPostSpec) { postSpecList.Add(task); }
                                if (task.IsPostPage) { _PostPageList.Add(task); }
                            }
                        }

                        if (onStep) { Console.ReadLine(); }
                    }
                }

                //  Page内の全Output実行後の処理
                postSpecList.ForEach(x => x.PostSpec());
            }
        }

        #endregion

        #region Procee Job

        /// <summary>
        /// Job (Require, Work)
        /// </summary>
        /// <param name="list"></param>
        public void JobProcess(List<Page> list)
        {
            bool abort = false;
            int jobIndex = 0;
            foreach (Page page in list.OrderBy(x => x.Metadata.Priority))
            {
                _Manager.WriteProgressBar(1, list.Count, ++jobIndex, page.Metadata.Name);

                if (page.Metadata.Skip ?? false)
                {
                    _Manager.Setting.WriteLog(LogLevel.Info, "Skip. MetadataName={0}", page.Metadata.Name);
                    continue;
                }

                bool stop = false;
                var postSpecList = new List<TaskJob>();

                //  Require
                if (page.Job.Require != null)
                {
                    foreach (SpecJob spec in page.Job.Require)
                    {
                        if (spec.Skip ?? false)
                        {
                            _Manager.Setting.WriteLog(LogLevel.Info, "Skip. SpecName={0}", spec.Name);
                            continue;
                        }

                        bool onStep =
                            _Manager.Stepable && (_Manager.Setting.StepRequire || (page.Metadata.Step ?? false));

                        TaskJob task = Activate<TaskJob>(spec, "Require");
                        if (task == null)
                        {
                            //  Requireでは、Failedがnullの場合はStop
                            if (spec.Failed == SpecJob.FailedAction.Stop || spec.Failed == null)
                            {
                                _Manager.WriteLog(LogLevel.Debug, "Require stop. task name missing. \"{0}\"", spec.Task);
                                stop = true;
                                break;
                            }
                            else if (spec.Failed == SpecJob.FailedAction.Abort)
                            {
                                _Manager.WriteLog(LogLevel.Debug, "Require abort. task name missing. \"{0}\"", spec.Task);
                                abort = true;
                                break;
                            }
                            continue;
                        }

                        task.Manager = _Manager;
                        task.PageName = page.Metadata.Name;
                        task.SpecName = spec.Name;
                        task.SpecType = "Require";
                        task.SetParam(spec.Param);
                        if (task.CheckParam())
                        {
                            task.PreProcess();
                            if (onStep)
                            {
                                onStep = false;
                                Console.ReadLine();
                            }
                            task.MainProcess();
                            task.PostProcess();

                            if (!task.Success)
                            {
                                //  Requireでは、Failedがnullの場合はStop
                                if (spec.Failed == SpecJob.FailedAction.Stop || spec.Failed == null)
                                {
                                    _Manager.WriteLog(LogLevel.Attention, "Require stop.");
                                    stop = true;
                                    break;
                                }
                                else if (spec.Failed == SpecJob.FailedAction.Abort)
                                {
                                    _Manager.WriteLog(LogLevel.Attention, "Require abort.");
                                    abort = true;
                                    break;
                                }
                            }
                            if (task.IsPostSpec) { postSpecList.Add(task); }
                            if (task.IsPostPage) { _PostPageList.Add(task); }
                        }

                        if (onStep) { Console.ReadLine(); }
                    }
                }
                if (stop) { continue; }
                if (abort) { break; }

                //  Work
                if (page.Job.Work != null)
                {
                    //  workのどれか一つでProgress=trueの場合、work単位でプログレスバー表示
                    bool viewProgress = page.Job.Work.Any(x => x.Progress ?? false);
                    int workIndex = 0;

                    foreach (SpecJob spec in page.Job.Work)
                    {
                        if (viewProgress)
                        {
                            _Manager.WriteProgressBar(2, page.Job.Work.Length, ++workIndex, spec.Name);
                            System.Threading.Thread.Sleep(100);
                        }
                        if (spec.Skip ?? false)
                        {
                            _Manager.Setting.WriteLog(LogLevel.Info, "Skip. SpecName={0}", spec.Name);
                            continue;
                        }

                        bool onStep =
                            _Manager.Stepable && (_Manager.Setting.StepWork || (page.Metadata.Step ?? false));

                        TaskJob task = Activate<TaskJob>(spec, "Work");
                        if (task == null)
                        {
                            //  Workでは、Failedがnullの場合はContinue;
                            if (spec.Failed == SpecJob.FailedAction.Stop)
                            {
                                _Manager.WriteLog(LogLevel.Debug, "Work stop. task name missing. \"{0}\"", spec.Task);
                                stop = true;
                                break;
                            }
                            else if (spec.Failed == SpecJob.FailedAction.Abort)
                            {
                                _Manager.WriteLog(LogLevel.Debug, "Work stop. task name missing. \"{0}\"", spec.Task);
                                abort = true;
                                break;
                            }
                            continue;
                        }

                        task.Manager = _Manager;
                        task.PageName = page.Metadata.Name;
                        task.SpecName = spec.Name;
                        task.SpecType = "Work";
                        task.SetParam(spec.Param);
                        if (task.CheckParam())
                        {
                            task.PreProcess();
                            if (onStep)
                            {
                                //  CheckParamに成功する場合はこの箇所で一時停止。失敗した場合は後で一時停止
                                onStep = false;
                                Console.ReadLine();
                            }
                            task.MainProcess();
                            task.PostProcess();

                            if (!task.Success)
                            {
                                //  Workでは、Failedがnullの場合はContinue;
                                if (spec.Failed == SpecJob.FailedAction.Stop)
                                {
                                    _Manager.WriteLog(LogLevel.Attention, "Work stop.");
                                    stop = true;
                                    break;
                                }
                                else if (spec.Failed == SpecJob.FailedAction.Abort)
                                {
                                    _Manager.WriteLog(LogLevel.Attention, "Work abort.");
                                    abort = true;
                                    break;
                                }
                            }
                            if (task.IsPostSpec) { postSpecList.Add(task); }
                            if (task.IsPostPage) { _PostPageList.Add(task); }
                        }

                        if (onStep) { Console.ReadLine(); }
                    }
                }
                if (abort) { break; }

                //  Page内の全Job実行後の処理
                postSpecList.ForEach(x => x.PostSpec());
            }
        }

        #endregion

        /// <summary>
        /// 全Page終了後の処理
        /// </summary>
        public void PostPageProcess()
        {
            _PostPageList.ForEach(x => x.PostPage());
        }

        #region Activate

        /// <summary>
        /// SpecBaseからTaskBaseを生成して、Output/Require/Work用の処理を実行する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="spec"></param>
        /// <param name="specType"></param>
        /// <returns></returns>
        private T Activate<T>(SpecBase spec, string specType) where T : TaskBase
        {
            string[] types = spec.Task?.Split('/');
            if (types == null)
            {
                //  Task指定が正しくない場合
                return null;
            }
            switch (types.Length)
            {
                case < 2:
                    //  Task指定が正しくない('/'で分割して、要素数が2未満だった場合)
                    return null;
                case 2:
                    //  Taskの値が2つのみの場合は、組み込みクラスを使用する。Namespaceでの判定は若干不安ですが・・・
                    //  [WinBM/category/class]のような形式になるはず。正し、現在は組み込みクラスは未実装である為、今後この仕様は廃止の可能性大
                    types = new string[3] { this.GetType().Namespace, types[0], types[1] };
                    break;
            }
            string dllName = types[0];
            string categoryName = types[1];
            string className = types[2];

            string typeName = $"{dllName}.{specType}.{categoryName}.{className}";
            Type type = _LoadedTypes.
                FirstOrDefault(x => x.FullName.Equals(typeName, StringComparison.OrdinalIgnoreCase));

            if (type == null)
            {
                try
                {
                    Assembly asm = null;

                    if (_Manager.Setting.PluginFiles?.Length > 0)
                    {
                        //  PluginFilesから探して読み込み
                        string dllFile = _Manager.Setting.PluginFiles.FirstOrDefault(x =>
                            x.EndsWith(dllName + ".dll", StringComparison.OrdinalIgnoreCase));
                        if (dllFile != null)
                        {
                            asm = Assembly.LoadFrom(dllFile);
                        }
                    }
                    else if (!string.IsNullOrEmpty(_Manager.Setting.PluginDirectory))
                    {
                        //  PluginDirectoryから探して読み込み
                        string dllFile = Directory.GetFiles(_Manager.Setting.PluginDirectory).FirstOrDefault(x =>
                            x.EndsWith(dllName + ".dll", StringComparison.OrdinalIgnoreCase));
                        if (dllFile != null)
                        {
                            asm = Assembly.LoadFrom(dllFile);
                        }
                    }
                    else
                    {
                        //  カレントディレクトリ内のpluginフォルダーから読み込み
                        string tempDllPath = Path.Combine(
                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            "plugin",
                            $"{dllName}.dll");
                        //asm = Assembly.LoadFrom($"plugin\\{dllName}.dll");
                        asm = Assembly.LoadFrom(tempDllPath);
                    }
                    Module module = asm.GetModule($"{dllName}.dll");
                    type = module.GetType(typeName, true);
                    _LoadedTypes.AddRange(module.GetTypes());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (type == null)
            {
                _Manager.Setting.WriteLog(LogLevel.Warn, "Failed activate. \"{0}\"", typeName);
                return null;
            }
            return Activator.CreateInstance(type) as T;
        }

        #endregion
    }
}
