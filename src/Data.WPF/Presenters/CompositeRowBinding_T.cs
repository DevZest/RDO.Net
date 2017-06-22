using DevZest.Data.Presenters.Primitives;
using System.Windows;
using DevZest.Data.Views.Primitives;

namespace DevZest.Data.Presenters
{
    public sealed class CompositeRowBinding<T> : CompositeRowBinding
        where T : UIElement, ICompositeView, new()
    {
        internal override ICompositeView CreateView()
        {
            return new T();
        }

        public new T this[RowPresenter rowPresenter]
        {
            get { return (T)base[rowPresenter]; }
        }

        public CompositeRowBinding<T> AddChild<TChild>(RowBinding<TChild> binding, string name)
            where TChild : UIElement, new()
        {
            InternalAddChild(binding, name);
            return this;
        }
    }
}
