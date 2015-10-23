using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    internal class GridElement
    {
        internal GridElement(GridItem owner, UIElement uiElement)
        {
            Debug.Assert(owner != null);
            Debug.Assert(uiElement != null);
            Owner = owner;
            UIElement = uiElement;
        }

        public GridItem Owner { get; private set; }

        public UIElement UIElement { get; private set; }

        public Rect FinalRect { get; private set; }
    }
}
