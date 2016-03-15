using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public class RepeatItem : TemplateItem
    {
        internal static RepeatItem Create<T>()
            where T : UIElement, new()
        {
            return new RepeatItem(() => new T());
        }

        internal RepeatItem(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        public sealed class Builder<T> : TemplateItem.Builder<T, RepeatItem, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(GridRangeConfig rangeConfig)
                : base(rangeConfig, RepeatItem.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override TemplateBuilder End(GridRangeConfig rangeConfig, RepeatItem item)
            {
                return rangeConfig.End(item);
            }
        }
    }
}
