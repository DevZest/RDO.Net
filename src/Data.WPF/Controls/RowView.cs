using DevZest.Data.Windows.Primitives;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Controls
{
    [TemplatePart(Name = "PART_Panel", Type = typeof(RowElementPanel))]
    public class RowView : Control
    {
        private static readonly DependencyPropertyKey RowPresenterPropertyKey = DependencyProperty.RegisterReadOnly(nameof(RowPresenter),
            typeof(RowPresenter), typeof(RowView), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty RowPresenterProperty = RowPresenterPropertyKey.DependencyProperty;

        static RowView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowView), new FrameworkPropertyMetadata(typeof(RowView)));
        }

        public RowPresenter RowPresenter
        {
            get { return (RowPresenter)GetValue(RowPresenterProperty); }
            private set { SetValue(RowPresenterPropertyKey, value); }
        }

        internal virtual void Initialize(RowPresenter rowPresenter)
        {
            Debug.Assert(RowPresenter == null && rowPresenter != null);
            RowPresenter = rowPresenter;
            SetElementPanel();
        }

        internal void Cleanup()
        {
            Debug.Assert(RowPresenter != null);
            RowPresenter.Cleanup();
            RowPresenter = null;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (RowPresenter != null)
                SetElementPanel();
        }

        private void SetElementPanel()
        {
            Debug.Assert(RowPresenter != null);
            RowPresenter.SetElementPanel(ElementPanel);
        }

        private RowElementPanel ElementPanel
        {
            get { return Template == null ? null : Template.FindName("PART_Panel", this) as RowElementPanel; }
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
