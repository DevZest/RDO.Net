using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class StackItem : DataItemBase
    {
        public sealed class Builder<T> : DataItemBase.Builder<T, StackItem, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(GridRangeBuilder rangeConfig)
                : base(rangeConfig, StackItem.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override TemplateBuilder End(GridRangeBuilder rangeConfig, StackItem item)
            {
                return rangeConfig.End(item);
            }
        }

        internal static StackItem Create<T>()
            where T : UIElement, new()
        {
            return new StackItem(() => new T());
        }

        private StackItem(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        internal override void VerifyGridRange(GridRange rowRange)
        {
            if (GridRange.IntersectsWith(rowRange))
                throw new InvalidOperationException(Strings.StackItem_IntersectsWithRowRange(Ordinal));

            if (!Template.StackOrientation.HasValue)
                throw new InvalidOperationException(Strings.StackItem_NullStackOrientation);

            var orientation = Template.StackOrientation.GetValueOrDefault();
            if (orientation == Orientation.Horizontal)
            {
                if (!rowRange.Contains(GridRange.Left) || !rowRange.Contains(GridRange.Right))
                    throw new InvalidOperationException(Strings.StackItem_OutOfHorizontalRowRange(Ordinal));
            }
            else
            {
                Debug.Assert(orientation == Orientation.Vertical);
                if (!rowRange.Contains(GridRange.Top) || !rowRange.Contains(GridRange.Bottom))
                    throw new InvalidOperationException(Strings.DataItem_OutOfVerticalRowRange(Ordinal));
            }
        }
    }
}
