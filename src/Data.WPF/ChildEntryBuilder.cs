using System;

namespace DevZest.Data.Windows
{
    public sealed class ChildEntryBuilder<T> : GridEntryBuilder<T, ChildEntry, ChildEntryBuilder<T>>
        where T : DataSetView, new()
    {
        internal ChildEntryBuilder(DataSetPresenterBuilderRange builderRange, Func<DataRowPresenter, DataSetPresenter> childPresenterConstructor)
            : base(builderRange, ChildEntry.Create<T>(childPresenterConstructor))
        {
        }

        internal override ChildEntryBuilder<T> This
        {
            get { return this; }
        }

        internal override DataSetPresenterBuilder End(DataSetPresenterBuilderRange builderRange, ChildEntry entry)
        {
            return builderRange.ChildEntry(entry);
        }
    }
}
