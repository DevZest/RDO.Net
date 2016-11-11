using System;
using System.Collections.Generic;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ValidationManager : ElementManager
    {
        protected ValidationManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, bool emptyBlockViewList)
            : base(template, dataSet, where, orderBy, emptyBlockViewList)
        {
        }

        internal Input FlushingInput { get; set; }

        private Dictionary<Input, ValidationMessage> _flushingErrors = new Dictionary<Input, ValidationMessage>();
        private IReadOnlyList<ValidationMessage> _validationSummary = Array<ValidationMessage>.Empty;
        private Dictionary<Input, IReadOnlyList<ValidationMessage>> _validationMessages = new Dictionary<Input, IReadOnlyList<ValidationMessage>>();
        private IReadOnlyList<ValidationMessage> _mergedValidaitonSummary = Array<ValidationMessage>.Empty;
        private Dictionary<Input, IReadOnlyList<ValidationMessage>> _mergedValidationMessages = new Dictionary<Input, IReadOnlyList<ValidationMessage>>();

        internal bool HasErrors(Input input)
        {
            return _flushingErrors.ContainsKey(input) || _validationMessages.ContainsKey(input) || _mergedValidationMessages.ContainsKey(input);
        }

        internal IEnumerable<ValidationMessage> GetErrors(Input input)
        {
            {
                ValidationMessage flushingError;
                if (_flushingErrors.TryGetValue(input, out flushingError))
                    yield return flushingError;
            }

            {
                IReadOnlyList<ValidationMessage> validationMessages;
                if (_validationMessages.TryGetValue(input, out validationMessages))
                {
                    foreach (var validationMessage in validationMessages)
                        yield return validationMessage;
                }
            }

            {
                IReadOnlyList<ValidationMessage> mergedValidationMessages;
                if (_mergedValidationMessages.TryGetValue(input, out mergedValidationMessages))
                {
                    foreach (var validationMessage in mergedValidationMessages)
                        yield return validationMessage;
                }
            }
        }

        public bool IsValidated { get; private set; }

        public bool IsValid(bool ignoreMergedResult = true)
        {
            if (_flushingErrors.Count > 0)
                return false;

            throw new NotImplementedException();
        }

        public bool Validate(bool ignoreMergedResult = true)
        {
            IsValidated = true;
            throw new NotImplementedException();
        }
    }
}
