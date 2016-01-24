using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public class ListItem : TemplateItem
    {
        internal static ListItem Create<T>()
            where T : UIElement, new()
        {
            return new ListItem(() => new T());
        }

        internal ListItem(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        public sealed class Builder<T> : TemplateItem.Builder<T, ListItem, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(GridRangeConfig rangeConfig)
                : base(rangeConfig, ListItem.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override DataViewBuilder End(GridRangeConfig rangeConfig, ListItem item)
            {
                return rangeConfig.End(item);
            }
        }
    }
}
