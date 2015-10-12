
using System.Windows;

namespace DevZest.Data.Wpf
{
    public abstract class ViewGenerator
    {
        public abstract ViewGeneratorKind Kind { get; }

        public GridRange GridRange { get; protected set; }

        public DataSetControl DataSetControl { get; internal set; }

        public Model Model
        {
            get { return DataSetControl == null ? null : DataSetControl.DataSet.Model; }
        }

        internal abstract UIElement CreateUIElement();

        internal virtual void Initialize(UIElement uiElement)
        {
        }

        internal virtual void Dispose(UIElement uiElement)
        {
        }
    }
}
