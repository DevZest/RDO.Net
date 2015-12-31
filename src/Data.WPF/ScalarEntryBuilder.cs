using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ScalarEntryBuilder<T> : GridEntryBuilder<T, ScalarEntry, ScalarEntryBuilder<T>>
        where T : UIElement, new()
    {
        internal ScalarEntryBuilder(DataSetPresenterBuilderRange builderRange)
            : base(builderRange, ScalarEntry.Create<T>())
        {
        }

        internal override ScalarEntryBuilder<T> This
        {
            get { return this; }
        }

        internal override DataSetPresenterBuilder End(DataSetPresenterBuilderRange builderRange, ScalarEntry entry)
        {
            return builderRange.ScalarEntry(entry);
        }

        public ScalarEntryBuilder<T> FlowMode(FlowMode value)
        {
            Entry.InitFlowMode(value);
            return this;
        }
    }
}
