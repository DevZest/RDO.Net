using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ChildEntry : ListEntry
    {
        public sealed new class Builder<T> : GridEntry.Builder<T, ChildEntry, Builder<T>>
            where T : DataSetView, new()
        {
            internal Builder(DataSetPresenterBuilderRange builderRange, Func<DataRowPresenter, DataSetPresenter> childPresenterConstructor)
                : base(builderRange, ChildEntry.Create<T>(childPresenterConstructor))
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override DataSetPresenterBuilder End(DataSetPresenterBuilderRange builderRange, ChildEntry entry)
            {
                return builderRange.ChildEntry(entry);
            }
        }

        internal static ChildEntry Create<T>(Func<DataRowPresenter, DataSetPresenter> childPresenterConstructor)
            where T : DataSetView, new()
        {
            return new ChildEntry(() => new T(), childPresenterConstructor);
        }

        private ChildEntry(Func<UIElement> constructor, Func<DataRowPresenter, DataSetPresenter> childPresenterConstructor)
                : base(constructor)
        {
            Debug.Assert(childPresenterConstructor != null);
            ChildPresenterConstructor = childPresenterConstructor;
        }

        internal int ChildOrdinal { get; private set; }

        internal void Seal(GridTemplate owner, GridRange gridRange, int ordinal, int childOrdinal)
        {
            base.Seal(owner, gridRange, ordinal);
            ChildOrdinal = childOrdinal;
        }

        internal Func<DataRowPresenter, DataSetPresenter> ChildPresenterConstructor { get; private set; }

        internal sealed override void OnInitialize(UIElement element)
        {
            base.OnInitialize(element);
            var dataSetView = (DataSetView)element;
            var parentDataRowPresenter = dataSetView.GetDataRowPresenter();
            dataSetView.Show(parentDataRowPresenter.Children[ChildOrdinal]);
        }

        internal sealed override void OnCleanup(UIElement element)
        {
            base.OnCleanup(element);
            var dataSetView = (DataSetView)element;
            dataSetView.Cleanup();
        }
    }
}
