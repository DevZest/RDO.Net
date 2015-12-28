using System;

namespace DevZest.Data.Windows
{
    public static class DataSetViewFactory
    {
        public static ChildGridItem DataSetView<T>(this T childModel, Action<DataSetPresenterConfig, T> configAction, Func<DataSetView> viewConstructor = null)
            where T : Model, new()
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (configAction == null)
                throw new ArgumentNullException(nameof(configAction));

            return new ChildGridItem(viewConstructor, owner => DataSetPresenter.Create(owner.DataRow.Children(childModel), configAction));
        }

        public static ChildGridItem DataSetView<T>(this _DataSet<T> childDataSet, Action<DataSetPresenterConfig, T> configAction, Func<DataSetView> viewConstructor = null)
            where T : Model, new()
        {
            if (childDataSet == null)
                throw new ArgumentNullException(nameof(childDataSet));
            if (configAction == null)
                throw new ArgumentNullException(nameof(configAction));

            return new ChildGridItem(viewConstructor, owner => DataSetPresenter.Create(childDataSet[owner.DataRow], configAction));
        }
    }
}
