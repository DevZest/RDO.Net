using System;

namespace DevZest.Data.Windows
{
    public struct ReverseBindingMessage
    {
        public static ReverseBindingMessage Empty = new ReverseBindingMessage();
        public static ReverseBindingMessage Error(string message)
        {
            return new ReverseBindingMessage(string.Empty, message, ValidationSeverity.Error);
        }

        public static ReverseBindingMessage Error(string messageId, string message)
        {
            return new ReverseBindingMessage(messageId, message, ValidationSeverity.Error);
        }

        public static ReverseBindingMessage Warning(string message)
        {
            return new ReverseBindingMessage(string.Empty, message, ValidationSeverity.Warning);
        }

        public static ReverseBindingMessage Warning(string messageId, string message)
        {
            return new ReverseBindingMessage(messageId, message, ValidationSeverity.Warning);
        }

        public readonly string MessageId;
        public readonly string Message;
        public readonly ValidationSeverity Severity;

        private ReverseBindingMessage(string messageId, string message, ValidationSeverity severity)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));
            MessageId = messageId;
            Message = message;
            Severity = severity;
        }

        public bool IsEmpty
        {
            get { return Message == null; }
        }
    }
}
