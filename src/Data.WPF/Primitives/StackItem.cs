using System;
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
    }
}
