using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;

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

        public static void BuildKeyMappings(ColumnMapper builder, Model source, KeyOutput target)
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

                builder.Map(sourceColumn, i);
            }
        }

        public IReadOnlyList<ColumnMapping> MapTo(PrimaryKey key)
        {
            Check.NotNull(key, nameof(key));

            var sourceKey = PrimaryKey;
            if (sourceKey.Count != key.Count)
                throw new ArgumentException(DiagnosticMessages.KeyOutput_MapTo_ColumnsCountMismatch, nameof(key));
            var result = new ColumnMapping[key.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var sourceColumn = sourceKey[i].Column;
                var targetColumn = key[i].Column;
                if (sourceColumn.DataType != targetColumn.DataType)
                    throw new ArgumentException(DiagnosticMessages.KeyOutput_MapTo_ColumnDataTypeMismatch, string.Format("{0}[{1}]", nameof(key), i));
                result[i] = new ColumnMapping(sourceColumn, targetColumn);
            }
            return result;
        }

        public IReadOnlyList<ColumnMapping> MapTo(IReadOnlyList<Column> targetColumns)
        {
            Check.NotNull(targetColumns, nameof(targetColumns));
            var sourceKey = PrimaryKey;
            if (sourceKey.Count != targetColumns.Count)
                throw new ArgumentException(DiagnosticMessages.KeyOutput_MapTo_ColumnsCountMismatch, nameof(targetColumns));

            var result = new ColumnMapping[targetColumns.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var sourceColumn = sourceKey[i].Column;
                var targetColumn = targetColumns[i];
                if (sourceColumn.DataType != targetColumn.DataType)
                    throw new ArgumentException(DiagnosticMessages.KeyOutput_MapTo_ColumnDataTypeMismatch, string.Format("{0}[{1}]", nameof(targetColumns), i));
                result[i] = new ColumnMapping(sourceColumn, targetColumn);
            }
            return result;
        }
    }
}
