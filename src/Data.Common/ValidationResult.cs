using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
            var jsonParser = new JsonParser(json);
            var result = jsonParser.ParseValidationResult(dataSet);
            jsonParser.ExpectToken(JsonTokenKind.Eof);
            return result;
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

        public string ToJsonString(bool isPretty)
        {
            return JsonWriter.New().Write(this).ToString(isPretty);
        }

        public bool IsValid
        {
            get { return !HasError; }
        }

        public bool HasError
        {
            get { return Any(ValidationSeverity.Error); }
        }

        public bool HasWarning
        {
            get { return Any(ValidationSeverity.Warning); }
        }

        private bool Any(ValidationSeverity severity)
        {
            return Entries.Any(x => Any(x.Messages, severity));
        }

        private static bool Any(IEnumerable<ValidationMessage> messages, ValidationSeverity severity)
        {
            return messages.Any(x => x.Severity == severity);
        }
    }
}
