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
        private static readonly DependencyPropertyKey ChildPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Child), typeof(UIElement), typeof(GridCell),
            new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ChildProperty = ChildPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ModePropertyKey = DependencyProperty.RegisterAttachedReadOnly(nameof(Mode), typeof(GridCellMode?), typeof(GridCell),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty ModeProperty = ModePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsCurrentPropertyKey = DependencyProperty.RegisterAttachedReadOnly(nameof(IsCurrent), typeof(bool), typeof(GridCell),
            new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty IsCurrentProperty = IsCurrentPropertyKey.DependencyProperty;

        static GridCell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(typeof(GridCell)));
            FocusableProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(null, CoerceFocusable));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
            ServiceManager.Register<Handler, Handler>();
        }

        private static object CoerceFocusable(DependencyObject d, Object baseValue)
        {
            return BooleanBoxes.True;
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

        public static GridCellMode? GetMode(UIElement element)
        {
            return (GridCellMode?)element.GetValue(ModeProperty);
        }

        public sealed class Handler : IService
        {
            public DataPresenter DataPresenter { get; private set; }

            void IService.Initialize(DataPresenter dataPresenter)
            {
                if (!dataPresenter.IsMounted)
                    throw new InvalidOperationException(DiagnosticMessages.DataPresenter_NotMounted);
                DataPresenter = dataPresenter;
                _gridCellBindings = Template.RowBindings.Where(x => typeof(GridCell).IsAssignableFrom(x.ViewType)).ToArray();
                if (_gridCellBindings.Length > 0)
                    _current = 0;
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
                    _extendedSelection = 0;
                    DataPresenter.InvalidateView();
                }
            }

            private int _current;
            private int _extendedSelection;

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
                    throw new ArgumentException("", paramName);
                return index;
            }

            public bool IsSelected(GridCell gridCell)
            {
                if (Mode == GridCellMode.Edit)
                    return false;

                var index = VerifyGridCell(gridCell, nameof(gridCell));
                var startIndex = Math.Min(_current, _current + _extendedSelection);
                var endIndex = Math.Max(_current, _current + _extendedSelection);
                return index >= startIndex && index <= endIndex && gridCell.GetRowPresenter().IsSelected;
            }

            public bool IsCurrent(GridCell gridCell)
            {
                var index = VerifyGridCell(gridCell, nameof(gridCell));
                return _current == index && gridCell.GetRowPresenter().IsCurrent;
            }

            public void Select(GridCell gridCell, bool isExtended)
            {
                if (Mode != GridCellMode.Select)
                    throw new InvalidOperationException(DiagnosticMessages.GridCell_Handler_SelectionNotAllowedInEditMode);

                var index = VerifyGridCell(gridCell, nameof(gridCell));

                SuspendInvalidateView();

                if (isExtended)
                    _extendedSelection += _current - index;
                else
                    _extendedSelection = 0;

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

                Debug.Assert(_current == index);
                InvalidateView();
                ResumeInvalidateView();
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
                var index = IndexOf(gridCell);
                Debug.Assert(index >= 0 && index < _gridCellBindings.Length);
                _current = index;
                if (!_isExtendedSelectionLocked)
                    _extendedSelection = 0;

                InvalidateView();
            }
        }

        private DataPresenter DataPresenter
        {
            get { return this.GetRowPresenter()?.DataPresenter; }
        }

        protected override void OnIsKeyboardFocusedChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusedChanged(e);
            if ((bool)e.NewValue)
                DataPresenter?.GetService<Handler>().OnFocused(this);
        }

        void IRowElement.Setup(RowPresenter p)
        {
        }

        void IRowElement.Refresh(RowPresenter p)
        {
            var handler = DataPresenter.GetService<Handler>();
            if (handler.Mode == GridCellMode.Edit)
                Mode = handler.IsCurrent(this) ? new GridCellMode?(GridCellMode.Edit) : null;
            else
                Mode = handler.IsSelected(this) ? new GridCellMode?(GridCellMode.Select) : null;
            IsCurrent = handler.IsCurrent(this);
        }

        void IRowElement.Cleanup(RowPresenter p)
        {
        }
    }
}
