using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ChildEntry : ListEntry
    {
        internal ChildEntry(Func<DataSetView> viewConstructor, Func<DataRowPresenter, DataSetPresenter> childPresenterConstructor)
                : base(viewConstructor, null)
        {
            _viewConstructor = viewConstructor == null ? () => new DataSetView() : viewConstructor;

            Debug.Assert(childPresenterConstructor != null);
            ChildPresenterConstructor = childPresenterConstructor;
        }

        internal int ChildOrdinal { get; private set; }

        internal void Seal(GridTemplate owner, GridRange gridRange, int ordinal, int childOrdinal)
        {
            base.Seal(owner, gridRange, ordinal);
            ChildOrdinal = childOrdinal;
        }

        Func<DataSetView> _viewConstructor;

        internal Func<DataRowPresenter, DataSetPresenter> ChildPresenterConstructor { get; private set; }

        internal sealed override void OnMounted(UIElement uiElement)
        {
            var dataSetView = (DataSetView)uiElement;
            var parentDataRowPresenter = dataSetView.GetDataRowPresenter();
            dataSetView.Show(parentDataRowPresenter.Children[ChildOrdinal]);
        }

        internal sealed override void OnUnmounting(UIElement uiElement)
        {
            var dataSetView = (DataSetView)uiElement;
            dataSetView.Unmount();
        }
    }
}
