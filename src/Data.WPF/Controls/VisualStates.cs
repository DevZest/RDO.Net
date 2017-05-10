using System.Windows;
using System.Windows.Controls;

namespace DevZest.Windows.Controls
{
    internal static class VisualStates
    {
        public const string StateNormal = "Normal";

        public const string StateMouseOver = "MouseOver";

        public const string StatePressed = "Pressed";

        public const string StateDisabled = "Disabled";

        public const string StateReadOnly = "ReadOnly";

        internal const string StateDeterminate = "Determinate";

        public const string GroupCommon = "CommonStates";

        public const string StateUnfocused = "Unfocused";

        public const string StateFocused = "Focused";

        public const string StateFocusedDropDown = "FocusedDropDown";

        public const string GroupFocus = "FocusStates";

        public const string StateExpanded = "Expanded";

        public const string StateCollapsed = "Collapsed";

        public const string GroupExpansion = "ExpansionStates";

        public const string StateOpen = "Open";

        public const string StateClosed = "Closed";

        public const string GroupOpen = "OpenStates";

        public const string StateHasItems = "HasItems";

        public const string StateNoItems = "NoItems";

        public const string GroupHasItems = "HasItemsStates";

        public const string StateExpandDown = "ExpandDown";

        public const string StateExpandUp = "ExpandUp";

        public const string StateExpandLeft = "ExpandLeft";

        public const string StateExpandRight = "ExpandRight";

        public const string GroupExpandDirection = "ExpandDirectionStates";

        public const string StateSelected = "Selected";

        public const string StateSelectedUnfocused = "SelectedUnfocused";

        public const string StateSelectedInactive = "SelectedInactive";

        public const string StateUnselected = "Unselected";

        public const string GroupSelection = "SelectionStates";

        public const string StateEditable = "Editable";

        public const string StateUneditable = "Uneditable";

        public const string GroupEdit = "EditStates";

        public const string StateActive = "Active";

        public const string StateInactive = "Inactive";

        public const string GroupActive = "ActiveStates";

        public const string StateValid = "Valid";

        public const string StateInvalidFocused = "InvalidFocused";

        public const string StateInvalidUnfocused = "InvalidUnfocused";

        public const string GroupValidation = "ValidationStates";

        public const string StateUnwatermarked = "Unwatermarked";

        public const string StateWatermarked = "Watermarked";

        public const string GroupWatermark = "WatermarkStates";

        public const string StateChecked = "Checked";

        public const string StateUnchecked = "Unchecked";

        public const string StateIndeterminate = "Indeterminate";

        public const string GroupCheck = "CheckStates";

        public const string StateRegular = "Regular";

        public const string StateCurrent = "Current";

        public const string GroupCurrent = "CurrentStates";

        public const string StateDisplay = "Display";

        public const string StateEditing = "Editing";

        public const string GroupInteraction = "InteractionStates";

        public const string StateUnsorted = "Unsorted";

        public const string StateSortAscending = "SortAscending";

        public const string StateSortDescending = "SortDescending";

        public const string GroupSort = "SortStates";

        public const string DATAGRIDROW_stateAlternate = "Normal_AlternatingRow";

        public const string DATAGRIDROW_stateMouseOver = "MouseOver";

        public const string DATAGRIDROW_stateMouseOverEditing = "MouseOver_Unfocused_Editing";

        public const string DATAGRIDROW_stateMouseOverEditingFocused = "MouseOver_Editing";

        public const string DATAGRIDROW_stateMouseOverSelected = "MouseOver_Unfocused_Selected";

        public const string DATAGRIDROW_stateMouseOverSelectedFocused = "MouseOver_Selected";

        public const string DATAGRIDROW_stateNormal = "Normal";

        public const string DATAGRIDROW_stateNormalEditing = "Unfocused_Editing";

        public const string DATAGRIDROW_stateNormalEditingFocused = "Normal_Editing";

        public const string DATAGRIDROW_stateSelected = "Unfocused_Selected";

        public const string DATAGRIDROW_stateSelectedFocused = "Normal_Selected";

        public const string DATAGRIDROWHEADER_stateMouseOver = "MouseOver";

        public const string DATAGRIDROWHEADER_stateMouseOverCurrentRow = "MouseOver_CurrentRow";

        public const string DATAGRIDROWHEADER_stateMouseOverEditingRow = "MouseOver_Unfocused_EditingRow";

        public const string DATAGRIDROWHEADER_stateMouseOverEditingRowFocused = "MouseOver_EditingRow";

        public const string DATAGRIDROWHEADER_stateMouseOverSelected = "MouseOver_Unfocused_Selected";

        public const string DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRow = "MouseOver_Unfocused_CurrentRow_Selected";

        public const string DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRowFocused = "MouseOver_CurrentRow_Selected";

        public const string DATAGRIDROWHEADER_stateMouseOverSelectedFocused = "MouseOver_Selected";

        public const string DATAGRIDROWHEADER_stateNormal = "Normal";

        public const string DATAGRIDROWHEADER_stateNormalCurrentRow = "Normal_CurrentRow";

        public const string DATAGRIDROWHEADER_stateNormalEditingRow = "Unfocused_EditingRow";

        public const string DATAGRIDROWHEADER_stateNormalEditingRowFocused = "Normal_EditingRow";

        public const string DATAGRIDROWHEADER_stateSelected = "Unfocused_Selected";

        public const string DATAGRIDROWHEADER_stateSelectedCurrentRow = "Unfocused_CurrentRow_Selected";

        public const string DATAGRIDROWHEADER_stateSelectedCurrentRowFocused = "Normal_CurrentRow_Selected";

        public const string DATAGRIDROWHEADER_stateSelectedFocused = "Normal_Selected";

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
