using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Windows
{
    public class RowForm : Control
    {
        public RowView View { get; internal set; }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnPreviewLostKeyboardFocus(e);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            if (View != null)
                View.IsFocused = false;
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnPreviewGotKeyboardFocus(e);
            if (View != null)
                View.IsFocused = true;
        }
    }
}
