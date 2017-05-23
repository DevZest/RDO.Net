using System;
using DevZest.Windows.Primitives;
using System.Windows;
using DevZest.Windows.Controls;

namespace DevZest.Windows
{
    public sealed class RowPane<T> : RowPane
        where T : Pane, new()
    {
        internal override Pane CreatePane()
        {
            return new T();
        }

        public new T this[RowPresenter rowPresenter]
        {
            get { return (T)base[rowPresenter]; }
        }

        public RowPane<T> AddChild<TChild>(RowBinding<TChild> binding, string name)
            where TChild : UIElement, new()
        {
            InternalAddChild(binding, name);
            return this;
        }
    }
}
