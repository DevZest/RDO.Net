using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public abstract class Model<T> : Model
        where T : PrimaryKey
    {
        protected Model()
        {
            AddDbTableConstraint(new PrimaryKeyConstraint(this, null, true, () => PrimaryKey._columns), true);
        }

        public abstract new T PrimaryKey { get; }

        internal sealed override PrimaryKey GetPrimaryKeyCore()
        {
            return this.PrimaryKey;
        }
    }
}
