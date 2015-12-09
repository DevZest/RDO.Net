using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    internal class LayoutManager
    {
        internal LayoutManager(DataSetManager owner)
        {
            Debug.Assert(owner != null);
            _owner = owner;

            VisibleRows = new ObservableCollection<DataRowManager>();
            ScalarUIElements = new ObservableCollection<UIElement>();
            DataRowListView = new DataRowListView()
            {
                ItemsSource = VisibleRows
            };
        }

        private DataSetManager _owner;

        private GridTemplate Template
        {
            get { return _owner.Template; }
        }

        internal DataRowListView DataRowListView { get; private set; }

        internal ObservableCollection<UIElement> ScalarUIElements { get; private set; }

        internal ObservableCollection<DataRowManager> VisibleRows { get; private set; }

        internal Size ExtentSize { get; private set; }

        internal Size ViewportSize { get; set; }

        internal double HorizontalOffset { get; set; }

        internal double VerticalOffset { get; set; }

        internal ScrollViewer ScrollOwner { get; set; }

    }
}
