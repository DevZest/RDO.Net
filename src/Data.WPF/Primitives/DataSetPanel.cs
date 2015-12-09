using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    public class DataSetPanel : FrameworkElement, IScrollInfo
    {
        private static readonly DependencyProperty ManagerProperty = DependencyProperty.Register(nameof(Manager),
            typeof(DataSetManager), typeof(DataSetPanel), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnManagerChanged));

        private static void OnManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (DataSetManager)e.OldValue;
            var newValue = (DataSetManager)e.NewValue;
            ((DataSetPanel)d).OnManagerChanged(oldValue, newValue);
        }

        public DataSetPanel()
        {
            var binding = new Binding(DataSetView.ManagerProperty.Name);
            binding.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
            BindingOperations.SetBinding(this, ManagerProperty, binding);
        }

        private DataSetManager Manager
        {
            get { return (DataSetManager)GetValue(ManagerProperty); }
        }

        private void OnManagerChanged(DataSetManager oldValue, DataSetManager newValue)
        {
            if (oldValue != null)
            {
                var layoutManager = oldValue.LayoutManager;
                RemoveLogicalChild(layoutManager.DataRowListView);
                RemoveVisualChild(layoutManager.DataRowListView);
                RemoveScalarUIElements(layoutManager.ScalarUIElements);
                layoutManager.ScalarUIElements.CollectionChanged -= OnScalarUIElementsChanged;
            }

            if (newValue != null)
            {
                var layoutManager = newValue.LayoutManager;
                AddLogicalChild(layoutManager.DataRowListView);
                AddVisualChild(layoutManager.DataRowListView);
                AddScalarUIElements(layoutManager.ScalarUIElements);
                layoutManager.ScalarUIElements.CollectionChanged += OnScalarUIElementsChanged;
            }
        }

        LayoutManager LayoutManager
        {
            get { return Manager == null ? null : Manager.LayoutManager; }
        }

        ObservableCollection<UIElement> ScalarUIElements
        {
            get { return LayoutManager == null ? null : LayoutManager.ScalarUIElements; }
        }

        DataRowListView DataRowListView
        {
            get { return LayoutManager == null ? null : LayoutManager.DataRowListView; }
        }

        private void OnScalarUIElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Reset)
                RemoveScalarUIElements(e.OldItems);

            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Reset)
                AddScalarUIElements(e.NewItems);
        }

        private void RemoveScalarUIElements(ICollection items)
        {
            foreach (var item in items)
            {
                RemoveLogicalChild(item);
                RemoveVisualChild((UIElement)item);
            }
        }

        private void AddScalarUIElements(ICollection items)
        {
            foreach (var item in items)
            {
                AddLogicalChild(item);
                AddVisualChild((UIElement)item);
            }
        }

        protected override int VisualChildrenCount
        {
            get { return LayoutManager == null ? 0: ScalarUIElements.Count + 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= VisualChildrenCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            return index == 0 ? DataRowListView : ScalarUIElements[index + 1];
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }

        #region IScrollInfo

        bool _canVerticallyScroll;
        bool IScrollInfo.CanVerticallyScroll
        {
            get { return _canVerticallyScroll; }
            set { _canVerticallyScroll = value; }
        }

        bool _canHorizontallyScroll;
        bool IScrollInfo.CanHorizontallyScroll
        {
            get { return _canHorizontallyScroll; }
            set { _canHorizontallyScroll = value; }
        }

        double IScrollInfo.ExtentWidth
        {
            get { return LayoutManager.ExtentSize.Width; }
        }

        double IScrollInfo.ExtentHeight
        {
            get { return LayoutManager.ExtentSize.Height; }
        }

        double IScrollInfo.ViewportWidth
        {
            get { return LayoutManager.ViewportSize.Width; }
        }

        double IScrollInfo.ViewportHeight
        {
            get { return LayoutManager.ViewportSize.Height; }
        }

        double IScrollInfo.HorizontalOffset
        {
            get { return LayoutManager.HorizontalOffset; }
        }

        double IScrollInfo.VerticalOffset
        {
            get { return LayoutManager.VerticalOffset; }
        }

        ScrollViewer IScrollInfo.ScrollOwner
        {
            get { return LayoutManager.ScrollOwner; }
            set { LayoutManager.ScrollOwner = value; }
        }

        void IScrollInfo.LineUp()
        {
            throw new NotImplementedException();
        }

        void IScrollInfo.LineDown()
        {
            throw new NotImplementedException();
        }

        void IScrollInfo.LineLeft()
        {
            throw new NotImplementedException();
        }

        void IScrollInfo.LineRight()
        {
            throw new NotImplementedException();
        }

        void IScrollInfo.PageUp()
        {
            throw new NotImplementedException();
        }

        void IScrollInfo.PageDown()
        {
            throw new NotImplementedException();
        }

        void IScrollInfo.PageLeft()
        {
            throw new NotImplementedException();
        }

        void IScrollInfo.PageRight()
        {
            throw new NotImplementedException();
        }

        void IScrollInfo.MouseWheelUp()
        {
            throw new NotImplementedException();
        }

        void IScrollInfo.MouseWheelDown()
        {
            throw new NotImplementedException();
        }

        void IScrollInfo.MouseWheelLeft()
        {
            throw new NotImplementedException();
        }

        void IScrollInfo.MouseWheelRight()
        {
            throw new NotImplementedException();
        }

        void IScrollInfo.SetHorizontalOffset(double offset)
        {
            LayoutManager.HorizontalOffset = offset;
        }

        void IScrollInfo.SetVerticalOffset(double offset)
        {
            LayoutManager.VerticalOffset = offset;
        }

        Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
