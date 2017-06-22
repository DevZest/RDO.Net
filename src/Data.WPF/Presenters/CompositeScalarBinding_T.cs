using DevZest.Data.Presenters.Primitives;
using System.Windows;
using DevZest.Data.Views.Primitives;

namespace DevZest.Data.Presenters
{
    public sealed class CompositeScalarBinding<T> : CompositeScalarBinding
        where T : UIElement, ICompositeView, new()
    {
        internal override ICompositeView CreateView()
        {
            return new T();
        }

        public new T this[int flowIndex]
        {
            get { return (T)base[flowIndex]; }
        }

        public CompositeScalarBinding<T> AddChild<TChild>(ScalarBinding<TChild> binding, string name)
            where TChild : UIElement, new()
        {
            InternalAddChild(binding, name);
            return this;
        }
    }
}
