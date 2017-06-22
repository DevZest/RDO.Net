using DevZest.Data.Views;
using DevZest.Data.Presenters.Primitives;
using System.Windows;
using DevZest.Data.Views.Primitives;

namespace DevZest.Data.Presenters
{
    public sealed class CompositeBlockBinding<T> : CompositeBlockBinding
        where T : UIElement, ICompositeView, new()
    {
        internal override ICompositeView CreateView()
        {
            return new T();
        }

        public new T this[int blockOrdinal]
        {
            get { return (T)base[blockOrdinal]; }
        }

        public CompositeBlockBinding<T> AddChild<TChild>(BlockBinding<TChild> binding, string name)
            where TChild : UIElement, new()
        {
            InternalAddChild(binding, name);
            return this;
        }
    }
}
