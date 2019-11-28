using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace DevZest.Data.Tools
{
    public static class TextBoxPlaceholderService
    {
        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.RegisterAttached(
           "PlaceholderText",
           typeof(string),
           typeof(TextBoxPlaceholderService),
           new FrameworkPropertyMetadata((object)null, new PropertyChangedCallback(OnPlaceholderTextChanged)));

        public static string GetPlaceholderText(TextBox textBox)
        {
            return (string)textBox.GetValue(PlaceholderTextProperty);
        }

        public static void SetPlaceholderText(TextBox textBox, string value)
        {
            textBox.SetValue(PlaceholderTextProperty, value);
        }

        private static void OnPlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = (TextBox)d;

            textBox.Loaded += TextBox_Loaded;
            textBox.GotKeyboardFocus += TextBox_GotKeyboardFocus;
            textBox.LostKeyboardFocus += TextBox_Loaded;
            textBox.TextChanged += TextBox_GotKeyboardFocus;
        }

        private static void TextBox_GotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (ShouldShowWatermark(textBox))
                ShowWatermark(textBox);
            else
                RemoveWatermark(textBox);
        }

        private static void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (ShouldShowWatermark(textBox))
                ShowWatermark(textBox);
        }


        #region Helper Methods

        private static void RemoveWatermark(UIElement uiElement)
        {
            var watermarkAdorner = GetWatermarkAdorner(uiElement);
            if (watermarkAdorner != null)
                watermarkAdorner.Visibility = Visibility.Hidden;
        }

        private static WatermarkAdorner GetWatermarkAdorner(UIElement uiElement)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(uiElement);
            if (layer == null)
                return null;

            Adorner[] adorners = layer.GetAdorners(uiElement);
            if (adorners == null)
                return null;

            foreach (Adorner adorner in adorners)
            {
                if (adorner is WatermarkAdorner result)
                    return result;
            }

            return null;
        }

        private static void ShowWatermark(TextBox textBox)
        {
            var watermarkAdorner = GetWatermarkAdorner(textBox);
            if (watermarkAdorner != null)
            {
                watermarkAdorner.Visibility = Visibility.Visible;
                return;
            }

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(textBox);

            // layer could be null if control is no longer in the visual tree
            if (layer != null)
                layer.Add(new WatermarkAdorner(textBox, GetPlaceholderText(textBox)));
        }

        private static bool ShouldShowWatermark(TextBox textBox)
        {
            return !textBox.IsKeyboardFocusWithin && string.IsNullOrEmpty(textBox.Text);
        }

        #endregion
    }
}
