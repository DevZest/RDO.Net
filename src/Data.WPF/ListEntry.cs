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

        public sealed class Builder<T> : GridEntry.Builder<T, ListEntry, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(DataSetPresenterBuilderRange builderRange)
                : base(builderRange, ListEntry.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override DataSetPresenterBuilder End(DataSetPresenterBuilderRange builderRange, ListEntry entry)
            {
                return builderRange.ListEntry(entry);
            }
        }
    }
}
