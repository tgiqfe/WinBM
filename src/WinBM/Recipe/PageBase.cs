using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBM.Recipe
{
    public class PageBase
    {
        public virtual void PreSerialize() { }

        public virtual void PostDeserialize(string pageName) { }
    }
}
