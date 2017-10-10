using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class ScalarBindingBase<T> : ScalarBinding
        where T : UIElement, new()
    {
        public new T this[int flowIndex]
        {
            get { return (T)base[flowIndex]; }
        }
    }
}
