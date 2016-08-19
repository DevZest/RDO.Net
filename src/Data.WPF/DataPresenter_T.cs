using DevZest.Data.Windows.Controls;
using DevZest.Data.Windows.Primitives;
using System;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    public abstract class DataPresenter<T> : DataPresenter
        where T : Model, new()
    {
        public void Show(DataView view, DataSet<T> dataSet, _Boolean where = null, ColumnSort[] orderBy = null)
        {
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            DetachView();
            DataSet = dataSet;
            var template = new Template();
            using (var builder = new TemplateBuilder(template, DataSet.Model))
            {
                BuildTemplate(builder);
            }
            _layoutManager = LayoutManager.Create(this, template, dataSet, where, orderBy);
            AttachView(view);
        }

        private void DetachView()
        {
            if (_view != null)
            {
                _view.DataPresenter = null;
                LayoutManager.ClearElements();
                _view = null;
            }
        }

        private void AttachView(DataView value)
        {
            Debug.Assert(View == null && value != null);
            _view = value;
            _view.DataPresenter = this;
        }

        public new DataSet<T> DataSet { get; private set; }

        private DataView _view;
        public sealed override DataView View
        {
            get { return _view; }
        }

        private LayoutManager _layoutManager;
        internal sealed override LayoutManager LayoutManager
        {
            get { return _layoutManager; }
        }

        protected abstract void BuildTemplate(TemplateBuilder builder);

        public T _
        {
            get { return DataSet == null ? null : DataSet._; }
        }
    }
}
