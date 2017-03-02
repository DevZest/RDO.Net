using DevZest.Data;
using DevZest.Windows.Controls;
using DevZest.Windows.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Windows.Data
{
    public abstract class DataPresenter<T> : DataPresenter
        where T : Model, new()
    {
        public void Show(DataView dataView, DataSet<T> dataSet, _Boolean where = null, ColumnSort[] orderBy = null)
        {
            if (dataView == null)
                throw new ArgumentNullException(nameof(dataView));
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            if (dataView.DataPresenter != null && dataView.DataPresenter != this)
                throw new ArgumentException(Strings.DataPresenter_InvalidDataView, nameof(dataView));

            var existingView = dataView.DataPresenter == this;
            if (existingView)
                DetachView();
            DataSet = dataSet;
            var template = new Template();
            using (var builder = new TemplateBuilder(template, DataSet.Model))
            {
                BuildTemplate(builder);
            }
            _layoutManager = LayoutManager.Create(this, template, dataSet, where, orderBy);
            AttachView(dataView);
            if (!existingView)
            {
                dataView.SetupCommandBindings();
                dataView.SetupInputBindings();
            }
        }

        private void DetachView()
        {
            Debug.Assert(_view != null);
            _layoutManager.ClearElements();
            _view.DataPresenter = null;
            _view = null;
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
