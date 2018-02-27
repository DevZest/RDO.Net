using System;
using System.Collections.Generic;
using System.Windows.Controls;
using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using DevZest.Data.Views.Primitives;

namespace DevZest.Data.Presenters
{
    public interface IDataPresenter : IService
    {
        bool CanInsert { get; }
        bool CanSubmitInput { get; }
        ContainerView CurrentContainerView { get; }
        RowPresenter CurrentRow { get; set; }
        DataSet DataSet { get; }
        RowPresenter EditingRow { get; }
        int FlowRepeatCount { get; }
        RowPresenter InsertingRow { get; }
        bool IsEditing { get; }
        bool IsInserting { get; }
        bool IsRecursive { get; }
        Orientation? LayoutOrientation { get; }
        IComparer<DataRow> OrderBy { get; set; }
        IReadOnlyList<RowPresenter> Rows { get; }
        RowValidation RowValidation { get; }
        IReadOnlyList<Scalar> Scalars { get; }
        ScalarValidation ScalarValidation { get; }
        IScrollable Scrollable { get; }
        IReadOnlyCollection<RowPresenter> SelectedRows { get; }
        Template Template { get; }
        DataView View { get; }
        RowPresenter VirtualRow { get; }
        Predicate<DataRow> Where { get; set; }

        event EventHandler<EventArgs> ViewChanged;
        event EventHandler ViewInvalidated;
        event EventHandler ViewRefreshed;
        event EventHandler ViewRefreshing;

        void Apply(Predicate<DataRow> where, IComparer<DataRow> orderBy);
        void BeginInsertAfter(RowPresenter row = null);
        void BeginInsertBefore(RowPresenter row = null);
        void DetachView();
        T GetService<T>() where T : class, IService;
        void InvalidateMeasure();
        void InvalidateView();
        void ResumeInvalidateView();
        void Select(params RowPresenter[] rows);
        void Select(RowPresenter rowPresenter, SelectionMode selectionMode, bool ensureVisible = true, Action beforeSelecting = null);
        bool SubmitInput(bool focusToErrorInput = true);
        void SuspendInvalidateView();
    }
}