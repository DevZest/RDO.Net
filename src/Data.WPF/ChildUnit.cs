using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ChildUnit : ListUnit
    {
        public sealed new class Builder<T> : TemplateUnit.Builder<T, ChildUnit, Builder<T>>
            where T : DataSetView, new()
        {
            internal Builder(GridRangeConfig rangeConfig, Func<DataRowPresenter, DataSetPresenter> childPresenterConstructor)
                : base(rangeConfig, ChildUnit.Create<T>(childPresenterConstructor))
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override DataSetPresenterBuilder End(GridRangeConfig rangeConfig, ChildUnit unit)
            {
                return rangeConfig.End(unit);
            }
        }

        internal static ChildUnit Create<T>(Func<DataRowPresenter, DataSetPresenter> childPresenterConstructor)
            where T : DataSetView, new()
        {
            return new ChildUnit(() => new T(), childPresenterConstructor);
        }

        private ChildUnit(Func<UIElement> constructor, Func<DataRowPresenter, DataSetPresenter> childPresenterConstructor)
                : base(constructor)
        {
            Debug.Assert(childPresenterConstructor != null);
            ChildPresenterConstructor = childPresenterConstructor;
        }

        internal int Index { get; private set; }

        internal void Seal(GridTemplate owner, GridRange gridRange, int ordinal, int index)
        {
            base.Seal(owner, gridRange, ordinal);
            Index = index;
        }

        internal Func<DataRowPresenter, DataSetPresenter> ChildPresenterConstructor { get; private set; }

        internal sealed override void Initialize(UIElement element)
        {
            base.Initialize(element);
            var dataSetView = (DataSetView)element;
            var parentDataRowPresenter = dataSetView.GetDataRowPresenter();
            dataSetView.Show(parentDataRowPresenter.Children[Index]);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            base.Cleanup(element);
            var dataSetView = (DataSetView)element;
            dataSetView.Cleanup();
        }
    }
}
