using DevZest.Data.Views;
using DevZest.Data.Presenters.Primitives;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public sealed class BlockPane<T> : BlockPane
        where T : Pane, new()
    {
        internal override Pane CreatePane()
        {
            return new T();
        }

        public new T this[int blockOrdinal]
        {
            get { return (T)base[blockOrdinal]; }
        }

        public BlockPane<T> AddChild<TChild>(BlockBinding<TChild> binding, string name)
            where TChild : UIElement, new()
        {
            InternalAddChild(binding, name);
            return this;
        }
    }
}
