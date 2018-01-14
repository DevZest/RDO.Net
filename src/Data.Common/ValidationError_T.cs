namespace DevZest.Data
{
    public abstract class ValidationError<T> : ValidationError
    {
        protected ValidationError(string message, T source)
            : base(message)
        {
            _source = source;
        }

        private readonly T _source;
        public T Source
        {
            get { return _source; }
        }
    }
}
