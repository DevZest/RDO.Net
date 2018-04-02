using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    public struct KeyMapping
    {
        public KeyMapping(PrimaryKey sourceKey, PrimaryKey targetKey)
        {
            Check.NotNull(sourceKey, nameof(sourceKey));
            Check.NotNull(targetKey, nameof(targetKey));
            if (targetKey.GetType() != sourceKey.GetType())
                throw new ArgumentException(DiagnosticMessages.KeyMapping_SourceTargetTypeMismatch, nameof(targetKey));
            Debug.Assert(sourceKey.Count == targetKey.Count);
            _sourceKey = sourceKey;
            _targetKey = targetKey;
        }

        private readonly PrimaryKey _sourceKey;
        public PrimaryKey SourceKey
        {
            get { return _sourceKey; }
        }

        private readonly PrimaryKey _targetKey;
        public PrimaryKey TargetKey
        {
            get { return _targetKey; }
        }

        public IReadOnlyList<ColumnMapping> GetColumnMappings()
        {
            if (IsEmpty)
                return null;

            var result = new ColumnMapping[_targetKey.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var targetColumn = _targetKey[i].Column;
                var sourceColumn = _sourceKey[i].Column;
                Debug.Assert(targetColumn.DataType == sourceColumn.DataType);
                result[i] = new ColumnMapping(sourceColumn, targetColumn);
            }
            return result;
        }

        public bool IsEmpty
        {
            get { return _sourceKey == null; }
        }

        public Model SourceModel
        {
            get { return _sourceKey?.ParentModel; }
        }

        public Model TargetModel
        {
            get { return _targetKey?.ParentModel; }
        }
    }
}
