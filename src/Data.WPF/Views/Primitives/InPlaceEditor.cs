using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views.Primitives
{
    public abstract class InPlaceEditor : ContentControl
    {
        protected abstract FrameworkElement GenerateElement();

        protected abstract FrameworkElement GenerateEditingElement();
    }
}
