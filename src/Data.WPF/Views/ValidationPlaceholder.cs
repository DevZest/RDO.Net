using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    public class ValidationPlaceholder : Control
    {
        static ValidationPlaceholder()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ValidationPlaceholder), new FrameworkPropertyMetadata(typeof(ValidationPlaceholder)));
        }
    }
}
