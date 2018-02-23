using DevZest.Data.Presenters;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.Diagnostics;

namespace DevZest.Data.Views
{
    internal static class SelectionHandler
    {
        private interface ISelectionService : IService
        {
            void SuspendCoerceSelection();
            void ResumeCoerceSelection();
        }

        private sealed class SelectionService : ISelectionService
        {
            public DataPresenter DataPresenter { get; private set; }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
                dataPresenter.ViewInvalidated += OnViewInvalidated;
            }

            private RowPresenter CurrentRow
            {
                get { return DataPresenter.CurrentRow; }
            }

            private void OnViewInvalidated(object sender, EventArgs e)
            {
                if (ShouldCoerceSelection)
                    DataPresenter.Select(CurrentRow);
            }

            private int _suspendCoerceSelectionCount;
            public void SuspendCoerceSelection()
            {
                if (_suspendCoerceSelectionCount == 0)
                    DataPresenter.ViewInvalidated -= OnViewInvalidated;
                _suspendCoerceSelectionCount++;
            }

            public void ResumeCoerceSelection()
            {
                Debug.Assert(_suspendCoerceSelectionCount > 0);
                _suspendCoerceSelectionCount--;
                if (_suspendCoerceSelectionCount == 0)
                    DataPresenter.ViewInvalidated += OnViewInvalidated;
            }

            private bool ShouldCoerceSelection
            {
                get
                {
                    if (CurrentRow == null)
                        return false;

                    if (!CurrentRow.IsSelected)
                        return true;

                    if (CurrentRow.IsEditing)
                    {
                        var selectedRows = DataPresenter.SelectedRows;
                        if (selectedRows.Count != 1)
                            return true;
                    }

                    return false;
                }
            }
        }

        static SelectionHandler()
        {
            ServiceManager.Register<ISelectionService, SelectionService>();
        }

        public static void EnsureInitialized(DataPresenter dataPresenter)
        {
            Debug.Assert(dataPresenter != null);
            var service = dataPresenter.GetService<ISelectionService>();
            Debug.Assert(service != null);
        }

        private static void SuspendCoerceSelection(DataPresenter dataPresenter)
        {
            dataPresenter.GetService<ISelectionService>().SuspendCoerceSelection();
        }

        private static void ResumeCoerceSelection(DataPresenter dataPresenter)
        {
            dataPresenter.GetService<ISelectionService>().ResumeCoerceSelection();
        }

        public static void Select(DataPresenter dataPresenter, MouseButton mouseButton, RowPresenter row, Action beforeSelecting)
        {
            if (dataPresenter.EditingRow != null)
                return;

            var selectionMode = GetSelectionMode(dataPresenter, mouseButton, row);
            if (selectionMode.HasValue)
            {
                SuspendCoerceSelection(dataPresenter);
                dataPresenter.Select(row, selectionMode.GetValueOrDefault(), true, beforeSelecting);
                ResumeCoerceSelection(dataPresenter);
            }
        }

        private static SelectionMode? GetSelectionMode(DataPresenter dataPresenter, MouseButton mouseButton, RowPresenter row)
        {
            var templateSelectionMode = dataPresenter.Template.SelectionMode;
            if (!templateSelectionMode.HasValue)
                templateSelectionMode = SelectionMode.Extended;

            switch (templateSelectionMode.Value)
            {
                case SelectionMode.Single:
                    return SelectionMode.Single;
                case SelectionMode.Multiple:
                    return SelectionMode.Multiple;
                case SelectionMode.Extended:
                    if (mouseButton != MouseButton.Left)
                    {
                        if (mouseButton == MouseButton.Right && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == ModifierKeys.None)
                        {
                            if (row.IsSelected)
                                return null;
                            return SelectionMode.Single;
                        }
                        return null;
                    }

                    if (IsControlDown && IsShiftDown)
                        return null;

                    return IsShiftDown ? SelectionMode.Extended : (IsControlDown ? SelectionMode.Multiple : SelectionMode.Single);
            }
            return null;
        }

        private static bool IsControlDown
        {
            get { return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control; }
        }

        private static bool IsShiftDown
        {
            get { return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift; }
        }
    }
}
