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
            : base(ListEntry.Create<T>())
        {
            _builderRange = builderRange;
        }

        internal override ListEntryBuilder<T> This
        {
            get { return this; }
        }

        private DataSetPresenterBuilderRange _builderRange;
        public override DataSetPresenterBuilder End()
        {
            return _builderRange.ListEntry(Entry);
        }
    }
}
