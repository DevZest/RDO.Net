using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class DataItem : DataItemBase
    {
        public sealed class Builder<T> : DataItemBase.Builder<T, DataItem, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(GridRangeBuilder rangeConfig)
                : base(rangeConfig, DataItem.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override TemplateBuilder End(GridRangeBuilder rangeConfig, DataItem item)
            {
                return rangeConfig.End(item);
            }

            public Builder<T> Multidimensioinal(bool value)
            {
                Item.IsMultidimensional = value;
                return this;
            }
        }

        internal static DataItem Create<T>()
            where T : UIElement, new()
        {
            return new DataItem(() => new T());
        }

        private DataItem(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        public bool IsMultidimensional { get; private set; }

        internal int AccumulatedStackDimensionsDelta { get; set; }

        internal override void VerifyGridRange(GridRange rowRange)
        {
            if (GridRange.IntersectsWith(rowRange))
                throw new InvalidOperationException(Strings.DataItem_IntersectsWithRowRange(Ordinal));

            if (!IsMultidimensional)
                return;

            if (Template.IsMultidimensional(Orientation.Horizontal))
            {
                if (!rowRange.Contains(GridRange.Left) || !rowRange.Contains(GridRange.Right))
                    throw new InvalidOperationException(Strings.DataItem_OutOfHorizontalRowRange(Ordinal));
            }
            else if (Template.IsMultidimensional(Orientation.Vertical))
            {
                if (!rowRange.Contains(GridRange.Top) || !rowRange.Contains(GridRange.Bottom))
                    throw new InvalidOperationException(Strings.DataItem_OutOfVerticalRowRange(Ordinal));
            }
            else
                throw new InvalidOperationException(Strings.DataItem_NonMultidimensionalTemplate(Ordinal));
        }
    }
}
