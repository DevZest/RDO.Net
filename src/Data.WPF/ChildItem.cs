using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ChildItem : RepeatItem
    {
        public sealed new class Builder<T> : TemplateItem.Builder<T, ChildItem, Builder<T>>
            where T : DataView, new()
        {
            internal Builder(GridRangeConfig rangeConfig, Func<RowPresenter, DataPresenter> childPresenterConstructor)
                : base(rangeConfig, ChildItem.Create<T>(childPresenterConstructor))
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override DataPresenterBuilder End(GridRangeConfig rangeConfig, ChildItem item)
            {
                return rangeConfig.End(item);
            }
        }

        internal static ChildItem Create<T>(Func<RowPresenter, DataPresenter> childPresenterConstructor)
            where T : DataView, new()
        {
            return new ChildItem(() => new T(), childPresenterConstructor);
        }

        private ChildItem(Func<UIElement> constructor, Func<RowPresenter, DataPresenter> childPresenterConstructor)
                : base(constructor)
        {
            Debug.Assert(childPresenterConstructor != null);
            ChildPresenterConstructor = childPresenterConstructor;
        }

        internal int Index { get; private set; }

        internal void Seal(GridTemplate owner, GridRange gridRange, int ordinal, int index)
        {
            base.Construct(owner, gridRange, ordinal);
            Index = index;
        }

        internal Func<RowPresenter, DataPresenter> ChildPresenterConstructor { get; private set; }

        internal sealed override void Initialize(UIElement element)
        {
            base.Initialize(element);
            var dataView = (DataView)element;
            var parentRow = dataView.GetRowPresenter();
            dataView.Show(parentRow.ChildDataPresenters[Index]);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            base.Cleanup(element);
            var dataView = (DataView)element;
            dataView.Cleanup();
        }
    }
}
