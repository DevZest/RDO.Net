using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ScalarItem : TemplateItem
    {
        internal static ScalarItem Create<T>()
            where T : UIElement, new()
        {
            return new ScalarItem(() => new T());
        }

        private ScalarItem(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        public ScalarItemRepeatMode RepeatMode { get; private set; }

        public bool IsRepeat
        {
            get { return RepeatMode == ScalarItemRepeatMode.Flow || RepeatMode == ScalarItemRepeatMode.Grow; }
        }

        private void InitRepeatMode(ScalarItemRepeatMode value)
        {
            RepeatMode = value;
        }

        public sealed class Builder<T> : TemplateItem.Builder<T, ScalarItem, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(GridRangeConfig rangeConfig)
                : base(rangeConfig, ScalarItem.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override DataViewBuilder End(GridRangeConfig rangeConfig, ScalarItem item)
            {
                return rangeConfig.End(item);
            }

            public Builder<T> Repeat(ScalarItemRepeatMode value)
            {
                Item.InitRepeatMode(value);
                return this;
            }
        }
    }
}
