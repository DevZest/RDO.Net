using System.Collections.Generic;
using DevZest.Data.Windows.Primitives;
using System.Windows;
using DevZest.Data.Windows.Controls;
using System.Diagnostics;
using System;

namespace DevZest.Data.Windows
{
    public abstract class DataPresenter
    {
        public DataSet DataSet
        {
            get { return GetDataSet(); }
        }

        internal abstract DataSet GetDataSet();

        public DataView View { get; private set; }

        public abstract Template Template { get; }

        internal abstract LayoutManager LayoutManager { get; }

        public IReadOnlyList<RowPresenter> Rows
        {
            get { return LayoutManager == null ? null : LayoutManager.Rows; }
        }

        public RowPresenter CurrentRow
        {
            get { return LayoutManager == null ? null : LayoutManager.CurrentRow; }
        }

        public RowPresenter EditingRow
        {
            get { return LayoutManager == null ? null : LayoutManager.EditingRow; }
        }

        public IReadOnlyCollection<RowPresenter> SelectedRows
        {
            get { return LayoutManager == null ? null : LayoutManager.SelectedRows; }
        }

        public void Attach(DataView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (View == view)
                return;

            if (DataSet == null)
                throw new InvalidOperationException(Strings.DataPresenter_NullDataSet);

            if (View != null)
                Detach();

            View = view;
            View.DataPresenter = this;
        }

        public void Detach()
        {
            if (View == null)
                return;

            View.DataPresenter = null;
            LayoutManager.ClearElements();
            View = null;
        }
    }
}
