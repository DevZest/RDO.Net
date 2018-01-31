using System.Collections;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    public struct ValidationInfo
    {
        private sealed class DummyMessage : ValidationError
        {
            public static readonly DummyMessage Validating = new DummyMessage(UserMessages.Validation_Validating);
            public static readonly DummyMessage Validated = new DummyMessage(UserMessages.Validation_Validated);

            private DummyMessage(string message)
                : base(message)
            {
            }
        }

        private static IValidationErrors DummyValidatingMessage
        {
            get { return DummyMessage.Validating; }
        }

        private static IValidationErrors DummyValidatedMessage
        {
            get { return DummyMessage.Validated; }
        }

        internal static ValidationInfo Empty
        {
            get { return new ValidationInfo(); }
        }

        internal static ValidationInfo Validating
        {
            get { return new ValidationInfo(DummyValidatingMessage); }
        }

        internal static ValidationInfo Validated
        {
            get { return new ValidationInfo(DummyValidatedMessage); }
        }

        internal static ValidationInfo Error(IValidationErrors errors)
        {
            Debug.Assert(errors != null && errors.Count > 0);
            return new ValidationInfo(errors);
        }

        private ValidationInfo(IValidationErrors messages)
        {
            _messages = messages;
        }

        private readonly IValidationErrors _messages;
        public IValidationErrors Errors
        {
            get
            {
                if (_messages == null || _messages == DummyValidatingMessage || _messages == DummyValidatedMessage)
                    return ValidationErrors.Empty;
                return _messages;
            }
        }

        internal IEnumerable Messages
        {
            get { return _messages; }
        }

        public ValidationStatus? Status
        {
            get
            {
                if (_messages == null)
                    return null;
                else if (_messages == DummyValidatingMessage)
                    return ValidationStatus.Validating;
                else if (_messages == DummyValidatedMessage)
                    return ValidationStatus.Succeeded;
                else if (_messages.Count == 1 && _messages[0] is FlushingError)
                    return ValidationStatus.FailedFlushing;
                else
                    return ValidationStatus.Failed;
            }
        }
    }
}
