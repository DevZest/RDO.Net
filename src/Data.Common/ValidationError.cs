using DevZest.Data.Utilities;

namespace DevZest.Data
{
    public abstract class ValidationError
    {
        protected ValidationError(string message)
        {
            Check.NotEmpty(message, nameof(message));

            _message = message;
        }

        private readonly string _message;
        public string Message
        {
            get { return _message; }
        }

        public override string ToString()
        {
            return Message;
        }
    }
}
