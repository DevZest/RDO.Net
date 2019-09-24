using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using DevZest.Windows;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    /// <summary>
    /// Represents the container of single child element that can perform either selection or editing operation.
    /// </summary>
    public partial class GridCell : Control, IRowElement
    {
        /// <summary>
        /// Gets the default in-place editing policy.
        /// </summary>
        /// <remarks>This is the default value of <see cref="GridCell"/>'s InPlaceEditor.EditingPolicy attached property.</remarks>
        public static InPlaceEditor.IEditingPolicy InPlaceEditingPolicy
        {
            get { return EditingPolicy.Singleton; }
        }

        private sealed class EditingPolicy : InPlaceEditor.IEditingPolicy
        {
            public static EditingPolicy Singleton = new EditingPolicy();

            private EditingPolicy()
            {
            }

            public bool QueryEditingMode(InPlaceEditor inPlaceEditor)
            {
                return GetMode(inPlaceEditor) == GridCellMode.Edit;
            }

            public bool QueryEditorElementFocus(InPlaceEditor inPlaceEditor)
            {
                return GetMode(inPlaceEditor) == GridCellMode.Edit;
            }
        }

        private static readonly DependencyPropertyKey ChildPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Child), typeof(UIElement), typeof(GridCell),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="Child"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ChildProperty = ChildPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ModePropertyKey = DependencyProperty.RegisterAttachedReadOnly(nameof(Mode), typeof(GridCellMode?), typeof(GridCell),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Identifies the Mode attached readonly property (<see cref="GetMode(UIElement)"/>).
        /// </summary>
        public static readonly DependencyProperty ModeProperty = ModePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsCurrentPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsCurrent), typeof(bool), typeof(GridCell),
            new FrameworkPropertyMetadata(BooleanBoxes.False));

        /// <summary>
        /// Identifies the <see cref="IsCurrent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsCurrentProperty = IsCurrentPropertyKey.DependencyProperty;

        static GridCell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(typeof(GridCell)));
            FocusableProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(null, CoerceFocusable));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
            Service.Register<Presenter, Presenter>();
            Service.Register<ICommandService, CommandService>();
        }

        private static object CoerceFocusable(DependencyObject d, Object baseValue)
        {
            return BooleanBoxes.Box(GetFocusable((GridCell)d));
        }

        private static bool GetFocusable(GridCell gridCell)
        {
            if (gridCell.IsKeyboardFocused)
                return true;

            if (gridCell.ContainsKeyboardFocus())
                return false;

            var dataPresenter = gridCell.DataPresenter;
            if (dataPresenter != null)
            {
                var p = dataPresenter.GetService<Presenter>(false);
                if (p != null && p.Mode == GridCellMode.Edit)   // DataView is in Edit mode
                    return gridCell.IsEditable;
            }

            return true;
        }

        /// <summary>
        /// Gets a value indicates whether child element is editable.
        /// </summary>
        public bool IsEditable
        {
            get
            {
                var child = Child;
                if (child == null)
                    return false;

                return (child.GetBinding() is RowBinding binding) && binding.IsEditable;
            }
        }

        /// <summary>
        /// Gets the contained child element. This is a dependency property.
        /// </summary>
        public UIElement Child
        {
            get { return (UIElement)GetValue(ChildProperty); }
            private set { SetValue(ChildPropertyKey, value); }
        }

        internal T GetChild<T>()
            where T : UIElement, new()
        {
            if (Child == null)
                Child = new T();
            return (T)Child;
        }

        private static class ModeBoxes
        {
            public static readonly object Select = GridCellMode.Select;
            public static readonly object Edit = GridCellMode.Edit;

            public static object Box(GridCellMode value)
            {
                return value == GridCellMode.Select ? Select : Edit;
            }
        }

        /// <summary>
        /// Gets the operation that the <see cref="GridCell"/> can perform.
        /// </summary>
        public GridCellMode? Mode
        {
            get { return (GridCellMode?)GetValue(ModeProperty); }
            private set
            {
                if (value == null)
                    ClearValue(ModePropertyKey);
                else
                    SetValue(ModePropertyKey, ModeBoxes.Box(value.GetValueOrDefault()));
            }
        }

        /// <summary>
        /// Gets the grid cell mode for specified element. This is the getter of Mode attached property.
        /// </summary>
        /// <param name="element">The specified element.</param>
        /// <returns>The grid cell mode.</returns>
        public static GridCellMode? GetMode(UIElement element)
        {
            return (GridCellMode?)element.GetValue(ModeProperty);
        }

        /// <summary>
        /// Gets a value indicates whether this is the current <see cref="GridCell"/>.
        /// </summary>
        public bool IsCurrent
        {
            get { return (bool)GetValue(IsCurrentProperty); }
            private set
            {
                if (value)
                    SetValue(IsCurrentPropertyKey, BooleanBoxes.True);
                else
                    ClearValue(IsCurrentPropertyKey);
            }
        }

        private DataPresenter DataPresenter
        {
            get { return this.GetRowPresenter()?.DataPresenter; }
        }

        /// <inheritdoc/>
        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusedChanged(e);

            var p = DataPresenter?.GetService<Presenter>();
            if (p == null)
                return;

            Refresh(p);
            if ((bool)e.NewValue)
                p.OnFocused(this);
        }

        void IRowElement.Setup(RowPresenter p)
        {
            var dataPresenter = p.DataPresenter;
            this.SetupCommandEntries(GetCommandService(p.DataPresenter), GetCommandEntries);
        }

        private static IEnumerable<CommandEntry> GetCommandEntries(ICommandService commandService, GridCell gridCell)
        {
            return commandService.GetCommandEntries(gridCell);
        }

        void IRowElement.Refresh(RowPresenter p)
        {
            Refresh(GetPresenter());
        }

        private void Refresh()
        {
            var presenter = GetPresenter();
            if (presenter != null)
                Refresh(presenter);
        }

        private void Refresh(Presenter p)
        {
            IsCurrent = p.IsCurrent(this);
            Mode = GetMode(p);
            CoerceValue(FocusableProperty);
        }

        private Presenter GetPresenter()
        {
            return DataPresenter?.GetService<Presenter>();
        }

        private GridCellMode? GetMode(Presenter p)
        {
            if (p.Mode == GridCellMode.Edit)
            {
                if (IsCurrent && ContainsPhysicalOrLogicalFocus)
                    return GridCellMode.Edit;
            }
            return p.IsSelected(this) ? new GridCellMode?(GridCellMode.Select) : null;
        }

        private bool ContainsPhysicalOrLogicalFocus
        {
            get
            {
                if (IsKeyboardFocusWithin)
                    return true;

                var focusScope = FocusManager.GetFocusScope(this);
                if (focusScope == null)
                    return false;
                return FocusManager.GetFocusedElement(focusScope) is DependencyObject focusedElement ? IsAncestorOf(focusedElement): false;
            }
        }

        void IRowElement.Cleanup(RowPresenter p)
        {
            this.CleanupCommandEntries();
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (!IsKeyboardFocusWithin && Focusable)
                Focus();
        }

        /// <inheritdoc/>
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);
            TryEnterEditMode();
        }

        private void TryEnterEditMode()
        {
            var presenter = GetPresenter();
            if (presenter == null)
                return;

            if (presenter.Mode == GridCellMode.Select && IsEditable)
                presenter.ToggleMode(this);
        }
    }
}
