using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    internal static class VisualStates
    {
        public const string StateNormal = "Normal";

        public const string StateMouseOver = "MouseOver";

        public const string StatePressed = "Pressed";

        public const string StateDisabled = "Disabled";

        public const string StateReadOnly = "ReadOnly";

        public const string GroupCommon = "CommonStates";

        public const string StateUnfocused = "Unfocused";

        public const string StateFocused = "Focused";

        public const string GroupFocus = "FocusStates";

        public const string StateExpanded = "Expanded";

        public const string StateCollapsed = "Collapsed";

        public const string GroupExpansion = "ExpansionStates";

        public const string StateSelected = "Selected";

        public const string StateSelectedUnfocused = "SelectedUnfocused";

        public const string StateSelectedInactive = "SelectedInactive";

        public const string StateUnselected = "Unselected";

        public const string GroupSelection = "SelectionStates";

        public const string StateActive = "Active";

        public const string StateInactive = "Inactive";

        public const string GroupActive = "ActiveStates";

        public const string StateValid = "Valid";

        public const string StateInvalidFocused = "InvalidFocused";

        public const string StateInvalidUnfocused = "InvalidUnfocused";

        public const string GroupValidation = "ValidationStates";

        public const string StateUnsorted = "Unsorted";

        public const string StateSortAscending = "SortAscending";

        public const string StateSortDescending = "SortDescending";

        public const string GroupSort = "SortStates";

        public const string StateRegularRow = "Regular";

        public const string StateCurrentRow = "Current";

        public const string StateCurrentEditingRow = "CurrentEditing";

        public const string StateNewRow = "New";

        public const string StateNewCurrentRow = "NewCurrent";

        public const string StateNewEditingRow = "NewEditing";

        public const string GroupRowIndicator = "RowIndicator";

        public static void GoToState(Control control, bool useTransitions, params string[] stateNames)
        {
            if (stateNames == null)
                return;

            for (int i = 0; i < stateNames.Length; i++)
            {
                string stateName = stateNames[i];
                if (VisualStateManager.GoToState(control, stateName, useTransitions))
                    break;
            }
        }

        public static void GoToState(Control control, bool useTransitions, string stateName)
        {
            VisualStateManager.GoToState(control, stateName, useTransitions);
        }
    }
}
