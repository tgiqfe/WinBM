using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBM.Task
{
    public class ValuesAttribute : Attribute
    {
        private string[][] _Candidate { get; set; }

        public string Default { get; set; }

        public ValuesAttribute(params string[] candidate)
        {
            this._Candidate = new string[candidate.Length][];
            for (int i = 0; i < _Candidate.Length; i++)
            {
                _Candidate[i] = candidate[i].Split(',').Select(x => x.Trim()).ToArray();
            }
        }

        public string[][] GetCandidate()
        {
            return this._Candidate;
        }
    }
}
