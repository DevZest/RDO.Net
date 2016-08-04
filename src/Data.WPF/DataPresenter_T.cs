using DevZest.Data.Windows.Controls;
using System;

namespace DevZest.Data.Windows
{
    public abstract class DataPresenter<T> : DataPresenter
        where T : Model, new()
    {
        public void Show(DataSet<T> dataSet, DataView dataView)
        {
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            _dataSet = dataSet;
            using (var builder = new TemplateBuilder(Template, _))
            {
                BuildTemplate(builder);
            }
            dataView.Cleanup();
            dataView.Initialize(this);
        }

        private DataSet<T> _dataSet;
        public new DataSet<T> DataSet
        {
            get { return _dataSet; }
        }

        internal sealed override DataSet GetDataSet()
        {
            return _dataSet;
        }

        public T _
        {
            get { return _dataSet == null ? null : _dataSet._; }
        }

        protected abstract void BuildTemplate(TemplateBuilder builder);
    }
}
