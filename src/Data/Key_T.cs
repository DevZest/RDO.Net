using DevZest.Data.Annotations.Primitives;
using System.Diagnostics;

namespace DevZest.Data
{
    public abstract class Key<T> : Projection, IEntity<T>
        where T : CandidateKey
    {
        private sealed class ContainerModel : Model<T>
        {
            public ContainerModel(Key<T> key)
            {
                Debug.Assert(key != null);
                Debug.Assert(key.ParentModel == null);
                key.Construct(this, GetType(), string.Empty);
                Add(key);
                _key = key;
            }

            private readonly Key<T> _key;

            protected override T CreatePrimaryKey()
            {
                return _key.CreatePrimaryKey();
            }

            internal override bool IsProjectionContainer
            {
                get { return true; }
            }
        }

        [CreateKey]
        protected abstract T CreatePrimaryKey();

        private ContainerModel _containerModel;

        public Model<T> Model => _containerModel;

        internal override void EnsureConstructed()
        {
            if (ParentModel == null)
            {
                _containerModel = new ContainerModel(this);
                Debug.Assert(ParentModel == _containerModel);
            }
        }
    }
}
