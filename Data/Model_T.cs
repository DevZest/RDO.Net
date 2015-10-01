using DevZest.Data.Primitives;
using DevZest.Data.Resources;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    public abstract class Model<T> : Model
        where T : ModelKey
    {
        protected Model()
        {
            AddDbTableConstraint(new PrimaryKeyConstraint(this, null, true, () => PrimaryKey._columns), true);
        }

        public abstract new T PrimaryKey { get; }

        internal sealed override ModelKey GetPrimaryKeyCore()
        {
            return this.PrimaryKey;
        }
    }
}
