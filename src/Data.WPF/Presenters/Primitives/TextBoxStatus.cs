using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Provides attached property and extension methods for <see cref="TextBox"/> status.
    /// </summary>
    public static class TextBoxStatus
    {
        private static readonly DependencyPropertyKey IsEditingPropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsEditing", typeof(bool), typeof(TextBoxStatus),
            new FrameworkPropertyMetadata(BooleanBoxes.False));

        /// <summary>
        /// Identifies the IsEditing attached readonly property (<see cref="GetIsEditing(TextBox)"/>).
        /// </summary>
        public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the value indicates whether specified <see cref="TextBox"/> is in editing mode. This is the getter of IsEditing attached property.
        /// </summary>
        /// <param name="textBox">The specified <see cref="TextBox"/>.</param>
        /// <returns><see langword="true"/> if specified <see cref="TextBox"/> is in editing mode, otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Setup the <see cref="TextBox"/> for status management.
        /// </summary>
        /// <param name="textBox">The <see cref="TextBox"/>.</param>
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

        /// <summary>
        /// Cleanup the <see cref="TextBox"/> for status management.
        /// </summary>
        /// <param name="textBox">The <see cref="TextBox"/>.</param>
        public static void Cleanup(this TextBox textBox)
        {
            textBox.PreviewTextInput -= TextBox_PreviewTextInput;
            textBox.LostFocus -= TextBox_LostFocus;
            textBox.SetIsEditing(false);
        }
    }
}
