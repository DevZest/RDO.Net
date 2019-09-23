using System;
using System.Collections.Generic;
using System.Windows.Controls;
using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using DevZest.Data.Views.Primitives;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Contains presentation logic for scalar data and DataSet.
    /// </summary>
    public interface IDataPresenter : IService
    {
        /// <summary>
        /// Gets a value inidicates whether new row can be inserted.
        /// </summary>
        bool CanInsert { get; }

        /// <summary>
        /// Determines whether data input can be submitted without any validation error.
        /// </summary>
        bool CanSubmitInput { get; }

        /// <summary>
        /// Gets the current <see cref="ContainerView"/>, which is either <see cref="RowView"/> or <see cref="BlockView"/>.
        /// </summary>
        ContainerView CurrentContainerView { get; }

        /// <summary>
        /// Gets or sets the current <see cref="RowPresenter"/>.
        /// </summary>
        RowPresenter CurrentRow { get; set; }

        /// <summary>
        /// Gets the source DataSet.
        /// </summary>
        DataSet DataSet { get; }

        /// <summary>
        /// Gets the current <see cref="RowPresenter"/> which is in edit mode.
        /// </summary>
        RowPresenter EditingRow { get; }

        /// <summary>
        /// Gets the value that specifies how many rows will flow in BlockView first, then expand afterwards.
        /// </summary>
        int FlowRepeatCount { get; }

        /// <summary>
        /// Gets the current <see cref="RowPresenter"/> which is in inserting mode.
        /// </summary>
        RowPresenter InsertingRow { get; }

        /// <summary>
        /// Gets a value indicates whether current row is in edit mode.
        /// </summary>
        bool IsEditing { get; }

        /// <summary>
        /// Gets a value indicates whether current row is in inserting mode.
        /// </summary>
        bool IsInserting { get; }

        /// <summary>
        /// Gets a value indicates the underlying rows are recursive tree structure.
        /// </summary>
        bool IsRecursive { get; }

        /// <summary>
        /// Gets the layout orientation that rows will be presented repeatedly.
        /// </summary>
        Orientation? LayoutOrientation { get; }

        /// <summary>
        /// Gets or sets the comparer to sort the rows.
        /// </summary>
        IComparer<DataRow> OrderBy { get; set; }

        /// <summary>
        /// Gets the collection <see cref="RowPresenter"/> objects.
        /// </summary>
        IReadOnlyList<RowPresenter> Rows { get; }

        /// <summary>
        /// Gets the object that contains row validation logic.
        /// </summary>        
        RowValidation RowValidation { get; }

        /// <summary>
        /// Gets the container for all scalar values.
        /// </summary>
        ScalarContainer ScalarContainer { get; }

        /// <summary>
        /// Gets an object which contains the scalar data validation logic.
        /// </summary>
        ScalarValidation ScalarValidation { get; }

        /// <summary>
        /// Gets the object that contains layout scrolling logic.
        /// </summary>
        IScrollable Scrollable { get; }

        /// <summary>
        /// Gets the collection of selected rows.
        /// </summary>
        IReadOnlyCollection<RowPresenter> SelectedRows { get; }

        /// <summary>
        /// Gets the <see cref="Template"/> associated with this presenter.
        /// </summary>
        Template Template { get; }

        /// <summary>
        /// Gets the <see cref="DataView"/> that is attached to this <see cref="DataPresenter"/>.
        /// </summary>
        DataView View { get; }

        /// <summary>
        /// Gets the virtual row for inserting indication.
        /// </summary>
        RowPresenter VirtualRow { get; }

        /// <summary>
        /// Gets or sets the condition to filter the rows.
        /// </summary>
        Predicate<DataRow> Where { get; set; }

        /// <summary>
        /// Occurs when the view has been changed.
        /// </summary>
        event EventHandler<EventArgs> ViewChanged;

        /// <summary>
        /// Occurs before view is invalidating.
        /// </summary>
        event EventHandler ViewInvalidating;

        /// <summary>
        /// Occurs after view is invalidated.
        /// </summary>
        event EventHandler ViewInvalidated;

        /// <summary>
        /// Occurs before view is refreshing.
        /// </summary>
        event EventHandler ViewRefreshing;

        /// <summary>
        /// Occurs after view is refreshed.
        /// </summary>
        event EventHandler ViewRefreshed;

        /// <summary>
        /// Applies filtering condition and sorting comparer.
        /// </summary>
        /// <param name="where">The filtering condition.</param>
        /// <param name="orderBy">The sorting comparer.</param>
        void Apply(Predicate<DataRow> where, IComparer<DataRow> orderBy);

        /// <summary>
        /// Begins inserting new row after specified row.
        /// </summary>
        /// <param name="row">The specified row. If <see langword="null"/>, insert at the end.</param>
        void BeginInsertAfter(RowPresenter row = null);

        /// <summary>
        /// Begins inserting new row before specified row.
        /// </summary>
        /// <param name="row">The specified row. If <see langword="null"/>, insert at the beginning.</param>
        void BeginInsertBefore(RowPresenter row = null);

        /// <summary>
        /// Detaches this presenter from the view.
        /// </summary>
        void DetachView();

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="autoCreate">Indicates whether the service should be created automatically.</param>
        /// <returns>The result service.</returns>
        T GetService<T>(bool autoCreate = true) where T : class, IService;

        /// <summary>
        /// Invalidates the measurement state (layout) for the view.
        /// </summary>
        void InvalidateMeasure();

        /// <summary>
        /// Invalidates the rendering of the view.
        /// </summary>
        void InvalidateView();

        /// <summary>
        /// Temporarily suspends the view rendering.
        /// </summary>
        /// <remarks>Call <see cref="SuspendInvalidateView"/> and <see cref="ResumeInvalidateView"/> in tandem to suppress multiple
        /// view rendering for performance.</remarks>
        void SuspendInvalidateView();

        /// <summary>
        /// Resumes the usual view rendering.
        /// </summary>
        /// <remarks>Call <see cref="SuspendInvalidateView"/> and <see cref="ResumeInvalidateView"/> in tandem to suppress multiple
        /// view rendering for performance.</remarks>
        void ResumeInvalidateView();

        /// <summary>
        /// Selects multiple rows.
        /// </summary>
        /// <param name="rows">The multiple rows.</param>
        void Select(params RowPresenter[] rows);

        /// <summary>
        /// Selects specified <see cref="RowPresenter"/>.
        /// </summary>
        /// <param name="rowPresenter">The specified <see cref="RowPresenter"/>.</param>
        /// <param name="selectionMode">The selection mode.</param>
        /// <param name="ensureVisible">Indicates whether selected row must be visible.</param>
        /// <param name="beforeSelecting">A delegate will be invoked before selectinng.</param>
        void Select(RowPresenter rowPresenter, SelectionMode selectionMode, bool ensureVisible = true, Action beforeSelecting = null);

        /// <summary>
        /// Tries to submit data input with validation.
        /// </summary>
        /// <param name="focusToErrorInput">A value indicates whether the UI element with validation error should have keyboard focus.</param>
        /// <returns><see langword="true"/> if data input submitted sccessfully, otherwise <see langword="false"/>.</returns>
        bool SubmitInput(bool focusToErrorInput = true);
    }
}