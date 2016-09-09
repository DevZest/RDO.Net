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

        private LayoutManager RequireLayoutManager()
        {
            if (LayoutManager == null)
                throw new InvalidOperationException(Strings.DataPresenter_NullDataSet);
            return LayoutManager;
        }

        public void Query(_Boolean where)
        {
            RequireLayoutManager().Query(where);
        }

        public void Query(_Boolean where, ColumnSort[] orderBy)
        {
            RequireLayoutManager().Query(where, orderBy);
        }

        public void Query(ColumnSort[] orderBy)
        {
            RequireLayoutManager().Query(orderBy);
        }

        public IReadOnlyList<RowPresenter> Rows
        {
            get { return LayoutManager == null ? null : LayoutManager.Rows; }
        }

        public RowPresenter CurrentRow
        {
            get { return LayoutManager == null ? null : LayoutManager.CurrentRow; }
        }

        public bool IsEditing
        {
            get { return LayoutManager == null ? false : LayoutManager.IsEditing; }
        }

        public RowPresenter EditingRow
        {
            get { return CurrentRow != null && IsEditing ? CurrentRow : null; }
        }

        public bool IsInserting
        {
            get { return IsEditing && LayoutManager.CurrentRow == LayoutManager.Placeholder; }
        }

        public RowPresenter InsertingRow
        {
            get { return IsInserting ? CurrentRow : null; }
        }

        public bool CanInsert
        {
            get { return !IsEditing && RequireLayoutManager().DataSet.EditingRow == null; }
        }

        public void BeginInsertBefore(RowPresenter child = null)
        {
            VerifyInsert(child);
            RequireLayoutManager().BeginInsertBefore(null, child);
        }

        public void BeginInsertAfter(RowPresenter child = null)
        {
            VerifyInsert(child);
            RequireLayoutManager().BeginInsertAfter(null, child);
        }

        private void VerifyInsert(RowPresenter child)
        {
            if (!CanInsert)
                throw new InvalidOperationException(Strings.DataPresenter_VerifyCanInsert);
            if (child != null & child.RowManager != RequireLayoutManager())
                throw new ArgumentException(Strings.DataPresenter_InvalidChildRow, nameof(child));
        }

        public IReadOnlyCollection<RowPresenter> SelectedRows
        {
            get { return LayoutManager == null ? null : LayoutManager.SelectedRows; }
        }
    }
}
