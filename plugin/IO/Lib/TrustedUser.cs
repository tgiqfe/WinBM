using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Security.Principal;

namespace IO.Lib
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class TrustedUser
    {
        public bool Enabled { get; set; }

        private bool _HasToken { get; set; }

        private bool? IsAdmin { get; set; }

        public TrustedUser()
        {
            _ = GetPrivilege();
        }

        ~TrustedUser()
        {
            if (_HasToken)
            {
                TokenManipulator.RemovePrivilege(TokenManipulator.SE_RESTORE_NAME);
            }
        }

        public bool GetPrivilege()
        {
            CheckAdmin();
            if (IsAdmin ?? false)
            {
                if (!_HasToken)
                {
                    TokenManipulator.AddPrivilege(TokenManipulator.SE_RESTORE_NAME);
                    _HasToken = true;
                }
                Enabled = true;
            }
            return Enabled;
        }

        public void RemovePrivilege()
        {
            TokenManipulator.RemovePrivilege(TokenManipulator.SE_RESTORE_NAME);
            _HasToken = false;
        }

        private void CheckAdmin()
        {
            if (this.IsAdmin == null)
            {
                AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
                WindowsPrincipal wp = (WindowsPrincipal)System.Threading.Thread.CurrentPrincipal;
                this.IsAdmin = wp.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}
