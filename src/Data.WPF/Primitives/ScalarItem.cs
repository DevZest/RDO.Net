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
        }

        private ScalarRepeatMode _repeatMode = ScalarRepeatMode.None;
        public ScalarRepeatMode RepeatMode
        {
            get
            {
                if (_repeatMode == ScalarRepeatMode.None)
                    return ScalarRepeatMode.None;

                if (_repeatMode == ScalarRepeatMode.Flow)
                    return CoerceFlowRepeatMode();

                Debug.Assert(_repeatMode == ScalarRepeatMode.Grow);
                return CoerceGrowRepeatMode();
            }
        }

        private ScalarRepeatMode CoerceFlowRepeatMode()
        {
            Debug.Assert(_repeatMode == ScalarRepeatMode.Flow);
            bool isValid = false;
            if (Template.RepeatOrientation == RepeatOrientation.XY)
                isValid = Template.RepeatRange.Contains(GridRange.Left) && Template.RepeatRange.Contains(GridRange.Right);
            else if (Template.RepeatOrientation == RepeatOrientation.YX)
                isValid = Template.RepeatRange.Contains(GridRange.Top) && Template.RepeatRange.Contains(GridRange.Bottom);

            return isValid ? ScalarRepeatMode.Flow : ScalarRepeatMode.None;
        }

        private ScalarRepeatMode CoerceGrowRepeatMode()
        {
            Debug.Assert(_repeatMode == ScalarRepeatMode.Grow);
            bool isValid = false;
            if (Template.RepeatOrientation == RepeatOrientation.Y || Template.RepeatOrientation == RepeatOrientation.XY)
                isValid = Template.RepeatRange.Contains(GridRange.Top) && Template.RepeatRange.Contains(GridRange.Bottom);
            else if (Template.RepeatOrientation == RepeatOrientation.X || Template.RepeatOrientation == RepeatOrientation.YX)
                isValid = Template.RepeatRange.Contains(GridRange.Left) && Template.RepeatRange.Contains(GridRange.Right);

            return isValid ? ScalarRepeatMode.Grow : ScalarRepeatMode.None;
        }

        public bool IsRepeatable
        {
            get { return RepeatMode == ScalarRepeatMode.Flow || RepeatMode == ScalarRepeatMode.Grow; }
        }

        private void InitRepeatMode(ScalarRepeatMode value)
        {
            _repeatMode = value;
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

            public Builder<T> Repeat(ScalarRepeatMode value)
            {
                Item.InitRepeatMode(value);
                return this;
            }
        }
    }
}
