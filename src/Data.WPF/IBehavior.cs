using System.Windows;

namespace DevZest.Data.Windows
{
    public interface IBehavior<in T>
        where T : UIElement, new()
    {
        void Attach(T element);

        void Detach(T element);
    }
}
