using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public abstract class Model<T> : Model
        where T : KeyBase
    {
        protected Model()
        {
            AddDbTableConstraint(new PrimaryKeyConstraint(this, null, true, () => PrimaryKey._columns), true);
        }

        public abstract new T PrimaryKey { get; }

        internal sealed override KeyBase GetPrimaryKeyCore()
        {
            return this.PrimaryKey;
        }

        public Join Join(Model<T> target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            return new Join(this, target);
        }

        public Join Join<TTarget>(TTarget target, Func<TTarget, T> keyGetter)
            where TTarget : Model, new()
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (keyGetter == null)
                throw new ArgumentNullException(nameof(keyGetter));

            var sourceKey = this.PrimaryKey;
            var targetKey = keyGetter(target);
            return new Join(sourceKey, targetKey);
        }
    }
}
