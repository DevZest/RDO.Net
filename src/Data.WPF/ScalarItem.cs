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
            if (Owner.RepeatOrientation == RepeatOrientation.XY)
                isValid = Owner.RepeatRange.Contains(GridRange.Left) && Owner.RepeatRange.Contains(GridRange.Right);
            else if (Owner.RepeatOrientation == RepeatOrientation.YX)
                isValid = Owner.RepeatRange.Contains(GridRange.Top) && Owner.RepeatRange.Contains(GridRange.Bottom);

            return isValid ? ScalarRepeatMode.Flow : ScalarRepeatMode.None;
        }

        private ScalarRepeatMode CoerceGrowRepeatMode()
        {
            Debug.Assert(_repeatMode == ScalarRepeatMode.Grow);
            bool isValid = false;
            if (Owner.RepeatOrientation == RepeatOrientation.Y || Owner.RepeatOrientation == RepeatOrientation.XY)
                isValid = Owner.RepeatRange.Contains(GridRange.Top) && Owner.RepeatRange.Contains(GridRange.Bottom);
            else if (Owner.RepeatOrientation == RepeatOrientation.X || Owner.RepeatOrientation == RepeatOrientation.YX)
                isValid = Owner.RepeatRange.Contains(GridRange.Left) && Owner.RepeatRange.Contains(GridRange.Right);

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
            internal Builder(GridRangeConfig rangeConfig)
                : base(rangeConfig, ScalarItem.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override DataPresenterBuilder End(GridRangeConfig rangeConfig, ScalarItem item)
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
