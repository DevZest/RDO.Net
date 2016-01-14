using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    public class RowPanel : FrameworkElement
    {
        private static readonly DependencyProperty ViewProperty = DependencyProperty.Register(nameof(View), typeof(RowView),
            typeof(RowPanel), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnViewChanged));

        private static void OnViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RowPanel)d).OnViewChanged((RowView)e.OldValue, (RowView)e.NewValue);
        }

        public RowPanel()
        {
            _elements = IElementCollectionFactory.Create(this);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            var binding = new System.Windows.Data.Binding(DataForm.ViewProperty.Name);
            binding.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
            BindingOperations.SetBinding(this, ViewProperty, binding);
        }

        private RowView View
        {
            get { return (RowView)GetValue(ViewProperty); }
        }

        private void OnViewChanged(RowView oldValue, RowView newValue)
        {
            if (oldValue != null)
                oldValue.Recycle();

            if (newValue != null)
                newValue.Realize(_elements);
        }

        private IList<UIElement> _elements;

        protected override int VisualChildrenCount
        {
            get { return _elements.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= VisualChildrenCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _elements[index];
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
