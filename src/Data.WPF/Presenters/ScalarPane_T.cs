using System;
using DevZest.Data.Presenters.Primitives;
using System.Windows;
using DevZest.Data.Views;

namespace DevZest.Data.Presenters
{
    public sealed class ScalarPane<T> : ScalarPane
        where T : Pane, new()
    {
        internal override Pane CreatePane()
        {
            return new T();
        }

        public new T this[int flowIndex]
        {
            get { return (T)base[flowIndex]; }
        }

        public ScalarPane<T> AddChild<TChild>(ScalarBinding<TChild> binding, string name)
            where TChild : UIElement, new()
        {
            InternalAddChild(binding, name);
            return this;
        }
    }
}
