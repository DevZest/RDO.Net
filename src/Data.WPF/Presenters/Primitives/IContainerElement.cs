using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public interface IContainerElement
    {
        UIElement GetChild(int index);
    }
}
