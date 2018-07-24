using System.Diagnostics;

namespace DevZest.Data
{
    public abstract class Key<T> : Projection
        where T : PrimaryKey
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
                return _key.PrimaryKey;
            }

            internal override bool IsProjectionContainer
            {
                get { return true; }
            }
        }


        protected abstract T GetPrimaryKey();

        private T _primaryKey;
        public T PrimaryKey
        {
            get { return _primaryKey ?? (_primaryKey = GetPrimaryKey()); }
        }

        internal override void EnsureConstructed()
        {
            if (ParentModel == null)
            {
                var containerModel = new ContainerModel(this);
                Debug.Assert(ParentModel == containerModel);
            }
        }

        public KeyMapping Match(T target)
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(PrimaryKey, target);
        }

        public KeyMapping Match(Model<T> target)
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(PrimaryKey, target.PrimaryKey);
        }

        public KeyMapping Match(Key<T> target)
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(PrimaryKey, target.PrimaryKey);
        }
    }
}
