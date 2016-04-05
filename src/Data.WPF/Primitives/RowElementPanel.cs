using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    public class RowElementPanel : FrameworkElement
    {
        public RowElementPanel()
        {
        }

        RowPresenter _rowPresenter;
        private RowPresenter RowPresenter
        {
            get
            {
                var value = RowView == null ? null : RowView.RowPresenter;
                if (_rowPresenter != value)
                {
                    if (value != null)
                        value.RowPanel = this;
                    _rowPresenter = value;
                }
                return _rowPresenter;
            }
        }

        private RowView RowView
        {
            get { return TemplatedParent as RowView; }
        }

        internal IReadOnlyList<UIElement> Elements
        {
            get { return RowPresenter == null ? EmptyArray<UIElement>.Singleton : RowPresenter.Elements; }
        }

        protected override int VisualChildrenCount
        {
            get { return Elements.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= VisualChildrenCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            return Elements[index];
        }

        //protected override Size MeasureOverride(Size availableSize)
        //{
        //    return base.MeasureOverride(availableSize);
        //}

        //protected override Size ArrangeOverride(Size finalSize)
        //{
        //    return base.ArrangeOverride(finalSize);
        //}
    }
}
