using DevZest.Data.Windows.Controls;
using DevZest.Data.Windows.Primitives;
using System;

namespace DevZest.Data.Windows
{
    public abstract class DataPresenter<T> : DataPresenter
        where T : Model, new()
    {
        public void Show(DataSet<T> dataSet, DataView view)
        {
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            DataSet = dataSet;
            Attach(view);
        }

        private DataSet<T> _dataSet;
        public new DataSet<T> DataSet
        {
            get { return _dataSet; }
            set
            {
                _dataSet = value;
                _template = new Template();
                using (var builder = new TemplateBuilder(Template, DataSet.Model))
                {
                    BuildTemplate(builder);
                }
                _layoutManager = LayoutManager.Create(this);
            }
        }

        private Template _template;
        public sealed override Template Template
        {
            get { return _template; }
        }

        private LayoutManager _layoutManager;
        internal sealed override LayoutManager LayoutManager
        {
            get { return _layoutManager; }
        }

        protected abstract void BuildTemplate(TemplateBuilder builder);

        internal sealed override DataSet GetDataSet()
        {
            return _dataSet;
        }

        public T _
        {
            get { return _dataSet == null ? null : _dataSet._; }
        }
    }
}
