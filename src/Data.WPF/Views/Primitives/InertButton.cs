using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views.Primitives
{
    /// <remarks>
    /// This button has the ability to be enabled when it's template parent is disabled:
    /// https://wpf.2000things.com/tag/isenabled/
    /// </remarks>
    internal class InertButton : Button
    {
        static InertButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InertButton), new FrameworkPropertyMetadata(typeof(InertButton)));
            IsEnabledProperty.OverrideMetadata(typeof(InertButton), new FrameworkPropertyMetadata(true, IsEnabledPropertyChanged, CoerceIsEnabled));
        }

        private static void IsEnabledPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            // Overriding PropertyChanged results in merged metadata, which is what we want--
            // the PropertyChanged logic in UIElement.IsEnabled will still get invoked.
        }

        private static object CoerceIsEnabled(DependencyObject source, object value)
        {
            return value;
        }
    }
}
