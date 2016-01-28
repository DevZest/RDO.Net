using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    internal sealed class ScalarElementsContainer : FrameworkElement
    {
        public ScalarElementsContainer(ScalarItem scalarItem)
        {
            Debug.Assert(scalarItem.IsRepeat);
            _scalarItem = scalarItem;
            _scalarElements = new UIElementCollection(this, this);
        }

        private readonly ScalarItem _scalarItem;
        private readonly UIElementCollection _scalarElements;

        private ScalarItemRepeatMode RepeatMode
        {
            get { return _scalarItem.RepeatMode; }
        }

        private GridTemplate Template
        {
            get { return _scalarItem.Owner; }
        }

        private DataView DataView
        {
            get { return Template.Owner; }
        }

        private LayoutManager LayoutManager
        {
            get { return DataView.LayoutManager; }
        }

        public UIElement this[Orientation orientation, int repeatIndex]
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
