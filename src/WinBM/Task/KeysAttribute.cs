using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBM.Task
{
    public class KeysAttribute : Attribute
    {
        private string[] _Candidate { get; set; }

        public KeysAttribute(params string[] candidate)
        {
            this._Candidate = candidate;
        }

        public string[] GetCandidate()
        {
            return this._Candidate;
        }
    }
}
