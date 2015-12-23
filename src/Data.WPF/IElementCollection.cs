using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows
{
    internal interface IElementCollection : IList<UIElement>, IReadOnlyList<UIElement>
    {
    }
}
