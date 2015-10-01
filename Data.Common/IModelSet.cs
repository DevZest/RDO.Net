using System;
using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IModelSet : IEnumerable<Model>
    {
        bool Contains(Model model);

        int Count { get; }
    }
}
