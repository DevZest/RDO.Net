using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    internal sealed class Join
    {
        internal Join(Model source, Model target)
            : this(source.PrimaryKey, target.PrimaryKey)
        {
        }

        internal Join(KeyBase source, KeyBase target)
        {
            Debug.Assert(source != null);
            Debug.Assert(target != null);
            Debug.Assert(source.Count == target.Count);
            _source = source;
            _target = target;
        }

        private readonly KeyBase _source;
        public KeyBase Source
        {
            get { return _source; }
        }

        private readonly KeyBase _target;
        public KeyBase Target
        {
            get { return _target; }
        }

        private ColumnMapping[] _relationship;
        public IReadOnlyList<ColumnMapping> Relationship
        {
            get
            {
                if (_relationship == null)
                    _relationship = GetMappings();
                return _relationship;
            }
        }

        private ColumnMapping[] GetMappings()
        {
            var result = new ColumnMapping[_source.Count];
            for (int i = 0; i < _source.Count; i++)
            {
                var sourceColumn = _source[i].Column;
                var targetColumn = _target[i].Column;
                result[i] = new ColumnMapping(sourceColumn, targetColumn);
            }

            return result;
        }

        public Join Swap()
        {
            return new Join(_target, _source);
        }
    }
}
