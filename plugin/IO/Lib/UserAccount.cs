using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;

namespace IO.Lib
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class UserAccount
    {
        public string Name { get; set; }
        public string Domain { get; set; }

        private NTAccount _ntAccount = null;
        public NTAccount NTAccount
        {
            get
            {
                _ntAccount ??= string.IsNullOrEmpty(this.Domain) ?
                    new NTAccount(this.Name) :
                    new NTAccount($"{this.Domain}\\{this.Name}");
                return _ntAccount;
            }
        }

        public string FullName
        {
            get
            {
                return string.IsNullOrEmpty(this.Domain) ?
                    $"{this.Domain}\\{this.Name}" :
                    $"{Environment.MachineName}\\{this.Name}";
            }
        }

        public UserAccount(string name)
        {
            if (string.IsNullOrEmpty(name)) { return; }

            string resolvedName = Resolv(name);
            if (resolvedName.Contains("\\"))
            {
                this.Name = resolvedName.Substring(resolvedName.IndexOf("\\") + 1);
                this.Domain = resolvedName.Substring(0, resolvedName.IndexOf("\\"));
            }
            else
            {
                this.Name = resolvedName;
            }
        }
        public UserAccount(NTAccount ntAccount) : this(ntAccount.Value) { }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Domain))
            {
                return this.Name;
            }
            return $"{this.Domain}\\{this.Name}";
        }

        #region IsMatch method

        /// <summary>
        /// UserAccountクラス同士で一致確認。どちらかのDomainの未設定の場合、Nameのみ比較
        /// </summary>
        /// <param name="userAccount"></param>
        /// <returns></returns>
        public bool IsMatch(UserAccount userAccount)
        {
            if (!string.IsNullOrEmpty(this.Domain) && !string.IsNullOrEmpty(userAccount.Domain))
            {
                if (this.Domain.Equals(userAccount.Domain, StringComparison.OrdinalIgnoreCase) &&
                    this.Name.Equals(userAccount.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return this.Name.Equals(userAccount.Name, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsMatch(string name)
        {
            return this.IsMatch(new UserAccount(name));
        }

        public bool IsMatch(NTAccount ntAccount)
        {
            return this.IsMatch(new UserAccount(ntAccount));
        }

        #endregion
        #region Predefined

        /// <summary>
        /// 事前定義アカウント。初回呼び出し時にセット
        /// </summary>
        private static Dictionary<string, string> _preDefinedAccounts = null;

        /// <summary>
        /// 事前定義アカウントを解決
        /// </summary>
        /// <param name="name"></param>
        public static string Resolv(string name)
        {
            _preDefinedAccounts ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Administrators", "BUILTIN\\Administrators" },
                { Environment.MachineName + "\\Administrators", "BUILTIN\\Administrators" },
                { "System", "NT Authority\\System" },
            };

            if (_preDefinedAccounts.ContainsKey(name))
            {
                name = _preDefinedAccounts[name];
            }
            return name;
        }

        #endregion
    }
}
