using System;
using DevZest.Data.Windows.Primitives;

namespace DevZest.Data.Windows
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
    }
}
