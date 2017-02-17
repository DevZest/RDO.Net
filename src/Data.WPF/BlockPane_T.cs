using System;
using DevZest.Data.Windows.Primitives;

namespace DevZest.Data.Windows
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
    }
}
