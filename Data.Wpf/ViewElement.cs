using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Wpf
{
    internal class ViewElement
    {
        internal ViewElement(ViewItem owner, UIElement uiElement)
        {
            Debug.Assert(owner != null);
            Debug.Assert(uiElement != null);
            Owner = owner;
            UIElement = uiElement;
        }

        public ViewItem Owner { get; private set; }

        public UIElement UIElement { get; private set; }

        public Rect FinalRect { get; private set; }
    }
}
