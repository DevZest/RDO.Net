using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Windows
{
    public class RowForm : Control
    {
        private static readonly DependencyPropertyKey ViewPropertyKey = DependencyProperty.RegisterReadOnly(nameof(View),
            typeof(RowView), typeof(RowForm), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ViewProperty = ViewPropertyKey.DependencyProperty;

        public RowView View
        {
            get { return (RowView)GetValue(ViewProperty); }
        }

        internal void Initialize(RowView view)
        {
            Debug.Assert(view != null && View == null);
            SetValue(ViewPropertyKey, view);
        }

        internal void Cleanup()
        {
            Debug.Assert(View != null);
            ClearValue(ViewPropertyKey);
        }

        //protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        //{
        //    base.OnPreviewLostKeyboardFocus(e);
        //}

        //protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        //{
        //    base.OnLostKeyboardFocus(e);
        //    if (View != null)
        //        View.IsFocused = false;
        //}

        //protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        //{
        //    base.OnPreviewGotKeyboardFocus(e);
        //    if (View != null)
        //        View.IsFocused = true;
        //}
    }
}
