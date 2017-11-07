using DevZest.Data.Presenters.Primitives;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DevZest.Data.Presenters;
using System;
using System.Windows.Input;
using System.Linq;
using System.Diagnostics;

namespace DevZest.Data.Views
{
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateNormal)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateMouseOver)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateSelected)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateUnselected)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateRegularRow)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateCurrentRow)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateCurrentEditingRow)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateNewRow)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateNewCurrentRow)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateNewEditingRow)]
    public class RowHeader : ButtonBase, IRowElement, RowHeader.IAutoDeselectable
    {
        internal interface IAutoDeselectable
        {
        }

        private interface IAutoDeselectionService : IService
        {
        }

        private sealed class AutoDeselectionService : IAutoDeselectionService
        {
            private DataPresenter _dataPresenter;
            public DataPresenter DataPresenter
            {
                get { return _dataPresenter; }
            }

            private DataView _dataView;
            private DataView DataView
            {
                get { return _dataView; }
                set
                {
                    if (_dataView == value)
                        return;

                    if (_dataView != null)
                        _dataView.GotKeyboardFocus -= OnGotKeyboardFocus;
                    _dataView = value;
                    _selectable = null;
                    if (_dataView != null)
                        _dataView.GotKeyboardFocus += OnGotKeyboardFocus;
                }
            }

            private IAutoDeselectable _selectable;
            private IAutoDeselectable Selectable
            {
                get { return _selectable; }
                set
                {
                    if (_selectable == value)
                        return;
                    if (_selectable != null && value == null)
                        DeselectAll();
                    _selectable = value;
                }
            }

            private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
            {
                Selectable = FindSelectableAncestor(e.OriginalSource as DependencyObject);
            }

            private static IAutoDeselectable FindSelectableAncestor(DependencyObject child)
            {
                if (child == null)
                    return null;

                if (child is IAutoDeselectable)
                    return (IAutoDeselectable)child;

                DependencyObject parentObject = VisualTreeHelper.GetParent(child);

                //we've reached the end of the tree
                if (parentObject == null) return null;

                //check if the parent matches the type we're looking for
                var parent = parentObject as IAutoDeselectable;
                if (parent != null)
                    return parent;
                else
                    return FindSelectableAncestor(parentObject);
            }

            private void DeselectAll()
            {
                var rows = DataPresenter.SelectedRows.ToArray();
                foreach (var row in rows)
                    row.IsSelected = false;
            }

            public void Initialize(DataPresenter dataPresenter)
            {
                _dataPresenter = dataPresenter;
                DataView = dataPresenter.View;
                _dataPresenter.ViewChanged += OnViewChanged;
            }

            private void OnViewChanged(object sender, EventArgs e)
            {
                DataView = DataPresenter.View;
            }
        }

        public static readonly StyleKey FlatStyleKey = new StyleKey(typeof(RowHeader));

        private static readonly DependencyPropertyKey IsSelectedPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsSelected), typeof(bool),
            typeof(RowHeader), new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

        public static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush),
            typeof(RowHeader), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SeparatorVisibilityProperty = DependencyProperty.Register(nameof(SeparatorVisibility), typeof(Visibility),
            typeof(RowHeader), new FrameworkPropertyMetadata(Visibility.Visible));

        static RowHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowHeader), new FrameworkPropertyMetadata(typeof(RowHeader)));
        }

        internal static void EnsureAutoDeselectionServiceInitialized(DataPresenter dataPresenter)
        {
            if (!ServiceManager.IsRegistered<IAutoDeselectionService>())
                ServiceManager.Register<IAutoDeselectionService, AutoDeselectionService>();
            var service = ServiceManager.GetService<IAutoDeselectionService>(dataPresenter);
            Debug.Assert(service != null);
        }

        public RowHeader()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var rowPresenter = RowView.GetCurrent(this).RowPresenter;
            UpdateVisualStates(rowPresenter);
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            private set { SetValue(IsSelectedPropertyKey, BooleanBoxes.Box(value)); }
        }

        public Brush SeparatorBrush
        {
            get { return (Brush)GetValue(SeparatorBrushProperty); }
            set { SetValue(SeparatorBrushProperty, value); }
        }

        public Visibility SeparatorVisibility
        {
            get { return (Visibility)GetValue(SeparatorVisibilityProperty); }
            set { SetValue(SeparatorVisibilityProperty, value); }
        }

        void IRowElement.Cleanup(RowPresenter rowPresenter)
        {
        }

        void IRowElement.Refresh(RowPresenter rowPresenter)
        {
            UpdateVisualStates(rowPresenter);
        }

        void IRowElement.Setup(RowPresenter rowPresenter)
        {
            EnsureAutoDeselectionServiceInitialized(rowPresenter.DataPresenter);
        }

        private void UpdateVisualStates(RowPresenter rowPresenter)
        {
            UpdateVisualStates(rowPresenter, true);
        }

        private void UpdateVisualStates(RowPresenter rowPresenter, bool useTransitions)
        {
            if (!IsLoaded)
                return;

            if (rowPresenter.IsSelected)
            {
                IsSelected = true;
                VisualStates.GoToState(this, useTransitions, VisualStates.StateSelected);
            }
            else
            {
                IsSelected = false;
                VisualStates.GoToState(this, useTransitions, VisualStates.StateUnselected);
            }

            if (rowPresenter.IsVirtual)
            {
                if (rowPresenter.IsEditing)
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateNewEditingRow);
                else if (rowPresenter.IsCurrent)
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateNewCurrentRow);
                else
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateNewRow);
            }
            else if (rowPresenter.IsCurrent)
            {
                if (rowPresenter.IsEditing)
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateCurrentEditingRow);
                else
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateCurrentRow);
            }
            else
                VisualStates.GoToState(this, useTransitions, VisualStates.StateRegularRow);
        }
    }
}
