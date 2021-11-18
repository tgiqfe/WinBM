using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Lib
{
    class PredefinedAccount
    {
        /// <summary>
        /// 事前定義アカウント。初回呼び出し時にセット
        /// </summary>
        private static Dictionary<string, string> _Account = null;

        /// <summary>
        /// 事前定義アカウントを解決
        /// </summary>
        /// <param name="account"></param>
        public static string Resolv(string account)
        {
            _Account ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Administrators", "BUILTIN\\Administrators" },
                { Environment.MachineName + "\\Administrators", "BUILTIN\\Administrators" },
            };

            if (!account.Contains("\\"))
            {
                account = Environment.MachineName + "\\" + account;
            }
            if (_Account.ContainsKey(account))
            {
                account = _Account[account];
            }
            return account;
        }
    }
}
