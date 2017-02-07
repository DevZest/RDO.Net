using DevZest.Data.Windows.Primitives;
using System.Windows;

namespace DevZest.Data.Windows
{
    public static class BindingExtensions
    {
        public static T WithStyle<T>(this T binding, Style value)
            where T : Binding
        {
            binding.VerifyNotSealed();
            binding.Style = value;
            return binding;
        }

        public static T WithAutoSizeOrder<T>(this T binding, int value)
            where T : Binding
        {
            binding.VerifyNotSealed();
            binding.AutoSizeOrder = value;
            return binding;
        }

        public static T WithAutoSizeWaiver<T>(this T binding, AutoSizeWaiver value)
            where T : Binding
        {
            binding.VerifyNotSealed();
            binding.AutoSizeWaiver = value;
            return binding;
        }

    }
}
