using System.Windows;

namespace DevZest.Data.Windows
{
    public static class BindingExtensions
    {
        public static ScalarBinding<T> WithIsMultidimensional<T>(this ScalarBinding<T> binding, bool value)
            where T : UIElement, new()
        {
            binding.IsMultidimensional = value;
            return binding;
        }
    }
}
