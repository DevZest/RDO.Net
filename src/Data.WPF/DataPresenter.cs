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
        public abstract DataView View { get; }

        internal abstract LayoutManager LayoutManager { get; }

        public Template Template
        {
            get { return LayoutManager == null ? null : LayoutManager.Template; }
        }

        public DataSet DataSet
        {
            get { return LayoutManager == null ? null : LayoutManager.DataSet; }
        }

        public _Boolean Where
        {
            get { return LayoutManager == null ? null : LayoutManager.Where; }
        }

        public IReadOnlyList<ColumnSort> OrderBy
        {
            get { return LayoutManager == null ? null : LayoutManager.OrderBy; }
        }

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
    }
}
