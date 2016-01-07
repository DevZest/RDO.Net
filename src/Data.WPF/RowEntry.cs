using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public class RowEntry : GridEntry
    {
        internal static RowEntry Create<T>()
            where T : UIElement, new()
        {
            return new RowEntry(() => new T());
        }

        internal RowEntry(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        public sealed class Builder<T> : GridEntry.Builder<T, RowEntry, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(DataSetPresenterBuilderRange builderRange)
                : base(builderRange, RowEntry.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override DataSetPresenterBuilder End(DataSetPresenterBuilderRange builderRange, RowEntry entry)
            {
                return builderRange.RowEntry(entry);
            }
        }
    }
}
