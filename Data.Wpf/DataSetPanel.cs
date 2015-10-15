using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DevZest.Data.Wpf
{
    public class DataSetPanel : Panel, IScrollInfo
    {
        DataSetControl DataSetControl
        {
            get { return TemplatedParent as DataSetControl; }
        }

        #region IScrollInfo

        ScrollManager _scrollManager;

        ScrollViewer IScrollInfo.ScrollOwner
        {
            get { return _scrollManager.ScrollOwner; }
            set { _scrollManager.ScrollOwner = value; }
        }

        double IScrollInfo.ExtentHeight
        {
            get { return _scrollManager.ExtentHeight; }
        }

        double IScrollInfo.ExtentWidth
        {
            get { return _scrollManager.ExtentWidth; }
        }

        double IScrollInfo.ViewportHeight
        {
            get { return _scrollManager.ViewportHeight; }
        }

        double IScrollInfo.ViewportWidth
        {
            get { return _scrollManager.ViewportWidth; }
        }

        double IScrollInfo.VerticalOffset
        {
            get { return _scrollManager.VerticalOffset; }
        }

        double IScrollInfo.HorizontalOffset
        {
            get { return _scrollManager.HorizontalOffset; }
        }

        bool IScrollInfo.CanVerticallyScroll
        {
            get { return _scrollManager.CanVerticallyScroll; }
            set { _scrollManager.CanVerticallyScroll = value; }
        }

        bool IScrollInfo.CanHorizontallyScroll
        {
            get { return _scrollManager.CanHorizontallyScroll; }
            set { _scrollManager.CanHorizontallyScroll = value; }
        }

        void IScrollInfo.LineUp()
        {
            _scrollManager.LineUp();
        }

        void IScrollInfo.LineDown()
        {
            _scrollManager.LineDown();
        }

        void IScrollInfo.LineLeft()
        {
            _scrollManager.LineLeft();
        }

        void IScrollInfo.LineRight()
        {
            _scrollManager.LineRight();
        }

        void IScrollInfo.PageUp()
        {
            _scrollManager.PageUp();
        }

        void IScrollInfo.PageDown()
        {
            _scrollManager.PageDown();
        }

        void IScrollInfo.PageLeft()
        {
            _scrollManager.PageLeft();
        }

        void IScrollInfo.PageRight()
        {
            _scrollManager.PageRight();
        }

        void IScrollInfo.MouseWheelUp()
        {
            _scrollManager.MouseWheelUp();
        }

        void IScrollInfo.MouseWheelDown()
        {
            _scrollManager.MouseWheelDown();
        }

        void IScrollInfo.MouseWheelLeft()
        {
            _scrollManager.MouseWheelLeft();
        }

        void IScrollInfo.MouseWheelRight()
        {
            _scrollManager.MouseWheelRight();
        }

        void IScrollInfo.SetHorizontalOffset(double offset)
        {
           _scrollManager.SetHorizontalOffset(offset);
        }

        void IScrollInfo.SetVerticalOffset(double offset)
        {
            _scrollManager.SetVerticalOffset(offset);
        }

        Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle)
        {
            return _scrollManager.MakeVisible(visual, rectangle);
        }

        #endregion
    }
}
