using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DevZest.Data
{
    public struct ValidationResult
    {
        public static ValidationResult Empty
        {
            get { return new ValidationResult(); }
        }

        public static ValidationResult New(IEnumerable<ValidationEntry> entries)
        {
            var array = entries == null ? null : entries.ToArray();
            return array == null || array.Length == 0 ? new ValidationResult() : new ValidationResult(array);
        }

        public static ValidationResult ParseJson(DataSet dataSet, string json)
        {
            throw new NotImplementedException();
        }

        private readonly IReadOnlyList<ValidationEntry> _entries;
        public IReadOnlyList<ValidationEntry> Entries
        {
            get { return _entries == null ? Array<ValidationEntry>.Empty : _entries; }
        }

        private ValidationResult(IReadOnlyList<ValidationEntry> entries)
        {
            Debug.Assert(entries != null && entries.Count > 0);
            _entries = entries;
        }

        public override string ToString()
        {
            return ToJsonString(true);
        }

        internal void WriteJson(StringBuilder stringBuilder)
        {
            stringBuilder.WriteArray(Entries, (sb, entry) => sb.WriteValidationEntryJson(entry));
        }

        public string ToJsonString(bool isPretty)
        {
            var stringBuilder = new StringBuilder();
            WriteJson(stringBuilder);
            var result = stringBuilder.ToString();
            return isPretty ? JsonFormatter.PrettyPrint(result) : result;
        }

        public bool IsValid
        {
            get { return Entries.Count == 0; }
        }
    }
}
