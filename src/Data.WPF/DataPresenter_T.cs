using DevZest.Data;
using DevZest.Windows.Controls;
using DevZest.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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

            AttachView(dataView);
            Mount(dataView, dataSet, where, orderBy);
        }

        private void Mount(DataView dataView, DataSet<T> dataSet, Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            DataSet = dataSet;
            var template = new Template();
            using (var builder = new TemplateBuilder(template, DataSet.Model))
            {
                BuildTemplate(builder);
            }
            _layoutManager = LayoutManager.Create(this, template, dataSet, where, orderBy);
            _dataReloader = null;
            dataView.OnDataLoaded();
        }

        public Task ShowAsync(DataView dataView, Func<Task<DataSet<T>>> getDataSet, bool resetCriteria = false)
        {
            if (resetCriteria)
                return ShowAsync(dataView, getDataSet, null, null);
            else
                return ShowAsync(dataView, getDataSet, _ => Where, _ => OrderBy);
        }

        public async Task ShowAsync(DataView dataView, Func<Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
        {
            if (dataView == null)
                throw new ArgumentNullException(nameof(dataView));
            if (getDataSet == null)
                throw new ArgumentNullException(nameof(getDataSet));

            AttachView(dataView);
            dataView.OnDataLoading();
            DataSet<T> dataSet;
            try
            {
                dataSet = await getDataSet();
            }
            catch (Exception ex)
            {
                dataView.OnDataLoadFailed(ex.ToString());
                _dataReloader = new DataReloader(getDataSet, getWhere, getOrderBy);
                return;
            }

            if (dataSet == null)
                dataSet = DataSet<T>.New();
            Predicate<DataRow> where = getWhere == null ? null : getWhere(dataSet._);
            IComparer<DataRow> orderBy = getOrderBy == null ? null : getOrderBy(dataSet._);
            Mount(dataView, dataSet, where, orderBy);
            _dataReloader = new DataReloader(getDataSet, _ => Where, _ => OrderBy);
        }

        private sealed class DataReloader
        {
            public DataReloader(Func<Task<DataSet<T>>> getDataSet, Func<T, Predicate<DataRow>> getWhere, Func<T, IComparer<DataRow>> getOrderBy)
            {
                _getDataSet = getDataSet;
                _getWhere = getWhere;
                _getOrderBy = getOrderBy;
            }

            private readonly Func<Task<DataSet<T>>> _getDataSet;
            private readonly Func<T, Predicate<DataRow>> _getWhere;
            private readonly Func<T, IComparer<DataRow>> _getOrderBy;

            public Task Run(DataPresenter<T> dataPresenter, DataView dataView)
            {
                return dataPresenter.ShowAsync(dataView, _getDataSet, _getWhere, _getOrderBy);
            }
        }

        private DataReloader _dataReloader;

        internal sealed override bool CanReload
        {
            get { return _dataReloader != null; }
        }

        internal sealed override void Reload()
        {
            var dataReloader = _dataReloader;
            _dataReloader = null;
            dataReloader.Run(this, View);
        }

        public new DataSet<T> DataSet { get; private set; }

        public sealed override void DetachView()
        {
            base.DetachView();
            _layoutManager.ClearElements();
            _layoutManager = null;
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
