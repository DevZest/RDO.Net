using DevZest.Data.Primitives;
using System.Diagnostics;
using System;

namespace DevZest.Data
{
    internal sealed class SequentialKeyModel : Model
    {
        public SequentialKeyModel()
        {
            Debug.Fail("This class is for internal use only. This constructor should not be called.");
        }

        public SequentialKeyModel(Model model)
        {
            _dbAlias = "sys_sequential_" + model.DbAlias;

            var primaryKey = model.PrimaryKey;
            Debug.Assert(primaryKey != null);

            _primaryKey = primaryKey.Clone(this);
            var sortKeys = new ColumnSort[primaryKey.Count];
            for (int i = 0; i < sortKeys.Length; i++)
                sortKeys[i] = primaryKey[i];
            AddDbTableConstraint(new PrimaryKeyConstraint(this, null, false, () => { return sortKeys; }), true);
            this.AddTempTableIdentity();
        }


        private ModelKey _primaryKey;
        internal override ModelKey GetPrimaryKeyCore()
        {
            return _primaryKey;
        }
        private string _dbAlias;
        protected internal override string DbAlias
        {
            get { return _dbAlias; }
        }
    }
}
