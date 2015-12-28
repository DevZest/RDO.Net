using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Windows
{
    public class DataRowView : Control
    {
        public DataRowPresenter Presenter { get; internal set; }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnPreviewLostKeyboardFocus(e);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            if (Presenter != null)
                Presenter.IsFocused = false;
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnPreviewGotKeyboardFocus(e);
            if (Presenter != null)
                Presenter.IsFocused = true;
        }
    }
}
