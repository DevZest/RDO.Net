using DevZest.Data.Presenters;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    public class GridCell : Control
    {
        private static readonly DependencyPropertyKey ChildPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Child), typeof(UIElement), typeof(GridCell),
            new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ChildProperty = ChildPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ModePropertyKey = DependencyProperty.RegisterAttachedReadOnly(nameof(Mode), typeof(GridCellMode?), typeof(GridCell),
            new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ModeProperty = ModePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsCurrentPropertyKey = DependencyProperty.RegisterAttachedReadOnly(nameof(IsCurrent), typeof(bool), typeof(GridCell),
            new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty IsCurrentProperty = IsCurrentPropertyKey.DependencyProperty;

        static GridCell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(typeof(GridCell)));
            ServiceManager.Register<Handler, Handler>();
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
            }

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
                    DataPresenter.InvalidateView();
                }
            }
        }
    }
}
