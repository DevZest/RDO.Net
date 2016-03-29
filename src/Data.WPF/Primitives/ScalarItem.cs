using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
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
            FlowMode = FlowMode.Repeat;
        }

        public FlowMode FlowMode { get; private set; }

        internal bool IsRepeat
        {
            get
            {
                if (FlowMode == FlowMode.Stretch)
                    return false;
                else if (Template.RepeatOrientation == RepeatOrientation.XY)
                    return Template.RepeatRange.Contains(GridRange.Left) && Template.RepeatRange.Contains(GridRange.Right);
                else if (Template.RepeatOrientation == RepeatOrientation.YX)
                    return Template.RepeatRange.Contains(GridRange.Top) && Template.RepeatRange.Contains(GridRange.Bottom);
                else
                    return false;
            }
        }

        public sealed class Builder<T> : TemplateItem.Builder<T, ScalarItem, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(GridRangeBuilder rangeConfig)
                : base(rangeConfig, ScalarItem.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override TemplateBuilder End(GridRangeBuilder rangeConfig, ScalarItem item)
            {
                return rangeConfig.End(item);
            }

            public Builder<T> Flow(FlowMode value)
            {
                Item.FlowMode = value;
                return this;
            }
        }
    }
}
