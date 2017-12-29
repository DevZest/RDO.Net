using DevZest.Data.Presenters;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    public class ValidationMessagesControl : ItemsControl
    {
        public static class Templates
        {
            public static readonly TemplateId ValidationErrorId = new TemplateId(typeof(ValidationMessagesControl));
            public static readonly TemplateId ValidationWarningId = new TemplateId(typeof(ValidationMessagesControl));

            public static ControlTemplate ValidationError
            {
                get { return ValidationErrorId.GetOrLoad(); }
            }

            public static ControlTemplate ValidationWarning
            {
                get { return ValidationWarningId.GetOrLoad(); }
            }
        }

        private static readonly DependencyPropertyKey ItemsCountPropertyKey;
        public static readonly DependencyProperty ItemsCountProperty;

        static ValidationMessagesControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ValidationMessagesControl), new FrameworkPropertyMetadata(typeof(ValidationMessagesControl)));
            ItemsCountPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ItemsCount), typeof(int), typeof(ValidationMessagesControl), new FrameworkPropertyMetadata(0));
            ItemsCountProperty = ItemsCountPropertyKey.DependencyProperty;
        }

        public int ItemsCount
        {
            get { return (int)GetValue(ItemsCountProperty); }
            private set
            {
                if (value == 0)
                    ClearValue(ItemsCountPropertyKey);
                else
                    SetValue(ItemsCountPropertyKey, value);
            }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            ItemsCount = Items == null ? 0 : Items.Count;
        }
    }
}
