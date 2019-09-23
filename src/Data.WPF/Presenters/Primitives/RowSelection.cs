using DevZest.Data.Views;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DevZest.Data.Presenters.Primitives
{
    public static class RowSelection
    {
        public interface ISelector
        {
        }

        private sealed class FocusTracker : IService
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
                    _activeSelector = null;
                    if (_dataView != null)
                        _dataView.GotKeyboardFocus += OnGotKeyboardFocus;
                }
            }

            private ISelector _activeSelector;
            private ISelector ActiveSelector
            {
                get { return _activeSelector; }
                set
                {
                    if (_activeSelector == value)
                        return;
                    if (_activeSelector != null && value == null)
                        DeselectAll();
                    _activeSelector = value;
                }
            }

            private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
            {
                ActiveSelector = FindAncestorSelector(e.OriginalSource as DependencyObject);
            }

            private static ISelector FindAncestorSelector(DependencyObject child)
            {
                if (child == null)
                    return null;

                if (child is ISelector)
                    return (ISelector)child;

                DependencyObject parentObject = VisualTreeHelper.GetParent(child);
                if (parentObject == null)
                    return null;

                var parent = parentObject as ISelector;
                return parent ?? FindAncestorSelector(parentObject);
            }

            private void DeselectAll()
            {
                DataPresenter.SuspendInvalidateView();
                var rows = DataPresenter.SelectedRows.ToArray();
                foreach (var row in rows)
                    row.IsSelected = false;
                DataPresenter.ResumeInvalidateView();
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

        public static void EnsureSetup(DataPresenter dataPresenter)
        {
            if (!ServiceManager.IsRegistered<FocusTracker>())
                ServiceManager.Register<FocusTracker, FocusTracker>();
            var service = ServiceManager.GetService<FocusTracker>(dataPresenter);
            Debug.Assert(service != null);
        }
    }
}
