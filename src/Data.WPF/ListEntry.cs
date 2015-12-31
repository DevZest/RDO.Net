using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public class ListEntry : GridEntry
    {
        internal static ListEntry Create<T>()
            where T : UIElement, new()
        {
            return new ListEntry(() => new T());
        }

        internal ListEntry(Func<UIElement> constructor)
            : base(constructor)
        {
        }
    }
}
