using System;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public sealed class KeyOutput : Model
    {
        public KeyOutput()
        {
        }

        public KeyOutput(Model model, bool addTempTableIdentity)
        {
            Utilities.Check.NotNull(model, nameof(model));
            var primaryKey = model.PrimaryKey;
            if (primaryKey == null)
                throw new ArgumentException(DiagnosticMessages.DbTable_NoPrimaryKey(model), nameof(model));

            _sourceDbAlias = model.DbAlias;
            _primaryKey = primaryKey.Clone(this);
            var sortKeys = new ColumnSort[primaryKey.Count];
            for (int i = 0; i < sortKeys.Length; i++)
                sortKeys[i] = primaryKey[i];
            AddDbTableConstraint(new DbPrimaryKey(this, null, null, false, () => { return sortKeys; }), true);
            if (addTempTableIdentity)
                this.AddTempTableIdentity();
        }


        private PrimaryKey _primaryKey;
        internal override PrimaryKey GetPrimaryKeyCore()
        {
            return _primaryKey;
        }

        private string _sourceDbAlias;

        protected internal override string DbAlias
        {
            get { return (GetIdentity(true) == null ? "sys_key_" : "sys_sequential_") + _sourceDbAlias; }
        }

        public static void BuildKeyMappings<T>(ColumnMappingsBuilder builder, T source, KeyOutput target)
            where T : Model, new()
        {
            var sourceKey = source.PrimaryKey;
            if (sourceKey == null)
                throw new InvalidOperationException(DiagnosticMessages.DbTable_NoPrimaryKey(source));

            var targetKey = target.PrimaryKey;
            if (targetKey == null)
                throw new InvalidOperationException(DiagnosticMessages.DbTable_NoPrimaryKey(target));

            if (targetKey.Count != sourceKey.Count)
                throw new InvalidOperationException(DiagnosticMessages.DbTable_GetKeyMappings_CannotMatch);

            for (int i = 0; i < targetKey.Count; i++)
            {
                var targetColumn = targetKey[i].Column;
                var sourceColumn = sourceKey[i].Column;
                if (targetColumn.DataType != sourceColumn.DataType)
                    throw new InvalidOperationException(DiagnosticMessages.DbTable_GetKeyMappings_CannotMatch);

                builder.Select(sourceColumn, i);
            }
        }
    }
}
