using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Windows
{
    public class RowView : Control
    {
        private static readonly DependencyPropertyKey RowPresenterPropertyKey = DependencyProperty.RegisterReadOnly(nameof(RowPresenter),
            typeof(RowPresenter), typeof(RowView), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty RowPresenterProperty = RowPresenterPropertyKey.DependencyProperty;

        public RowPresenter RowPresenter
        {
            get { return (RowPresenter)GetValue(RowPresenterProperty); }
            private set { SetValue(RowPresenterPropertyKey, value); }
        }

        internal void Initialize(RowPresenter rowPresenter)
        {
            Debug.Assert(RowPresenter == null && rowPresenter != null);
            RowPresenter = rowPresenter;
        }

        internal void Cleanup()
        {
            Debug.Assert(RowPresenter != null);
            RowPresenter.RowPanel = null;
            RowPresenter = null;
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
