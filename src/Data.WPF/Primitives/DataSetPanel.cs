using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    public class DataSetPanel : FrameworkElement
    {
        DataSetManager Manager { get; set; }

        ObservableCollection<UIElement> ScalarUIElements
        {
            get { return Manager == null ? null : Manager.ScalarUIElements; }
        }

        DataRowListView DataRowListView
        {
            get { return Manager == null ? null : Manager.DataRowListView; }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var dataSetView = TemplatedParent as DataSetView;
            if (dataSetView != null)
            {
                Manager = dataSetView.Manager;
                AddLogicalChild(DataRowListView);
                AddVisualChild(DataRowListView);
                AddScalarUIElements(ScalarUIElements);
                ScalarUIElements.CollectionChanged += OnScalarUIElementsChanged;
            }
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
            get { return Manager == null ? 0: ScalarUIElements.Count + 1; }
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
    }
}
