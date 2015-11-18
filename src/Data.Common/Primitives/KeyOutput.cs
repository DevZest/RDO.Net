using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    internal sealed class KeyOutput : Model
    {
        public KeyOutput()
        {
        }

        internal KeyOutput Initialize(Model model, bool addTempTableIdentity)
        {
            _sourceDbAlias = model.DbAlias;

            var primaryKey = model.PrimaryKey;
            Debug.Assert(primaryKey != null);

            _primaryKey = primaryKey.Clone(this);
            var sortKeys = new ColumnSort[primaryKey.Count];
            for (int i = 0; i < sortKeys.Length; i++)
                sortKeys[i] = primaryKey[i];
            AddDbTableConstraint(new PrimaryKeyConstraint(this, null, false, () => { return sortKeys; }), true);
            if (addTempTableIdentity)
                this.AddTempTableIdentity();

            return this;
        }


        private ModelKey _primaryKey;
        internal override ModelKey GetPrimaryKeyCore()
        {
            return _primaryKey;
        }

        private string _sourceDbAlias;

        protected internal override string DbAlias
        {
            get { return (GetIdentity(true) == null ? "sys_key_" : "sys_sequential_") + _sourceDbAlias; }
        }
    }
}
