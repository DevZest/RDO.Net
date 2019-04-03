using System.Collections;

namespace DevZest
{
    internal class SingleChildEnumerator : IEnumerator
    {
        private int _index = -1;
        private int _count;
        private object _child;

        object IEnumerator.Current
        {
            get
            {
                if (_index != 0)
                    return null;
                return _child;
            }
        }

        internal SingleChildEnumerator(object Child)
        {
            _child = Child;
            _count = ((Child == null) ? 0 : 1);
        }

        bool IEnumerator.MoveNext()
        {
            _index++;
            return _index < _count;
        }

        void IEnumerator.Reset()
        {
            _index = -1;
        }
    }
}
