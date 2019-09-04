namespace DevZest.Data
{
    /// <summary>
    /// Base class for data validation error with strongly typed source.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ValidationError<T> : ValidationError
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ValidationError{T}"/> class.
        /// </summary>
        /// <param name="message">The validation error message.</param>
        /// <param name="source">The source of the validation error.</param>
        protected ValidationError(string message, T source)
            : base(message)
        {
            _source = source;
        }

        private readonly T _source;
        /// <summary>
        /// Gets the source of the validation error.
        /// </summary>
        public T Source
        {
            get { return _source; }
        }
    }
}
