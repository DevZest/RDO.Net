using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    public class RowPanel : FrameworkElement
    {
        public RowPanel()
        {
            ElementCollection = IElementCollectionFactory.Create(this);
        }

        internal IElementCollection ElementCollection { get; private set; }
        private IReadOnlyList<UIElement> Elements
        {
            get { return ElementCollection; }
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

        protected override Size MeasureOverride(Size availableSize)
        {
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }
    }
}
