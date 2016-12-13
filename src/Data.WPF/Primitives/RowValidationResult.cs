using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.Windows.Primitives
{
    internal struct RowValidationResult
    {
        public static RowValidationResult Empty
        {
            get { return new RowValidationResult(); }
        }

        public static RowValidationResult New(IReadOnlyList<RowValidationEntry> entries)
        {
            return entries == null || entries.Count == 0 ? new RowValidationResult() : new RowValidationResult(entries);
        }

        private readonly IReadOnlyList<RowValidationEntry> _entries;
        public IReadOnlyList<RowValidationEntry> Entries
        {
            get { return _entries == null ? Array<RowValidationEntry>.Empty : _entries; }
        }

        private RowValidationResult(IReadOnlyList<RowValidationEntry> entries)
        {
            Debug.Assert(entries != null && entries.Count > 0);
            _entries = entries;
        }

        public bool IsEmpty
        {
            get { return Entries.Count == 0; }
        }
    }
}
