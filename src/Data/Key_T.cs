using DevZest.Data.Annotations.Primitives;
using System.Diagnostics;

namespace DevZest.Data
{
    /// <summary>
    /// Represents model projection which contains primary key columns only, with unique constraint enforced.
    /// </summary>
    /// <typeparam name="T">Type of the primary key.</typeparam>
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

        /// <summary>
        /// Creates the primary key.
        /// </summary>
        /// <returns>The created primary key.</returns>
        [CreateKey]
        protected abstract T CreatePrimaryKey();

        private ContainerModel _containerModel;

        /// <summary>
        /// Gets the associated model.
        /// </summary>
        public Model<T> Model => _containerModel;

        /// <summary>
        /// Gets the primary key.
        /// </summary>
        public T PrimaryKey => Model.PrimaryKey;

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
