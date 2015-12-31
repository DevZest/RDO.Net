using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace DevZest.Data.Windows
{
    public sealed class ListEntryBuilder<T> : GridEntryBuilder<T, ListEntry, ListEntryBuilder<T>>
        where T : UIElement, new()
    {
        internal ListEntryBuilder(DataSetPresenterBuilderRange builderRange)
            : base(builderRange, ListEntry.Create<T>())
        {
        }

        internal override ListEntryBuilder<T> This
        {
            get { return this; }
        }

        internal override DataSetPresenterBuilder End(DataSetPresenterBuilderRange builderRange, ListEntry entry)
        {
            return builderRange.ListEntry(entry);
        }
    }
}
