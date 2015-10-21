using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DevZest.Data.Wpf
{
    public class DataSetPanel : FrameworkElement
    {
        //private static ConditionalWeakTable<GridView, DataSetPanel> s_panelsByView = new ConditionalWeakTable<GridView, DataSetPanel>();

        //private static DataSetPanel GetDataSetPanel(GridView dataSetView)
        //{
        //    Debug.Assert(dataSetView != null);
        //    DataSetPanel result;
        //    return s_panelsByView.TryGetValue(dataSetView, out result) ? result : null;
        //}

        //private static void SetDataSetPanel(GridView dataSetView, DataSetPanel value)
        //{
        //    Debug.Assert(dataSetView != null);
        //    Debug.Assert(value != null);

        //    var oldValue = GetDataSetPanel(dataSetView);
        //    if (oldValue == value)
        //        return;

        //    if (oldValue != null)
        //    {
        //        s_panelsByView.Remove(dataSetView);
        //        oldValue.DataSetView = null;
        //    }

        //    s_panelsByView.Add(dataSetView, value);
        //    value.DataSetView = dataSetView;
        //}

        //private GridView _dataSetView;
        //private GridView DataSetView
        //{
        //    get { return _dataSetView; }
        //    set
        //    {
        //        if (_dataSetView == value)
        //            return;

        //        _dataSetView = value;
        //        ViewElements = _dataSetView == null ? null : _dataSetView.Elements;
        //    }
        //}

        //private GridElementCollection _viewElements;
        //private GridElementCollection ViewElements
        //{
        //    get { return _viewElements; }
        //    set
        //    {
        //        if (_viewElements == value)
        //            return;

        //        if (_viewElements != null)
        //        {
        //            _viewElements.CollectionChanged -= OnViewElementsChanged;
        //            foreach (var viewElement in _viewElements)
        //            {
        //                RemoveVisualChild(viewElement.UIElement);
        //                RemoveLogicalChild(viewElement.UIElement);
        //            }
        //        }

        //        _viewElements = value;

        //        if (_viewElements != null)
        //        {
        //            foreach (var viewElement in _viewElements)
        //            {
        //                AddVisualChild(viewElement.UIElement);
        //                AddLogicalChild(viewElement.UIElement);
        //            }
        //            _viewElements.CollectionChanged += OnViewElementsChanged;
        //        }
        //    }
        //}

        //private void OnViewElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    foreach (var item in e.OldItems)
        //    {
        //        var viewElement = item as GridElement;
        //        if (viewElement != null)
        //        {
        //            RemoveLogicalChild(viewElement.UIElement);
        //            RemoveVisualChild(viewElement.UIElement);
        //        }
        //    }

        //    foreach (var item in e.NewItems)
        //    {
        //        var viewElement = item as GridElement;
        //        if (viewElement != null)
        //        {
        //            AddVisualChild(viewElement.UIElement);
        //            AddLogicalChild(viewElement.UIElement);
        //        }
        //    }
        //}

        //protected override int VisualChildrenCount
        //{
        //    get { return ViewElements == null ? 0 : ViewElements.Count; }
        //}

        //protected override Visual GetVisualChild(int index)
        //{
        //    if (index < 0 || index >= VisualChildrenCount)
        //        throw new ArgumentOutOfRangeException(nameof(index));

        //    return ViewElements[index].UIElement;
        //}

        //protected override Size MeasureOverride(Size availableSize)
        //{
        //    return DataSetView != null ? DataSetView.Measure(availableSize) : base.MeasureOverride(availableSize);
        //}

        //protected override Size ArrangeOverride(Size finalSize)
        //{
        //    return DataSetView != null ? DataSetView.Arrange(finalSize) : base.ArrangeOverride(finalSize);
        //}

        //public override void OnApplyTemplate()
        //{
        //    var dataSetControl = TemplatedParent as DataSetControl;
        //    var dataSetView = dataSetControl == null ? null : dataSetControl.GridView;
        //    if (dataSetView != null)
        //        SetDataSetPanel(dataSetView, this); // Call this method to tie this DataSetPanel with dataSetView exclusively.
        //}

        //#region IScrollInfo

        //ScrollViewer _scrollOwner;
        //ScrollViewer IScrollInfo.ScrollOwner
        //{
        //    get { return _scrollOwner; }
        //    set { _scrollOwner = value; }
        //}

        //double IScrollInfo.ExtentHeight
        //{
        //    get { return DataSetView.ExtentSize.Height; }
        //}

        //double IScrollInfo.ExtentWidth
        //{
        //    get { return DataSetView.ExtentSize.Width; }
        //}

        //double IScrollInfo.ViewportHeight
        //{
        //    get { return DataSetView.ViewportSize.Height; }
        //}

        //double IScrollInfo.ViewportWidth
        //{
        //    get { return DataSetView.ViewportSize.Width; }
        //}

        //double IScrollInfo.VerticalOffset
        //{
        //    get { return DataSetView.ViewportOffset.Y; }
        //}

        //double IScrollInfo.HorizontalOffset
        //{
        //    get { return DataSetView.ViewportOffset.X; }
        //}

        //bool IScrollInfo.CanVerticallyScroll
        //{
        //    get { throw new NotImplementedException(); }
        //    set { throw new NotImplementedException(); }
        //}

        //bool IScrollInfo.CanHorizontallyScroll
        //{
        //    get { throw new NotImplementedException(); }
        //    set { throw new NotImplementedException(); }
        //}

        //void IScrollInfo.LineUp()
        //{
        //    throw new NotImplementedException();
        //}

        //void IScrollInfo.LineDown()
        //{
        //    throw new NotImplementedException();
        //}

        //void IScrollInfo.LineLeft()
        //{
        //    throw new NotImplementedException();
        //}

        //void IScrollInfo.LineRight()
        //{
        //    throw new NotImplementedException();
        //}

        //void IScrollInfo.PageUp()
        //{
        //    throw new NotImplementedException();
        //}

        //void IScrollInfo.PageDown()
        //{
        //    throw new NotImplementedException();
        //}

        //void IScrollInfo.PageLeft()
        //{
        //    throw new NotImplementedException();
        //}

        //void IScrollInfo.PageRight()
        //{
        //    throw new NotImplementedException();
        //}

        //void IScrollInfo.MouseWheelUp()
        //{
        //    throw new NotImplementedException();
        //}

        //void IScrollInfo.MouseWheelDown()
        //{
        //    throw new NotImplementedException();
        //}

        //void IScrollInfo.MouseWheelLeft()
        //{
        //    throw new NotImplementedException();
        //}

        //void IScrollInfo.MouseWheelRight()
        //{
        //    throw new NotImplementedException();
        //}

        //void IScrollInfo.SetHorizontalOffset(double offset)
        //{
        //   DataSetView.SetHorizontalOffset(offset);
        //}

        //void IScrollInfo.SetVerticalOffset(double offset)
        //{
        //    DataSetView.SetVerticalOffset(offset);
        //}

        //Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle)
        //{
        //    throw new NotImplementedException();
        //}

        //#endregion
    }
}
