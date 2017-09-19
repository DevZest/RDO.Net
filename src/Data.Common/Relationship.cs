using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    public sealed class Relationship
    {
        internal static Relationship Create<T>(T source, T target)
            where T : KeyBase
        {
            return new Data.Relationship(source, target);
        }

        private Relationship(KeyBase source, KeyBase target)
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

        private ColumnMapping[] _mappings;
        public IReadOnlyList<ColumnMapping> Mappings
        {
            get
            {
                if (_mappings == null)
                    _mappings = GetMappings();
                return _mappings;
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

        public Relationship Swap()
        {
            return new Relationship(_target, _source);
        }
    }
}
