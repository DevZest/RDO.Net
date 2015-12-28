using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ChildGridItem : GridItem
    {
        internal ChildGridItem(Func<DataSetView> viewConstructor, Func<DataRowPresenter, DataSetPresenter> childPresenterConstructor)
                : base()
        {
            _viewConstructor = viewConstructor == null ? () => new DataSetView() : viewConstructor;

            Debug.Assert(childPresenterConstructor != null);
            ChildPresenterConstructor = childPresenterConstructor;
        }

        internal int Ordinal { get; set; }

        Func<DataSetView> _viewConstructor;

        internal Func<DataRowPresenter, DataSetPresenter> ChildPresenterConstructor { get; private set; }

        internal sealed override UIElement Create()
        {
            return _viewConstructor();
        }

        internal sealed override void OnMounted(UIElement uiElement)
        {
            var dataSetView = (DataSetView)uiElement;
            var parentDataRowPresenter = dataSetView.GetDataRowPresenter();
            dataSetView.Show(parentDataRowPresenter.Children[Ordinal]);
        }

        internal sealed override void OnUnmounting(UIElement uiElement)
        {
            var dataSetView = (DataSetView)uiElement;
            dataSetView.Unmount();
        }
    }
}
