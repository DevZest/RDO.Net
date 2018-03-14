using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    public class GridCell : Control, IRowElement
    {
        public static InPlaceEditor.ISwitcher InPlaceEditorSwitcher
        {
            get { return Switcher.Singleton; }
        }

        private sealed class Switcher : InPlaceEditor.ISwitcher
        {
            public static Switcher Singleton = new Switcher();

            private Switcher()
            {
            }

            public bool GetIsEditing(InPlaceEditor inPlaceEditor)
            {
                return GetMode(inPlaceEditor) == GridCellMode.Edit || GetPreviewMode(inPlaceEditor) == GridCellMode.Edit;
            }

            public bool ShouldFocusToEditorElement(InPlaceEditor inPlaceEditor)
            {
                return GetMode(inPlaceEditor) == GridCellMode.Edit;
            }
        }

        private static readonly DependencyPropertyKey ChildPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Child), typeof(UIElement), typeof(GridCell),
            new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ChildProperty = ChildPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ModePropertyKey = DependencyProperty.RegisterAttachedReadOnly(nameof(Mode), typeof(GridCellMode?), typeof(GridCell),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty ModeProperty = ModePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey PreviewModePropertyKey = DependencyProperty.RegisterAttachedReadOnly(nameof(PreviewMode), typeof(GridCellMode?), typeof(GridCell),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty PreviewModeProperty = PreviewModePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsCurrentPropertyKey = DependencyProperty.RegisterAttachedReadOnly(nameof(IsCurrent), typeof(bool), typeof(GridCell),
            new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty IsCurrentProperty = IsCurrentPropertyKey.DependencyProperty;

        static GridCell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(typeof(GridCell)));
            FocusableProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(null, CoerceFocusable));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
            ServiceManager.Register<Presenter, Presenter>();
        }

        private static object CoerceFocusable(DependencyObject d, Object baseValue)
        {
            return BooleanBoxes.Box(GetFocusable((GridCell)d));
        }

        private static bool GetFocusable(GridCell gridCell)
        {
            if (gridCell.IsKeyboardFocused)
                return true;

            if (gridCell.IsKeyboardFocusWithin)
                return false;

            var dataPresenter = gridCell.DataPresenter;
            if (dataPresenter != null)
            {
                var p = dataPresenter.GetService<Presenter>(false);
                if (p != null && p.Mode == GridCellMode.Edit)   // DataView is in Edit mode
                    return IsEditable(gridCell);
            }

            return true;
        }

        private static bool IsEditable(GridCell gridCell)
        {
            var child = gridCell.Child;
            if (child == null)
                return false;

            var binding = child.GetBinding() as RowBinding;
            return binding != null && binding.IsEditable;
        }

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

        public static GridCellMode? GetMode(UIElement element)
        {
            return (GridCellMode?)element.GetValue(ModeProperty);
        }

        public GridCellMode? PreviewMode
        {
            get { return (GridCellMode?)GetValue(PreviewModeProperty); }
            private set
            {
                if (value == null)
                    ClearValue(PreviewModePropertyKey);
                else
                    SetValue(PreviewModePropertyKey, ModeBoxes.Box(value.GetValueOrDefault()));
            }
        }

        public static GridCellMode? GetPreviewMode(UIElement element)
        {
            return (GridCellMode?)element.GetValue(PreviewModeProperty);
        }

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

        public sealed class Presenter : IService
        {
            public DataPresenter DataPresenter { get; private set; }

            void IService.Initialize(DataPresenter dataPresenter)
            {
                if (!dataPresenter.IsMounted)
                    throw new InvalidOperationException(DiagnosticMessages.DataPresenter_NotMounted);
                DataPresenter = dataPresenter;
                _gridCellBindings = Template.RowBindings.Where(x => typeof(GridCell).IsAssignableFrom(x.ViewType)).ToArray();
            }

            private Template Template
            {
                get { return DataPresenter.Template; }
            }

            private RowBinding[] _gridCellBindings;

            private GridCellMode _mode = GridCellMode.Edit;
            public GridCellMode Mode
            {
                get { return _mode; }
                set
                {
                    if (_mode == value)
                        return;

                    if (_mode == GridCellMode.Edit && DataPresenter.IsEditing)
                        DataPresenter.EditingRow.CancelEdit();
                    _mode = value;
                    _extendedBindingSelection = _extendedRowSelection = 0;
                    DataPresenter.InvalidateView();
                }
            }

            private int _currentBinding = -1;
            private int _extendedBindingSelection;
            private RowPresenter _currentRow;
            private int _extendedRowSelection;

            private int IndexOf(GridCell gridCell)
            {
                var binding = gridCell.GetBinding();
                return IndexOf(binding);
            }

            private int IndexOf(Binding binding)
            {
                if (binding == null)
                    return -1;
                if (binding.Template != Template)
                    return -1;
                return Array.IndexOf(_gridCellBindings, binding);
            }

            private int VerifyGridCell(GridCell gridCell, string paramName)
            {
                Check.NotNull(gridCell, paramName);
                var index = IndexOf(gridCell);
                if (index < 0)
                    throw new ArgumentException(DiagnosticMessages.GridCell_Presenter_VerifyGridCell, paramName);
                return index;
            }

            public bool IsSelected(GridCell gridCell)
            {
                if (DataPresenter.SelectedRows.Count > 0)
                    return gridCell.GetRowPresenter().IsSelected;

                if (Mode == GridCellMode.Edit)
                    return false;

                if (_currentBinding < 0)
                    return false;

                var bindingIndex = VerifyGridCell(gridCell, nameof(gridCell));
                return IsBindingSelected(bindingIndex) && IsRowSelected(gridCell.GetRowPresenter().Index);
            }

            private bool IsBindingSelected(int bindingIndex)
            {
                var extended = _currentBinding + _extendedBindingSelection;
                var startIndex = Math.Min(_currentBinding, extended);
                var endIndex = Math.Max(_currentBinding, extended);
                return bindingIndex >= startIndex && bindingIndex <= endIndex;
            }

            private bool IsRowSelected(int rowIndex)
            {
                if (_currentRow == null)
                    return false;
                var currentRowIndex = _currentRow.Index;
                var extended = currentRowIndex + _extendedRowSelection;
                var startIndex = Math.Min(currentRowIndex, extended);
                var endIndex = Math.Max(currentRowIndex, extended);
                return rowIndex >= startIndex && rowIndex <= endIndex;
            }

            public bool IsCurrent(GridCell gridCell)
            {
                var index = VerifyGridCell(gridCell, nameof(gridCell));
                if (!gridCell.IsKeyboardFocusWithin && DataPresenter.View.IsKeyboardFocusWithin)    // Another element in DataView has keyboard focus
                    return false;
                return _currentBinding == index && gridCell.GetRowPresenter() == _currentRow;
            }

            public void Select(GridCell gridCell, bool isExtended)
            {
                if (Mode != GridCellMode.Select)
                    throw new InvalidOperationException(DiagnosticMessages.GridCell_Presenter_SelectionNotAllowedInEditMode);

                var bindingIndex = VerifyGridCell(gridCell, nameof(gridCell));

                SuspendInvalidateView();

                DeselectAllRows();

                if (_currentBinding == -1)
                    _currentBinding = bindingIndex;
                if (_currentRow == null)
                    _currentRow = gridCell.GetRowPresenter();

                if (isExtended)
                    _extendedBindingSelection += _currentBinding - bindingIndex;
                else
                    _extendedBindingSelection = 0;

                if (!gridCell.IsKeyboardFocusWithin)
                {
                    _isExtendedSelectionLocked = true;
                    try
                    {
                        gridCell.Focus();
                    }
                    finally
                    {
                        _isExtendedSelectionLocked = false;
                    }
                }

                Debug.Assert(_currentBinding == bindingIndex);
                InvalidateView();
                ResumeInvalidateView();
            }

            private void DeselectAllRows()
            {
                var rows = DataPresenter.SelectedRows;
                if (rows.Count == 0)
                    return;

                foreach (var row in rows.ToArray())
                    row.IsSelected = false;
            }

            private void InvalidateView()
            {
                DataPresenter.InvalidateView();
            }

            private void SuspendInvalidateView()
            {
                DataPresenter.SuspendInvalidateView();
            }

            private void ResumeInvalidateView()
            {
                DataPresenter.ResumeInvalidateView();
            }

            private bool _isExtendedSelectionLocked;
            internal void OnFocused(GridCell gridCell)
            {
                if (Template.SelectionMode == null)
                    DeselectAllRows();

                var index = IndexOf(gridCell);
                Debug.Assert(index >= 0 && index < _gridCellBindings.Length);
                _currentBinding = index;
                _currentRow = gridCell.GetRowPresenter();
                if (!_isExtendedSelectionLocked)
                    _extendedBindingSelection = _extendedRowSelection = 0;

                InvalidateView();
            }
        }

        private DataPresenter DataPresenter
        {
            get { return this.GetRowPresenter()?.DataPresenter; }
        }

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
        }

        void IRowElement.Refresh(RowPresenter p)
        {
            Refresh(GetPresenter());
        }

        private void Refresh(Presenter p)
        {
            IsCurrent = p.IsCurrent(this);
            Mode = GetMode(p);
            CoerceValue(FocusableProperty);
            RefreshPreviewMode(p);
        }

        private Presenter GetPresenter()
        {
            return DataPresenter?.GetService<Presenter>();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == IsMouseOverProperty)
            {
                var p = GetPresenter();
                if (p != null)
                    RefreshPreviewMode(p);
            }
        }

        private void RefreshPreviewMode(Presenter p)
        {
            if (Focusable && IsMouseOver)
                PreviewMode = p.Mode == GridCellMode.Edit ? GridCellMode.Edit : GridCellMode.Select;
            else
                PreviewMode = null;
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
                var focusedElement = FocusManager.GetFocusedElement(focusScope) as DependencyObject;
                if (focusedElement == null)
                    return false;
                return IsAncestorOf(focusedElement);
            }
        }

        void IRowElement.Cleanup(RowPresenter p)
        {
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (!IsKeyboardFocusWithin && Focusable)
                Focus();
        }
    }
}
