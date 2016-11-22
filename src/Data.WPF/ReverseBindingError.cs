namespace DevZest.Data.Windows
{
    public struct ReverseBindingError
    {
        public static ReverseBindingError Empty = new ReverseBindingError();

        public readonly string MessageId;
        public readonly string Message;

        public ReverseBindingError(string message)
            : this(string.Empty, message)
        {
        }

        public ReverseBindingError(string messageId, string message)
        {
            MessageId = messageId;
            Message = message;
        }
    }
}
