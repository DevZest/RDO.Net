using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    /// <summary>
    /// Represents mapping between two keys.
    /// </summary>
    public struct KeyMapping
    {
        internal static KeyMapping Match<TSource, TTarget>(TSource source, TTarget target)
            where TSource : TTarget
            where TTarget : IEntity
        {
            return new KeyMapping(source.Model.PrimaryKey, target.Model.PrimaryKey);
        }

        internal KeyMapping(CandidateKey sourceKey, CandidateKey targetKey)
        {
            sourceKey.VerifyNotNull(nameof(sourceKey));
            targetKey.VerifyNotNull(nameof(targetKey));
            if (targetKey.GetType() != sourceKey.GetType())
                throw new ArgumentException(DiagnosticMessages.KeyMapping_SourceTargetTypeMismatch, nameof(targetKey));
            Debug.Assert(sourceKey.Count == targetKey.Count);
            _sourceKey = sourceKey;
            _targetKey = targetKey;
        }

        private readonly CandidateKey _sourceKey;
        /// <summary>
        /// Gets the source key of the mapping.
        /// </summary>
        public CandidateKey SourceKey
        {
            get { return _sourceKey; }
        }

        private readonly CandidateKey _targetKey;
        /// <summary>
        /// Gets the target key of the mapping.
        /// </summary>
        public CandidateKey TargetKey
        {
            get { return _targetKey; }
        }

        /// <summary>
        /// Gets the column mappings.
        /// </summary>
        /// <returns>A readonly list of <see cref="ColumnMapping"/> objects.</returns>
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

        /// <summary>
        /// Gets a value indicates whether this key mapping is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return _sourceKey == null; }
        }

        /// <summary>
        /// Gets the source model.
        /// </summary>
        public Model SourceModel
        {
            get { return _sourceKey?.ParentModel; }
        }

        /// <summary>
        /// Gets the target model.
        /// </summary>
        public Model TargetModel
        {
            get { return _targetKey?.ParentModel; }
        }
    }
}
