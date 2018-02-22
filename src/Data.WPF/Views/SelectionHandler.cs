using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
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

        public static bool Select(DataPresenter dataPresenter, MouseButton mouseButton, RowPresenter row)
        {
            if (dataPresenter.EditingRow != null)
                return false;

            var templateSelectionMode = dataPresenter.Template.SelectionMode;
            if (!templateSelectionMode.HasValue)
                templateSelectionMode = SelectionMode.Extended;

            switch (templateSelectionMode.Value)
            {
                case SelectionMode.Single:
                    dataPresenter.Select(row, SelectionMode.Single);
                    return true;
                case SelectionMode.Multiple:
                    dataPresenter.Select(row, SelectionMode.Multiple);
                    return true;
                case SelectionMode.Extended:
                    if (mouseButton != MouseButton.Left)
                    {
                        if (mouseButton == MouseButton.Right && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == ModifierKeys.None)
                        {
                            if (row.IsSelected)
                                return false;
                            dataPresenter.Select(row, SelectionMode.Single);
                            return true;
                        }
                        return false;
                    }

                    if (IsControlDown && IsShiftDown)
                        return false;

                    var selectionMode = IsShiftDown ? SelectionMode.Extended : (IsControlDown ? SelectionMode.Multiple : SelectionMode.Single);
                    dataPresenter.Select(row, selectionMode);
                    return true;
            }
            return false;
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
