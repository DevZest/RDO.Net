using System;
using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IModelSet : IReadOnlyList<Model>
    {
        bool Contains(Model model);
    }
}
