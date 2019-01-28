using DevZest.Data.Addons;
using System;

namespace DevZest.Data.Primitives
{
    public class KeyOutput : Model
    {
        private sealed class PK : CandidateKey
        {
            public PK(KeyOutput keyOutput, CandidateKey primaryKey)
                : base(GetColumns(keyOutput, primaryKey))
            {
            }

            private static ColumnSort[] GetColumns(KeyOutput _, CandidateKey primaryKey)
            {
                var result = new ColumnSort[primaryKey.Count];
                for (int i = 0; i < primaryKey.Count; i++)
                {
                    var columnSort = primaryKey[i];
                    result[i] = new ColumnSort(columnSort.Column.Clone(_), columnSort.Direction);
                }
                return result;
            }
        }

        public KeyOutput()
        {
        }

        public KeyOutput(Model model)
        {
            model.VerifyNotNull(nameof(model));
            var primaryKey = model.PrimaryKey;
            if (primaryKey == null)
                throw new ArgumentException(DiagnosticMessages.DbTable_NoPrimaryKey(model), nameof(model));

            _sourceDbAlias = model.DbAlias;
            _primaryKey = new PK(this, primaryKey);
            var sortKeys = new ColumnSort[primaryKey.Count];
            for (int i = 0; i < sortKeys.Length; i++)
                sortKeys[i] = primaryKey[i];
            AddDbTableConstraint(new DbPrimaryKey(this, null, null, false, () => { return sortKeys; }), true);
        }


        private readonly CandidateKey _primaryKey;
        internal override CandidateKey GetPrimaryKeyCore()
        {
            return _primaryKey;
        }

        private readonly string _sourceDbAlias;

        protected virtual string DbAliasPrefix
        {
            get { return "sys_key_"; }
        }

        protected internal override string DbAlias
        {
            get { return DbAliasPrefix  + _sourceDbAlias; }
        }

        public static void BuildKeyMappings(ColumnMapper mapper, Model source, KeyOutput target)
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

                mapper.Select(sourceColumn, i);
            }
        }
    }
}
