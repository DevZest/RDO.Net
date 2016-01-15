using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows
{
    internal interface IElementCollection : IList<UIElement>, IReadOnlyList<UIElement>
    {
        FrameworkElement Parent { get; }

        void RemoveRange(int index, int count);
    }
}
