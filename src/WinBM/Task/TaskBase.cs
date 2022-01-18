using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using WinBM.Lib;

namespace WinBM.Task
{
    public class TaskBase
    {
        /// <summary>
        /// Page。
        /// Rancherで呼び出し時にセット
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// spec名。Pageの各specのNameパラメータ。
        /// Rancherで呼び出し時にセット
        /// </summary>
        public string SpecName { get; set; }

        /// <summary>
        /// specの種類
        /// Init / Config / Output / Require / Workの5種類
        /// Rancherで呼び出し時にセット
        /// </summary>
        public string SpecType { get; set; }

        /// <summary>
        /// privateで格納するTask名
        /// </summary>
        private string _TaskName = null;

        /// <summary>
        /// specの種類、namespaceのトップとボトムの名前、クラス名
        /// 例)
        /// specの種類 ⇒ Output
        /// namespace ⇒ Standard.Output.Console
        /// class名 ⇒ StdOut
        /// の場合、(Output)[Standard/Console/StdOut]
        /// </summary>
        public string TaskName
        {
            get
            {
                if (this._TaskName == null)
                {
                    Type type = this.GetType();
                    string[] namespaces = type.Namespace.Split('.');
                    this._TaskName = $"({this.SpecType})[{namespaces[0]}/{namespaces.Last()}/{type.Name}]";
                }
                return this._TaskName;
            }
        }

        /// <summary>
        /// セッション用設定の管理用
        /// </summary>
        public SessionManager Manager { get; set; }

        /// <summary>
        /// Recipeファイルのパス
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Recipe内のページ番号
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// OutputとRequireでの実行結果を格納
        /// - Initの場合のSuccess ⇒ -
        /// - Configの場合のSuccess ⇒ -
        /// - Outputの場合のSuccess ⇒ パラメータに問題無し。OutputManagerに追加する
        /// - Requireの場合のSuccess ⇒ 事前条件チェックに成功。
        /// - Workの場合のSuccess ⇒ Jobを実行した結果が成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 別クラスのエイリアスかどうか
        /// </summary>
        protected virtual bool IsAlias { get { return false; } }

        #region Post methods

        /// <summary>
        /// Recipe内の全Pageが終了した後に実行する。環境の掃除等
        /// </summary>
        public virtual void PostRecipe() { }

        /// <summary>
        /// PostPageするかどうかの判定
        /// </summary>
        public bool IsPostRecipe { get; set; }

        /// <summary>
        /// Page内の全Specが完了した後に実行する。Job実行後の掃除等
        /// </summary>
        public virtual void PostPage() { }

        /// <summary>
        /// PostSpecするかどうかの判定
        /// </summary>
        public bool IsPostPage { get; set; }

        #endregion
        #region Set Parameter

        /// <summary>
        /// パラメータのセット
        /// </summary>
        /// <param name="param"></param>
        public virtual void SetParam(Dictionary<string, string> param)
        {
            var props = this.GetType().GetProperties(
                IsAlias ?
                    BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance :
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);

            foreach (var prop in props)
            {
                //  TaskParameter属性がない場合は無視
                var paramAttr = prop.GetCustomAttribute<TaskParameterAttribute>();
                if (paramAttr == null) { continue; }

                if (param == null)
                {
                    prop.SetValue(this, null);
                }
                else
                {
                    var keys = prop.GetCustomAttribute<KeysAttribute>();
                    string matchKey = keys?.GetCandidate().FirstOrDefault(x => param.ContainsKey(x));

                    if (matchKey == null)
                    {
                        prop.SetValue(this, null);
                    }
                    else
                    {
                        Type type = prop.PropertyType;

                        string matchValue = param[matchKey];

                        if (type == typeof(string))
                        {
                            //  環境変数の解決
                            if (paramAttr.Resolv)
                            {
                                matchValue = ExpandEnvironment(matchValue);
                            }
                            prop.SetValue(this, matchValue);
                        }
                        else if (type == typeof(bool))
                        {
                            prop.SetValue(this, !falseCandidate.Any(x =>
                                x.Equals(matchValue, StringComparison.OrdinalIgnoreCase)));
                        }
                        else if (type == typeof(bool?))
                        {
                            //  不正な値の場合はtrue
                            prop.SetValue(this, !falseCandidate.Any(x =>
                                x.Equals(matchValue, StringComparison.OrdinalIgnoreCase)));
                        }
                        else if (type == typeof(int?))
                        {
                            //  不正な値、且つ計算させない場合はnullになる。以降の数値型も同じ
                            if (int.TryParse(matchValue, out int tempInt))
                            {
                                if (!paramAttr.Unsigned || tempInt >= 0)
                                {
                                    prop.SetValue(this, tempInt);
                                }
                            }
                            else if (paramAttr.Resolv)
                            {
                                matchValue = ExpandEnvironment(matchValue);
                                var num = CalculateText.ToInt(matchValue, nullable: true);
                                if (num != null && (!paramAttr.Unsigned || num >= 0))
                                {
                                    prop.SetValue(this, num);
                                }
                            }
                        }
                        else if (type == typeof(long?))
                        {
                            if (long.TryParse(matchValue, out long tempLong))
                            {
                                if (!paramAttr.Unsigned || tempLong >= 0)
                                {
                                    prop.SetValue(this, tempLong);
                                }
                            }
                            else if (paramAttr.Resolv)
                            {
                                matchValue = ExpandEnvironment(matchValue);
                                var num = CalculateText.ToLong(matchValue, nullable: true);
                                if (num != null && (!paramAttr.Unsigned || num >= 0))
                                {
                                    prop.SetValue(this, num);
                                }
                            }
                        }
                        else if (type == typeof(double?))
                        {
                            if (double.TryParse(matchValue, out double tempDouble))
                            {
                                if (!paramAttr.Unsigned || tempDouble >= 0)
                                {
                                    prop.SetValue(this, tempDouble);
                                }
                            }
                            else if (paramAttr.Resolv)
                            {
                                matchValue = ExpandEnvironment(matchValue);
                                var num = CalculateText.ToDouble(matchValue, nullable: true);
                                if (num != null && (!paramAttr.Unsigned || num >= 0))
                                {
                                    prop.SetValue(this, num);
                                }
                            }
                        }
                        else if (type == typeof(DateTime?))
                        {
                            prop.SetValue(this, DateTime.TryParse(matchValue, out DateTime dt) ? dt : null);
                        }
                        else if (type == typeof(string[]))
                        {
                            char delimiter = matchValue.Contains(paramAttr.Delimiter) ?
                                paramAttr.Delimiter : '\n';
                            string[] array = matchValue.Split(delimiter).
                                Select(x => x.Trim()).
                                Where(x => x != null).
                                ToArray();

                            //  環境変数の解決
                            if (paramAttr.Resolv)
                            {
                                array = array.Select(x => ExpandEnvironment(x)).ToArray();
                            }
                            prop.SetValue(this, array);
                        }
                        else if (type == typeof(Dictionary<string, string>))
                        {
                            char delimiter = matchValue.Contains(paramAttr.Delimiter) ?
                                paramAttr.Delimiter : '\n';
                            string[] array = matchValue.Split(delimiter).
                                Select(x => x.Trim()).
                                Where(x => !string.IsNullOrEmpty(x)).
                                ToArray();

                            //  環境変数の解決
                            if (paramAttr.Resolv)
                            {
                                array = array.Select(x => ExpandEnvironment(x)).ToArray();
                            }

                            var dictionary = new Dictionary<string, string>();
                            foreach (string item in array.Where(x => x.Contains(paramAttr.EqualSign)))
                            {
                                string itemKey = item.Substring(0, item.IndexOf(paramAttr.EqualSign)).Trim();
                                string itemValue = item.Substring(item.IndexOf(paramAttr.EqualSign) + 1).Trim();
                                dictionary[itemKey] = itemValue;
                            }
                            prop.SetValue(this, dictionary);
                        }
                        else if (type.IsEnum)
                        {
                            //  通常のEnum型
                            var values = prop.GetCustomAttribute<ValuesAttribute>();
                            if (values == null)
                            {
                                prop.SetValue(this, Activator.CreateInstance(type));
                            }
                            else
                            {
                                bool unchanged = true;
                                foreach (string[] candidate in values.GetCandidate())
                                {
                                    if (candidate.Any(x => x.Equals(matchValue, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        //  ValuesAttributeのcandidate[0]を、設定する予定のenumの値名に合わせる必要有り。
                                        prop.SetValue(this, Enum.Parse(type, candidate[0], true));
                                        unchanged = false;
                                    }
                                }
                                if (unchanged)
                                {
                                    prop.SetValue(this,
                                        values.Default == null ?
                                            Activator.CreateInstance(type) :
                                            Enum.Parse(type, values.Default, true));
                                }
                            }
                        }
                        else if ((type = Nullable.GetUnderlyingType(type)).IsEnum)
                        {
                            //  Null許容のEnum型
                            var values = prop.GetCustomAttribute<ValuesAttribute>();
                            if (values == null)
                            {
                                prop.SetValue(this, null);
                            }
                            else
                            {
                                bool unchanged = true;
                                foreach (string[] candidate in values.GetCandidate())
                                {
                                    if (candidate.Any(x => x.Equals(matchValue, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        //  ValuesAttributeのcandidate[0]を、設定する予定のenumの値名に合わせる必要有り。
                                        prop.SetValue(this, Enum.Parse(type, candidate[0], true));
                                        unchanged = false;
                                    }

                                }
                                if (unchanged)
                                {
                                    prop.SetValue(this,
                                        values.Default == null ? null : Enum.Parse(type, values.Default, true));
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
        #region Check Parameter

        /// <summary>
        /// 必要パラメータの有無チェック
        /// </summary>
        public virtual bool CheckParam()
        {
            bool ret = true;

            var props = this.GetType().GetProperties(
                IsAlias ?
                    BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance :
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);

            var mAny = new Dictionary<int, bool>();
            foreach (var prop in props)
            {
                //  TaskParameterAttribute
                var paramAttr = prop.GetCustomAttribute<TaskParameterAttribute>();
                if (paramAttr == null) { continue; }

                object val = prop.GetValue(this);

                if (paramAttr.Mandatory)
                {
                    ret &= IsDefined(val);
                }
                else if (paramAttr.MandatoryAny > 0)
                {
                    //  MandatoryAnyチェック
                    if (!mAny.ContainsKey(paramAttr.MandatoryAny))
                    {
                        mAny[paramAttr.MandatoryAny] = true;
                    }
                    mAny[paramAttr.MandatoryAny] &= IsDefined(val);
                }

                //  ValidateEnumSetAttribute
                var validAttr = prop.GetCustomAttribute<ValidateEnumSetAttribute>();
                if (validAttr != null && val != null)
                {
                    ret &= validAttr.Contains(val.ToString());
                }
            }

            //  MandatoryAny全体チェック
            if (mAny.Count > 0)
            {
                ret &= mAny.Any(x => x.Value);
            }

            //  CheckParam失敗。RequireやWork以外のSpecは、GlobalLogに出力
            if (!ret && (this.SpecType == "Require" || this.SpecType == "Work"))
            {
                this.Manager.WriteLog(LogLevel.Warn, $"{this.TaskName} {this.SpecName} Failed parameter.");
            }
            else
            {
                GlobalLog.WriteLog(LogLevel.Warn, $"{this.TaskName} {this.SpecName} Failed parameter.");
            }

            return ret;
        }

        #endregion
        #region ProcessMethod

        /// <summary>
        /// 処理実行前の事前作業
        /// 基本的に実行する処理についてログを出力する作業のみ。オーバーライドは非推奨
        /// </summary>
        public virtual void PreProcess()
        {
            Manager.WriteLog(LogLevel.Info, $"{this.TaskName} {this.SpecName}");
        }

        /// <summary>
        /// メイン処理。基本的にこのメソッドをオーバーライドして使用
        /// </summary>
        public virtual void MainProcess() { }

        /// <summary>
        /// メイン処理後の作業
        /// 基本的にWriteTask実行のみ。オーバーライドは非推奨
        /// </summary>
        public virtual void PostProcess()
        {
            Manager.WriteTask(this);
        }

        #endregion
        #region Private methods

        /// <summary>
        /// Emptyではないかどうかを判定
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected bool IsDefined(object obj)
        {
            switch (obj)
            {
                case string s:
                    //  stringのempty許可/拒否については検討中。
                    //  変更の可能性有りの為、defualtに統合はまだ行わない。
                    return s != null;
                case string[] ar:
                    return (ar?.Length > 0);
                default:
                    return obj != null;
            }
        }

        /// <summary>
        /// falseと判定できる文字/文字列のセット
        /// </summary>
        protected static string[] falseCandidate = new string[]
        {
            "", "0", "-", "false", "fals", "no", "not", "none", "non", "empty", "null", "否", "不", "無"
        };

        /// <summary>
        /// TaskParameterでResolv=trueだった場合の、環境変数の解決
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected string ExpandEnvironment(string text)
        {
            for (int i = 0; i < 5 && text.Contains("%"); i++)
            {
                if (text.Contains("%DATE_FOR_PATH%", StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Replace("%DATE_FOR_PATH%", DateTime.Now.ToString("yyyyMMdd"), StringComparison.OrdinalIgnoreCase);
                }
                if (text.Contains("%TIME_FOR_PATH%", StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Replace("%TIME_FOR_PATH%", DateTime.Now.ToString("HHmmss"), StringComparison.OrdinalIgnoreCase);
                }
                if (text.Contains("%DATETIME%", StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Replace("%DATETIME%", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), StringComparison.OrdinalIgnoreCase);
                }
                if (text.Contains("%PAGE_NAME%", StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Replace("%PAGE_NAME%", this.PageName, StringComparison.OrdinalIgnoreCase);
                }
                if (text.Contains("%SPEC_NAME%", StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Replace("%SPEC_NAME%", this.SpecName, StringComparison.OrdinalIgnoreCase);
                }
                if (text.Contains("%SPEC_TYPE%", StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Replace("%SPEC_TYPE%", this.SpecType, StringComparison.OrdinalIgnoreCase);
                }
                if (text.Contains("%TASK_NAME%", StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Replace("%TASK_NAME%", this.TaskName, StringComparison.OrdinalIgnoreCase);
                }

                if(text.Contains("%RECIPE_FILE_PATH%", StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Replace("%RECIPE_FILE_PATH%", this.FilePath, StringComparison.OrdinalIgnoreCase);
                }
                if (text.Contains("%RECIPE_FILE_NAME%", StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Replace("%RECIPE_FILE_NAME%", System.IO.Path.GetFileName(this.FilePath), StringComparison.OrdinalIgnoreCase);
                }
                if (text.Contains("%RECIPE_DIRECTORY_PATH%", StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Replace("%RECIPE_DIRECTORY_PATH%", System.IO.Path.GetDirectoryName(this.FilePath), StringComparison.OrdinalIgnoreCase);
                }
                if (text.Contains("%RECIPE_DIRECTORY_NAME%", StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Replace("%RECIPE_DIRECTORY_NAME%", System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(this.FilePath)), StringComparison.OrdinalIgnoreCase);
                }
                if (text.Contains("%PAGE_INDEX%", StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Replace("%PAGE_INDEX%", this.Index.ToString(), StringComparison.OrdinalIgnoreCase);
                }

                if (text.Contains("%TASK_DLL%", StringComparison.OrdinalIgnoreCase))
                {
                    string[] namespaceArray = this.GetType().Namespace.Split('.');
                    text = namespaceArray.Length >= 1 ?
                        text.Replace("%TASK_DLL%", namespaceArray[0], StringComparison.OrdinalIgnoreCase) :
                        "";
                }
                if (text.Contains("%TASK_CATEGORY%", StringComparison.OrdinalIgnoreCase))
                {
                    string[] namespaceArray = this.GetType().Namespace.Split('.');
                    text = namespaceArray.Length >= 2 ?
                        text.Replace("%TASK_CATEGORY%", namespaceArray[2], StringComparison.OrdinalIgnoreCase) :
                        "";
                }
                if (text.Contains("%TASK_CLASS%", StringComparison.OrdinalIgnoreCase))
                {
                    text = text.Replace("%TASK_CLASS%", this.GetType().Name, StringComparison.OrdinalIgnoreCase);
                }

                FileScope.FileScopeList?.
                    Where(x => x.IsMathPath(this.FilePath)).
                    ToList().
                    ForEach(x => x.Resolv(ref text));

                text = Environment.ExpandEnvironmentVariables(text);
            }
            return text;
        }

        #endregion
    }
}