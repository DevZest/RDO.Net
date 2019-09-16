using DevZest.Data.Views;
using System;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        /// <summary>
        /// Binds child DataSet to <see cref="DataView"/>.
        /// </summary>
        /// <typeparam name="T">The type of child model.</typeparam>
        /// <param name="childModel">The model of child DataSet.</param>
        /// <param name="dataPresenterCreator">A delegate to create the data presenter.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<DataView> BindToDataView<T>(this T childModel, Func<DataPresenter<T>> dataPresenterCreator)
            where T : Model, new()
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (dataPresenterCreator == null)
                throw new ArgumentNullException(nameof(dataPresenterCreator));

            return new RowBinding<DataView>(onSetup: (v, p) =>
                {
                    var dataSet = childModel.GetChildDataSet(p.DataRow);
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
