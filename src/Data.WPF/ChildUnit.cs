using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ChildUnit : ListUnit
    {
        public sealed new class Builder<T> : TemplateUnit.Builder<T, ChildUnit, Builder<T>>
            where T : DataForm, new()
        {
            internal Builder(GridRangeConfig rangeConfig, Func<RowView, DataView> childViewConstructor)
                : base(rangeConfig, ChildUnit.Create<T>(childViewConstructor))
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override DataViewBuilder End(GridRangeConfig rangeConfig, ChildUnit unit)
            {
                return rangeConfig.End(unit);
            }
        }

        internal static ChildUnit Create<T>(Func<RowView, DataView> childViewConstructor)
            where T : DataForm, new()
        {
            return new ChildUnit(() => new T(), childViewConstructor);
        }

        private ChildUnit(Func<UIElement> constructor, Func<RowView, DataView> childViewConstructor)
                : base(constructor)
        {
            Debug.Assert(childViewConstructor != null);
            ChildViewConstructor = childViewConstructor;
        }

        internal int Index { get; private set; }

        internal void Seal(GridTemplate owner, GridRange gridRange, int ordinal, int index)
        {
            base.Construct(owner, gridRange, ordinal);
            Index = index;
        }

        internal Func<RowView, DataView> ChildViewConstructor { get; private set; }

        internal sealed override void Initialize(UIElement element)
        {
            base.Initialize(element);
            var dataForm = (DataForm)element;
            var parentRow = dataForm.GetRowView();
            dataForm.Show(parentRow.Children[Index]);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            base.Cleanup(element);
            var dataForm = (DataForm)element;
            dataForm.Cleanup();
        }
    }
}
