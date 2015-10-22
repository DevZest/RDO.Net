using DevZest.Data.Primitives;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public abstract class ChildSetGridItem : GridItem
    {
        internal ChildSetGridItem(Model childModel, GridTemplate template)
            : base(childModel.GetParentModel())
        {
            ChildModel = childModel;
        }

        public Model ChildModel { get; private set; }

        public GridTemplate Template { get; private set; }

        internal abstract UIElement Generate(DataSetView view);
    }
}
