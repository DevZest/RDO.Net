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
    public partial class GridCell : Control, IRowElement
    {
        public static InPlaceEditor.IPresenter InPlaceEditorPresenter
        {
            get { return EditorPresenter.Singleton; }
        }

        private sealed class EditorPresenter : InPlaceEditor.IPresenter
        {
            public static EditorPresenter Singleton = new EditorPresenter();

            private EditorPresenter()
            {
            }

            public bool QueryEditingMode(InPlaceEditor inPlaceEditor)
            {
                return GetMode(inPlaceEditor) == GridCellMode.Edit || GetPreviewMode(inPlaceEditor) == GridCellMode.Edit;
            }

            public bool QueryEditorElementFocus(InPlaceEditor inPlaceEditor)
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
            ServiceManager.Register<ICommandService, CommandService>();
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

        public void Refresh()
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
                return FocusManager.GetFocusedElement(focusScope) is DependencyObject focusedElement ? IsAncestorOf(focusedElement): false;
            }
        }

        void IRowElement.Cleanup(RowPresenter p)
        {
            this.CleanupCommandEntries();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (!IsKeyboardFocusWithin && Focusable)
                Focus();
        }

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
