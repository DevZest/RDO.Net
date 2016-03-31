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

            public Builder<T> Repeat(bool value)
            {
                Item.Repeatable = value;
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

        public bool Repeatable { get; private set; }

        internal bool CrossRepeatable
        {
            get
            {
                if (!Repeatable)
                    return false;
                else if (Template.CrossRepeatable(Orientation.Horizontal))
                    return Template.RowRange.Contains(GridRange.Left) && Template.RowRange.Contains(GridRange.Right);
                else if (Template.CrossRepeatable(Orientation.Vertical))
                    return Template.RowRange.Contains(GridRange.Top) && Template.RowRange.Contains(GridRange.Bottom);
                else
                    return false;
            }
        }

        internal int AccumulatedCrossRepeatsDelta { get; set; }
    }
}
