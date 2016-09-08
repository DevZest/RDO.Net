using DevZest.Data.Windows.Primitives;
using DevZest.Data.Windows.Utilities;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        private static RowView Focused { get; set; }

        internal static RowPresenter FocusedRow
        {
            get { return Focused == null ? null : Focused.RowPresenter; }
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            Focused = this;
            base.OnGotKeyboardFocus(e);
        }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            Debug.Assert(Focused == this);
            if (ShouldCommitEditing(e.NewFocus))
            {
                RowPresenter.RowManager.CommitEdit();
            }
            base.OnPreviewLostKeyboardFocus(e);
        }

        private bool ShouldCommitEditing(IInputElement newFocus)
        {
            if (RowPresenter == null || !RowPresenter.IsEditing)
                return false;

            var element = newFocus as DependencyObject;
            if (element == null)
                return true;

            if (this.Contains(element))
                return false;

            var currentFocusScope = FocusManager.GetFocusScope(this);
            var newFocusScope = FocusManager.GetFocusScope((DependencyObject)element);
            return !currentFocusScope.Contains(newFocusScope);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            Focused = null;
            base.OnLostKeyboardFocus(e);
        }
    }
}
