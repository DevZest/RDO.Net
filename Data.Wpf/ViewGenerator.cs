
using System.Windows;

namespace DevZest.Data.Wpf
{
    public abstract class ViewGenerator
    {
        public abstract ViewGeneratorKind Kind { get; }

        public DataSetControl Owner { get; private set; }

        public GridRange GridRange { get; private set; }

        internal ViewGenerator Initialize(DataSetControl owner, GridRange gridRange)
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

        internal abstract bool IsValidFor(Model model);

        internal abstract UIElement CreateUIElement();

        internal abstract void InitUIElement(UIElement uiElement);
    }
}
