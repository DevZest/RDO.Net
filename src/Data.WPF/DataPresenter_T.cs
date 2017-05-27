using DevZest.Data;
using DevZest.Windows.Controls;
using DevZest.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Windows
{
    public abstract class DataPresenter<T> : DataPresenter
        where T : Model, new()
    {
        public void Show(DataView dataView, DataSet<T> dataSet, bool resetCriteria = false)
        {
            if (resetCriteria)
                Show(dataView, dataSet, null, null);
            else
                Show(dataView, dataSet, Where, OrderBy);
        }

        public void Show(DataView dataView, DataSet<T> dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            if (dataView == null)
                throw new ArgumentNullException(nameof(dataView));
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            if (dataView.DataPresenter != null && dataView.DataPresenter != this)
                throw new ArgumentException(Strings.DataPresenter_InvalidDataView, nameof(dataView));

            if (View != null)
                DetachView();
            DataSet = dataSet;
            var template = new Template();
            using (var builder = new TemplateBuilder(template, DataSet.Model))
            {
                BuildTemplate(builder);
            }
            _layoutManager = LayoutManager.Create(this, template, dataSet, where, orderBy);
            AttachView(dataView);
        }

        private void DetachView()
        {
            Debug.Assert(_view != null);
            _view.CleanupCommandEntries();
            _layoutManager.ClearElements();
            _view.DataPresenter = null;
            _view = null;
        }

        private void AttachView(DataView value)
        {
            Debug.Assert(View == null && value != null);
            _view = value;
            _view.DataPresenter = this;
            _view.SetupCommandEntries();
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

        private sealed class DataRowFilter
        {
            public DataRowFilter(Func<T, DataRow, bool> where)
            {
                Debug.Assert(where != null);
                _where = where;
            }

            private readonly Func<T, DataRow, bool> _where;

            private bool Evaluate(DataRow dataRow)
            {
                return _where((T)dataRow.Model, dataRow);
            }

            public Predicate<DataRow> ToPredicate()
            {
                return Evaluate;
            }
        }

        public static Predicate<DataRow> ToPredicate(Func<T, DataRow, bool> predicate, bool ensureStatic = true)
        {
            if (predicate == null)
                return null;

            if (ensureStatic && predicate.Target != null)
                throw new ArgumentException(Strings.DataPresenter_ExpressionMustBeStatic, nameof(predicate));

            return new DataRowFilter(predicate).ToPredicate();
        }
    }
}
