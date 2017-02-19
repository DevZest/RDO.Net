using System;
using DevZest.Data.Windows.Primitives;

namespace DevZest.Data.Windows
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
    }
}
