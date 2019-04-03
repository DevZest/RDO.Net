using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace DevZest.Windows
{
    static partial class Extensions
    {
        // workaround bug of framework: inputElement.IsKeyboardFocusWithin == true but inputElement is not ancestor of Keyboard.FocusedElement
        // this can happen when keyboard focused UIElement is removed from visual tree, or some other edge cases.
        internal static bool ContainsKeyboardFocus<T>(this T inputElement)
            where T : DependencyObject, IInputElement
        {
            Debug.Assert(inputElement != null);
            if (inputElement.IsKeyboardFocused)
                return true;
            return (Keyboard.FocusedElement as DependencyObject)?.FindVisaulAncestor<T>() == inputElement;
        }
    }
}
