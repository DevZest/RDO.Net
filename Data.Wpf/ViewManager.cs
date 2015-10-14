
using System.Windows;

namespace DevZest.Data.Wpf
{
    public abstract class ViewManager
    {
        public DataSetControl Owner { get; private set; }

        public GridRange GridRange { get; private set; }

        internal ViewManager Initialize(DataSetControl owner, GridRange gridRange)
        {
            Owner = owner;
            GridRange = gridRange;
            return this;
        }

        internal void Clear()
        {
            Owner = null;
            GridRange = new GridRange();
        }

        internal abstract ViewManagerKind Kind { get; }

        internal abstract bool IsValidFor(Model model);

        internal abstract UIElement CreateView();

        internal abstract void InitUIElement(UIElement uiElement);
    }
}
