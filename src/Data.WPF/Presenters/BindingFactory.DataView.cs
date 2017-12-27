using DevZest.Data.Views;
using System;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<DataView> BindToDataView<T>(this T _, Func<DataPresenter<T>> dataPresenterCreator)
            where T : Model, new()
        {
            if (_ == null)
                throw new ArgumentNullException(nameof(_));
            if (dataPresenterCreator == null)
                throw new ArgumentNullException(nameof(dataPresenterCreator));

            return new RowBinding<DataView>(onSetup: (v, p) =>
                {
                    var dataSet = p.DataRow.Children(_);
                    var dataPresenter = dataPresenterCreator();
                    dataPresenter.Show(v, dataSet);
                },
                onRefresh: null,
                onCleanup: (v, p) =>
                {
                    var dataPresenter = v.DataPresenter;
                    dataPresenter.DetachView();
                });
        }
    }
}
