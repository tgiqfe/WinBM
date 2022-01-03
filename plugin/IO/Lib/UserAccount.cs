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

        public UserAccount(string name)
        {
            if (name.Contains("\\"))
            {
                this.Name = name.Substring(name.IndexOf("\\") + 1);
                this.Domain = name.Substring(0, name.IndexOf("\\"));
            }
            else
            {
                this.Name = name;
            }
        }
        public UserAccount(NTAccount ntAccount) : this(ntAccount.Value) { }

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
    }
}
