using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace DevZest.Data.Windows
{
    public sealed class ListEntryBuilder<T> : GridEntryBuilder<T, ListEntry, ListEntryBuilder<T>>
        where T : UIElement, new()
    {
        internal ListEntryBuilder(GridRangeBuilder gridRangeBuilder)
            : base(ListEntry.Create<T>())
        {
            _gridRangeBuilder = gridRangeBuilder;
        }

        internal override ListEntryBuilder<T> This
        {
            get { return this; }
        }

        private GridRangeBuilder _gridRangeBuilder;
        public override DataSetPresenterBuilder End()
        {
            return _gridRangeBuilder.ListEntry(Entry);
        }
    }
}
