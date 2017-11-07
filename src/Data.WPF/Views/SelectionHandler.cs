using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    internal static class SelectionHandler
    {
        public static bool Select(ElementManager elementManager, MouseButton mouseButton, RowPresenter oldCurrentRow, RowPresenter newCurrentRow)
        {
            var templateSelectionMode = elementManager.Template.SelectionMode;
            if (!templateSelectionMode.HasValue)
                templateSelectionMode = SelectionMode.Extended;

            switch (templateSelectionMode.Value)
            {
                case SelectionMode.Single:
                    Select(elementManager, (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control ? SelectionMode.Multiple : SelectionMode.Single, oldCurrentRow, newCurrentRow);
                    return true;
                case SelectionMode.Multiple:
                    Select(elementManager, SelectionMode.Multiple, oldCurrentRow, newCurrentRow);
                    return true;
                case SelectionMode.Extended:
                    if (mouseButton != MouseButton.Left)
                    {
                        if (mouseButton == MouseButton.Right && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == ModifierKeys.None)
                        {
                            if (newCurrentRow.IsSelected)
                                return false;
                            Select(elementManager, SelectionMode.Single, oldCurrentRow, newCurrentRow);
                            return true;
                        }
                        return false;
                    }

                    if (IsControlDown && IsShiftDown)
                        return false;

                    var selectionMode = IsShiftDown ? SelectionMode.Extended : (IsControlDown ? SelectionMode.Multiple : SelectionMode.Single);
                    Select(elementManager, selectionMode, oldCurrentRow, newCurrentRow);
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

        private static void Select(ElementManager elementManager, SelectionMode selectionMode, RowPresenter oldCurrentRow, RowPresenter newCurrentRow)
        {
            elementManager.Select(newCurrentRow, selectionMode, oldCurrentRow);
        }
    }
}
