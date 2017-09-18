using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data.Helpers
{
    public sealed class SimpleModelKey : KeyBase
    {
        public SimpleModelKey(_Int32 id)
        {
            Id = id;
        }

        public _Int32 Id { get; private set; }
    }
}
