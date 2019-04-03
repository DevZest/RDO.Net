using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Presenters.Primitives
{
    public static class TextBoxStatus
    {
        private static readonly DependencyPropertyKey IsEditingPropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsEditing", typeof(bool), typeof(TextBoxStatus),
            new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;

        public static bool GetIsEditing(this TextBox textBox)
        {
            return (bool)textBox.GetValue(IsEditingProperty);
        }

        private static void SetIsEditing(this TextBox textBox, bool value)
        {
            if (value)
                textBox.SetValue(IsEditingPropertyKey, BooleanBoxes.True);
            else
                textBox.ClearValue(IsEditingPropertyKey);
        }

        public static void Setup(this TextBox textBox)
        {
            textBox.PreviewTextInput += TextBox_PreviewTextInput;
            textBox.LostFocus += TextBox_LostFocus;
        }

        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ((TextBox)sender).SetIsEditing(true);
        }

        private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SetIsEditing(false);
        }

        public static void Cleanup(this TextBox textBox)
        {
            textBox.PreviewTextInput -= TextBox_PreviewTextInput;
            textBox.LostFocus -= TextBox_LostFocus;
            textBox.SetIsEditing(false);
        }
    }
}
