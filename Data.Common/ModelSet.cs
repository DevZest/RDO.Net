using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data
{
    internal class ModelSet : HashSet<Model>, IModelSet
    {
        private class EmptyModelSet : IModelSet
        {
            public bool Contains(Model model)
            {
                return false;
            }

            public int Count
            {
                get { return 0; }
            }

            public IEnumerator<Model> GetEnumerator()
            {
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                yield break;
            }
        }

        public static readonly IModelSet Empty = new EmptyModelSet();

        public ModelSet()
        {
        }

        public ModelSet(IModelSet modelSet)
            : base(modelSet)
        {
        }
    }
}
