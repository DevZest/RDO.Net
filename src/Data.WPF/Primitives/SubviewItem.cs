using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class SubviewItem : RowItem
    {
        public sealed new class Builder<T> : TemplateItem.Builder<T, SubviewItem, Builder<T>>
            where T : DataView, new()
        {
            internal Builder(GridRangeBuilder rangeConfig, Func<RowPresenter, DataPresenter> dataPresenterConstructor)
                : base(rangeConfig, SubviewItem.Create<T>(dataPresenterConstructor))
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override TemplateBuilder End(GridRangeBuilder gridRangeBuilder, SubviewItem item)
            {
                return gridRangeBuilder.End(item);
            }
        }

        internal static SubviewItem Create<T>(Func<RowPresenter, DataPresenter> dataPresenterConstructor)
            where T : DataView, new()
        {
            return new SubviewItem(() => new T(), dataPresenterConstructor);
        }

        private SubviewItem(Func<UIElement> constructor, Func<RowPresenter, DataPresenter> dataPresenterConstructor)
                : base(constructor)
        {
            Debug.Assert(dataPresenterConstructor != null);
            DataPresenterConstructor = dataPresenterConstructor;
        }

        internal int Index { get; private set; }

        internal void Seal(Template owner, GridRange gridRange, int ordinal, int index)
        {
            base.Construct(owner, gridRange, ordinal);
            Index = index;
        }

        internal Func<RowPresenter, DataPresenter> DataPresenterConstructor { get; private set; }

        internal sealed override void Initialize(UIElement element)
        {
            base.Initialize(element);
            var dataView = (DataView)element;
            var parentRow = dataView.GetRowPresenter();
            dataView.Initialize(parentRow.SubviewPresenters[Index]);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            base.Cleanup(element);
            var dataView = (DataView)element;
            dataView.Cleanup();
        }
    }
}
