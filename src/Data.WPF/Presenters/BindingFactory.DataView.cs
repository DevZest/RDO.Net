using DevZest.Data.Views;
using System;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<DataView> BindToDataView<T>(this T entity, Func<DataPresenter<T>> dataPresenterCreator)
            where T : class, IEntity, new()
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (dataPresenterCreator == null)
                throw new ArgumentNullException(nameof(dataPresenterCreator));

            return new RowBinding<DataView>(onSetup: (v, p) =>
                {
                    var dataSet = entity.GetChildDataSet(p.DataRow);
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
